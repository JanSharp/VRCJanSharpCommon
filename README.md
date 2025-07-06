
# JanSharp Common

Common files and scripts are, well, commonly used across multiple other scripts. It's like the core.

# Installing

Head to my [VCC Listing](https://jansharp.github.io/vrc/vcclisting.xhtml) and follow the instructions there.

# Features

## Editor

- OnBuildUtil: Allowing multiple registrations and a defined order
  - In an editor script, mark a class with the [InitializeOnLoad](https://docs.unity3d.com/ScriptReference/InitializeOnLoadAttribute.html) attribute
  - In its static constructor call `OnBuildUtil.RegisterType<T>(...)` passing in a function with type T as a parameter and returning a boolean
  - The registered callbacks will run on every component of the registered type T and any types deriving from T in the current scene when entering play mode and when VRChat builds the project
  - If your callback returns false it indicates failure and prevents VRChat from publishing. Entering play mode does not get aborted however
    - You should always `Debug.LogError` the reason for the abort
  - Since the RegisterType method takes an optional order argument, it is supported to register the same type twice but with different order
    - The order means that all lowest order registrations for all types are handled first, then the next order and so on
  - Enable the `JAN_SHARP_COMMON_ON_BUILD_TRACE` define to see which on build callbacks are being run. Can help for example when something is infinitely looping and therefore freezing the editor (Edit -> Project Settings... -> Player -> Scripting Define Symbols)
- EditorUtil: Helps working with SerializedObjects and SerializedProperties and other editor only utils
- OnAssemblyLoadUtil: A generic way for other editor scripts to obtain a list of all UdonSharpBehaviour classes in the entire project

Some [Library](#libraries)/[Manager](#managers) features are assisted by editor scripting:

- BuildTimeIdAssignmentEditor
- CustomRaisedEventsEditor
- SingletonScriptEditor
- WannaBeClassesEditor

## Runtime

### Components

- LocalEventOnInteract to send a custom event to any Udon behaviour locally
- LocalToggleOnInteract to toggle a single GameObject locally
- LocalToggleMultipleOnInteract to toggle multiple GameObjects locally
- LocalToggleOnValueChanged to toggle multiple objects to match the state of a UI Toggle
- InteractProxy to pass VRChat's interact event from one object to another. Useful when the interact is on an object that gets toggled, but the script should not get toggled
- UIToggleInteractProxy to send an Interact event whenever a UI Toggle gets turned on, turned off or both. Allows for generic OnInteract scripts to be hooked up to toggles. For that to makes sense it usually requires a pair of OnInteract scripts, one for turning on one for turning off
- UIToggleSendLocalEvent is very similar to UIToggleInteractProxy except that it sends a given custom event to the target udon behaviour instead of only supporting interacts
- UIToggleSync to make synced toggles without writing scripts for them every time
- UIToggleGroupSync to make synced toggle groups without writing scripts for them every time, as well as making it a little easier to setup toggle groups through the inspector
- PickupSendLocalEvent to send events when a VRCPickup is picked up, dropped or used while held
- BypassSingletonDependencyInstantiation: Prevents any script on this object or any of its children to trigger the singleton editor scripting to instantiate required prefabs. It effectively changes all dependencies of those scripts from required to optional

### Managers

Managers are singleton scripts with associated prefabs. When there are any singleton references in any scripts in a scene, these prefabs get automatically instantiated into the scene, thanks to the singleton editor scripting which is also part of this package.

- BoneAttachmentManager: To simplify the process of attaching any game object to bones of players, or tracking data for local players
- SingletonManager: A manager which is a singleton itself which holds references to all singleton scripts which exist in the scene. See [Libraries](#libraries) for details about singletons
- TrulyPostLateUpdateManager
- UpdateManager: Register and deregister any behaviour for a CustomUpdate function at runtime (runs on Update)
  - Udon behaviours used with this require a `CustomUpdate` event (public method) and an int variable `customUpdateInternalIndex`
  - Registering an already registered behaviour is supported, it does nothing, same goes for deregistering an already deregistered behaviour
  - Note that I'm unsure how performance compares to using `SendCustomEventDelayedFrames` to effectively create an update loop
- WannaBeClassesManager: A manager to enable creating instances of UdonSharpBehaviours which are pretending to be custom data structures. See [WannaBeClasses](#wannabeclasses) for details
- InterpolationManager: A manager to linearly interpolate the position, rotation and or scale of objects, with callbacks once done
- QuickDebugUI: Display/Monitor some real time values in a screen space overlay UI (Desktop UI) with relatively minimal setup

### Libraries

- ArrList: Makes it easier to make list like arrays. They are statically typed, unlike DataLists
- ArrQueue: Makes it easier to make queue like arrays. First in, first out
- Base64: Validate base 64 encoded strings to prevent throwing exceptions on invalid input
- BuildTimeIdAssignment: Associates ids with custom script instances in the scene at build time, while ensuring an id never gets reused, even ids for deleted script instances do not get reused
  - The BypassBuildTimeIdAssignment component can be used to mark an object and its children to be ignored by this system, meaning they will not get ids assigned to them
- CRC32: Calculate a crc 32 checksum of a byte array
- CustomRaisedEvents: Helps creating scripts, likely singletons, which raise events other scripts may listen to. Uses attributes and enums
- DataStream: For binary serialization and deserialization using byte arrays
- SingletonScript: Attributes to
  - Define a script as being a singleton, it only being allowed to exist once in the scene
  - Define prefabs of singletons which automatically get instantiated into the scene at build time if other scripts depend on this singleton
  - Define a field as a reference to a singleton script which gets set by editor scripting automatically at build time
- WannaBeArrList: An extension of ArrList, however holding strong references to WannaBeClass instances
- WannaBeArrQueue: An extension of ArrQueue, however holding strong references to WannaBeClass instances
- WannaBeClass: These are an unfortunately complex concept so they get [their own section](#wannabeclasses)
- ArithBitConverter: Deprecated, was useful when BitConverter wasn't exposed yet. Uses arithmetics to convert floating point numbers to their binary representation and back

### WannaBeClasses

Udon doesn't have custom data structures. This is a major restriction and annoyance in many cases. The WannaBeClass concept attempts to work around this restriction by instantiating a GameObject with an UdonSharpBehaviour and pretending like its an instance of a custom class. The downside is that instantiating UdonBehaviours is stupidly slow, even small scripts take over 1 millisecond, and deletion of these "custom class instances" doesn't happen automatically when all references to them are lost, since they are actual objects in the scene. Therefore these classes effectively require manual memory management to prevent memory leaks.

In order to make this manual memory management somewhat less annoying WannaBeClasses keep a reference counter. This counter starts at 1 when the instance gets created, it may be incremented and decremented, and when it hits 0 it is going to delete itself.

There are 2 different kinds of references to WannaBeClasses: **strong** and **weak**. Anything that holds a **strong** reference also increments the reference counter of the WannaBeClass, and must decrement the reference counter when it loses the reference to the WannaBeClass. **Weak** references do not touch the reference counter at all, which also means that the referenced WannaBeClass may get deleted while a weak reference to it was still held. The reference turns null at that point.

Sometimes something may hold a strong reference to a WannaBeClass and wish to pass this "ownership" to another function or script. For this there is a `StdMove` function on WannaBeClasses, the name is inspired by C++ move semantics. All it does is decrement the reference counter, however it does not immediately check if the counter reached 0. The receiving code shall then increment the reference counter, which is effectively it taking ownership of this WannaBeClass instance. If the reference counter does not get incremented, the instance gets deleted 1 frame later automatically, or until some code calls `CheckLiveliness`.

There's support for WannaBeClass instances to already exist in the scene. They'll get moved out of their original location in the hierarchy at build time, making them managed by the WannaBeClassesManager. They also start at a reference counter of 1.

### Textures

Some UI sprites for UI without rounded corners which I use across other packages. Effectively my UI style, so it doesn't look like plain default unity UI. These sprites have same anti aliasing issues, but I cannot figure out how to fix that and I don't deem it to be bad enough to worry about.

# Licensed Third Party Code

A big thank you to all the open source contributors out there! This project uses code from:

- [crc32](https://github.com/stbrumme/crc32), Copyright (c) 2011-2016 Stephan Brumme
