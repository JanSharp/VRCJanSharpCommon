
# JanSharp Common

Common files and scripts are, well, commonly used across multiple other scripts. It's like the core.

# Installing

Head to my [VCC Listing](https://jansharp.github.io/vrc/vcclisting.xhtml) and follow the instructions there.

# Features

## Editor

<!-- cSpell:ignore occluders, occludees -->

- OnBuildUtil, allowing multiple registrations and a defined order
  - In an editor script, mark a class with the [InitializeOnLoad](https://docs.unity3d.com/ScriptReference/InitializeOnLoadAttribute.html) attribute
  - In its static constructor call `OnBuildUtil.RegisterType<T>(...)` passing in a function with type T as a parameter and returning a boolean
  - The registered callbacks will run on every component of the registered type T and any types deriving from T in the current scene when entering play mode and when VRChat builds the project
  - If your callback returns false it indicates failure and prevents VRChat from publishing. Entering play mode does not get aborted however
    - You should always `Debug.LogError` the reason for the abort
  - Since the RegisterType method takes an optional order argument, it is supported to register the same type twice but with different order
    - The order means that all lowest order registrations for all types are handled first, then the next order and so on
- EditorUtil to help working with SerializedObjects and SerializedProperties and other editor only utils

## Runtime

- UpdateManager to register and deregister any behaviour for a CustomUpdate function at runtime (runs on Update)
  - Udon behaviours used with this require a `CustomUpdate` event (public method) and an int variable `customUpdateInternalIndex`
  - Registering an already registered behaviour is supported, it does nothing, same goes for deregistering an already deregistered behaviour
- UpdateManager prefab
- LocalEventOnInteract to send a custom event to any Udon behaviour locally
- LocalToggleOnInteract to toggle a single GameObject locally
- LocalToggleOnValueChanged to toggle multiple objects to match the state of a UI Toggle
- InteractProxy to pass VRChat's interact event from one object to another. Useful when the interact is on an object that gets toggled, but the script should not get toggled
- ArrList to make it easier to make list like arrays. They are statically typed, unlike DataLists
- ArrQueue to make it easier to make queue like arrays. First in, first out
- ArithBitConverter for floating point to and from binary conversions, because we don't have BitConverter - note that we do have BitConverter in the latest version of Udon now, but if you're still on unity 2019 then you don't. However there is a chance that this gets removed in the future
- DataStream for binary serialization and deserialization using byte arrays. Uses ArrList and ArithBitConverter - consider this experimental
- CRC32 for byte arrays
- Base64 conversion between byte arrays and strings, specifically to be able to attempt decoding without throwing exceptions on invalid input
- UIToggleInteractProxy to send an Interact event whenever a UI Toggle gets turned on, turned off or both. Allows for generic OnInteract scripts to be hooked up to toggles. For that to makes sense it usually requires a pair of OnInteract scripts, one for turning on one for turning off
- UIToggleSendLocalEvent is very similar to UIToggleInteractProxy except that it sends a given custom event to the target udon behaviour instead of only supporting interacts
- UIToggleSync to make synced toggles without writing scripts for them every time
- UIToggleGroupSync to make synced toggle groups without writing scripts for them every time, as well as making it a little easier to setup toggle groups through the inspector
- PickupSendLocalEvent to send events when a VRCPickup is picked up, dropped or used while held

# Ideas

Honestly I'm not sure. It feels like this is missing a lot of stuff, but then I'm not sure what really belongs in here. But I suppose the editor scripting this provides is already decent. But still, I'm not marking it as `1.0.0` yet.

# Licensed Third Party Code

A big thank you to all the open source contributors out there!

- [crc32](https://github.com/stbrumme/crc32), Copyright (c) 2011-2016 Stephan Brumme
