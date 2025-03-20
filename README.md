
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
- WannaBeClassesManager: A manager to enable creating instances of UdonSharpBehaviours which are pretending to be custom data structures. See [Libraries](#libraries) for details about WannaBeClasses

### Libraries

- ArrList: Makes it easier to make list like arrays. They are statically typed, unlike DataLists
- ArrQueue: Makes it easier to make queue like arrays. First in, first out
- Base64: Validate base 64 encoded strings to prevent throwing exceptions on invalid input
- BuildTimeIdAssignment: Associates ids with custom script instances in the scene at build time, while ensuring an id never gets reused, even after deletion of scripts with associated ids
- CRC32: Calculate a crc 32 checksum of a byte array
- CustomRaisedEvents: Helps creating scripts, likely singletons, which raise events other scripts may listen to. Uses attributes and enums
- DataStream: For binary serialization and deserialization using byte arrays
- SingletonScript: Attributes to
  - Define a script as being a singleton, it only being allowed to exist once in the scene
  - Define prefabs of singletons which automatically get instantiated into the scene at build time if other scripts depend on this singleton
  - Define a field as a reference to a singleton script which gets set by editor scripting automatically at build time
- WannaBeArrList: An extension of ArrList, however holding strong references to WannaBeClass instances
- WannaBeArrQueue: An extension of ArrQueue, however holding strong references to WannaBeClass instances
- WannaBeClass: Udon is a great language and it doesn't have custom data structures. This is a utility to instantiate objects with an UdonSharpBehaviour and pretend like its an instance of a custom class. The downside is that instantiating UdonBehaviours is stupidly slow, even small scripts take over 1 millisecond, and deletion of these "custom class instances" doesn't happen automatically. Therefore these classes effectively require manual memory management to prevent memory leaks. There's support for these class instances to already exist in the scene and they'll get moved out of that location in the hierarchy at build time, making them managed by the WannaBeClassesManager
- ArithBitConverter: Deprecated, was useful when BitConverter wasn't exposed yet. Uses arithmetics to convert floating point numbers to their binary representation and back

### Textures

Some UI sprites for UI without rounded corners which I use across other packages. Effectively my UI style, so it doesn't look like plain default unity UI. These sprites have same anti aliasing issues, but I cannot figure out how to fix that and I don't deem it to be bad enough to worry about.

# Ideas

Honestly I'm not sure. It feels like this is missing a lot of stuff, but then I'm not sure what really belongs in here. But I suppose the editor scripting this provides is already decent. But still, I'm not marking it as `1.0.0` yet.

# Licensed Third Party Code

A big thank you to all the open source contributors out there!

- [crc32](https://github.com/stbrumme/crc32), Copyright (c) 2011-2016 Stephan Brumme
