
# Changelog

## [1.1.0] - 2025-07-06

### Changed

- **Breaking:** Make OnBuildUtil ignore EditorOnly objects by default ([`cf92b84`](https://github.com/JanSharp/VRCJanSharpCommon/commit/cf92b843fa5379b6b06b434198e4067035c4ae15))
- Make UIToggleGroupSync work with potentially disabled UI Toggles ([`b4e7da2`](https://github.com/JanSharp/VRCJanSharpCommon/commit/b4e7da2be6301cab80230bb565fc22c075105b26))
- Change UIToggleSync and UIToggleGroupSync Toggle listener adding/removing to be automatic using OnBuildUtil ([`b4e7da2`](https://github.com/JanSharp/VRCJanSharpCommon/commit/b4e7da2be6301cab80230bb565fc22c075105b26), [`2bee174`](https://github.com/JanSharp/VRCJanSharpCommon/commit/2bee174b462edfae261ad69874b483f59d59940f))
- Make WannaBeClass instances that already exist in the scene run WannaBeConstructor on Start ([`304056e`](https://github.com/JanSharp/VRCJanSharpCommon/commit/304056edc4be75e2db2e08ccb85c395536759fd2))
- Make CustomRaisedEvents validation run OnBuild to avoid missing errors that were shown upon assembly reload ([`cc0e8ae`](https://github.com/JanSharp/VRCJanSharpCommon/commit/cc0e8ae9455b5608f9497dde937f854001b8e294))
- Add validation for singleton prefabs actually containing the associated script ([`0788210`](https://github.com/JanSharp/VRCJanSharpCommon/commit/078821048f972a9e5223c9178fa9fb7e892c534f))
- Improve ArrQueue array growth performance ([`9774fc6`](https://github.com/JanSharp/VRCJanSharpCommon/commit/9774fc6a636c5d8ffaaab02020f80cf3afd2eef1))
- Optimize Base64.TryDecode speed slightly ([`a4a155e`](https://github.com/JanSharp/VRCJanSharpCommon/commit/a4a155e54c9991fd532fba245c5988393f9ca468))
- Improve abort message for on build actions ([`0a65328`](https://github.com/JanSharp/VRCJanSharpCommon/commit/0a65328727797cfcfbf63bd63b95ec25788e0291))

### Added

- Add "Tools -> JanSharp -> Remove UI Toggle Listeners Targeting Missing Objects" ([`b4e7da2`](https://github.com/JanSharp/VRCJanSharpCommon/commit/b4e7da2be6301cab80230bb565fc22c075105b26))
- Add "Do Notify On Receive" to UIToggleGroupSync, just like UIToggleSync ([`b4e7da2`](https://github.com/JanSharp/VRCJanSharpCommon/commit/b4e7da2be6301cab80230bb565fc22c075105b26))
- Add QuickDebugUI to display some real time values ([`0e5d805`](https://github.com/JanSharp/VRCJanSharpCommon/commit/0e5d80546f7b0720d682cee00bf5c0b244ede414), [`07d71a5`](https://github.com/JanSharp/VRCJanSharpCommon/commit/07d71a55b4dad20781a176b8cfca52b364d9eff1))
- Add LocalToggleMultipleOnInteract component ([`1afdd93`](https://github.com/JanSharp/VRCJanSharpCommon/commit/1afdd935e175b7866b212cbf16d02522f1fb2236))
- Add InterpolationManager ([`ffa5e02`](https://github.com/JanSharp/VRCJanSharpCommon/commit/ffa5e02cc9eec2195e87d941446bd6aa43812747), [`3cd2cf0`](https://github.com/JanSharp/VRCJanSharpCommon/commit/3cd2cf0db45c3440955e8a767493915bff3e9db7), [`16c2bf1`](https://github.com/JanSharp/VRCJanSharpCommon/commit/16c2bf1f74fb2835356bc7c05eade8ed6872e48c))
- Add JAN_SHARP_COMMON_ON_BUILD_TRACE ([`613153c`](https://github.com/JanSharp/VRCJanSharpCommon/commit/613153c968be7b8039ccb8a5bd141d77ee80bd6c))
- Add BypassBuildTimeIdAssignment component ([`14ad655`](https://github.com/JanSharp/VRCJanSharpCommon/commit/14ad65550ea873105246c80d1dc884e965433b3f))
- Add DeletePersistentEventListenerAtIndex to EditorUtil ([`68108cc`](https://github.com/JanSharp/VRCJanSharpCommon/commit/68108cc9390ea85df307bfcb7725232a26f20178))
- Add IsEditorOnly helper functions to EditorUtil ([`95a14e3`](https://github.com/JanSharp/VRCJanSharpCommon/commit/95a14e3b5cbbba1d4889ee4f62bfd3e1f2b203a8))
- Add EnqueueAtFront and DequeueFromBack to ArrQueue ([`7049c14`](https://github.com/JanSharp/VRCJanSharpCommon/commit/7049c14b9dcfe40ec24f6b79a3ed2c1d8707617b))

### Removed

- **Breaking:** Remove "Automatically Use Child Toggles" from UIToggleGroupSync due to being unintuitive in favor of new editor scripting ([`b4e7da2`](https://github.com/JanSharp/VRCJanSharpCommon/commit/b4e7da2be6301cab80230bb565fc22c075105b26))

### Fixed

- Fix bone attachment error when reusing near bone ([`e5f8eb8`](https://github.com/JanSharp/VRCJanSharpCommon/commit/e5f8eb82d0130e51c255449f997f8e8455057e9b))
- Fix bone attachment index out of range exception ([`662cadf`](https://github.com/JanSharp/VRCJanSharpCommon/commit/662cadf26b052cf7a3b8c078edab277510e60711))

## [1.0.2] - 2025-04-24

### Added

- Add note about 0u not being used by build time id assignment ([`bb1f41a`](https://github.com/JanSharp/VRCJanSharpCommon/commit/bb1f41a360ece31b8ac23eddc7a51d7b30673ab3))
- Add TimeSpan to DataStream ([`a0d01b7`](https://github.com/JanSharp/VRCJanSharpCommon/commit/a0d01b7c39b77c89627972c7ccd8d0b4b5cd62ef))
- Add Color and Color32 to DataStream ([`1345972`](https://github.com/JanSharp/VRCJanSharpCommon/commit/1345972edf717aa4507ce2b2167de54b7bca6a9e))
- Add decimal read and write functions to DataStream ([`bbe1cae`](https://github.com/JanSharp/VRCJanSharpCommon/commit/bbe1cae730922e243f0116816c48e88b5037c10d))

### Fixed

- Fix error when attempting to use SingletonManager ([`d9907a2`](https://github.com/JanSharp/VRCJanSharpCommon/commit/d9907a21bc0fb107dc44672cc8c8bb9aff0557ea))

## [1.0.1] - 2025-03-26

### Fixed

- Fix UpdateManager erroring on multiple removals during CustomUpdate ([`58e00e9`](https://github.com/JanSharp/VRCJanSharpCommon/commit/58e00e9e3bb7c5e68a0e34b292fe721e43d799db))

## [1.0.0] - 2025-03-22

### Changed

- **Breaking:** Remove `ref` for params which do not need it in DataStream ([`1abda11`](https://github.com/JanSharp/VRCJanSharpCommon/commit/1abda1182c9b6bdfb018ba0382166bc174b697b0))
- **Breaking:** Update to Unity 2022 ([`ac36f8f`](https://github.com/JanSharp/VRCJanSharpCommon/commit/ac36f8fd201910bae5ddf385eeba14100d24cbab))
- Rearrange docs in readme ([`9651db7`](https://github.com/JanSharp/VRCJanSharpCommon/commit/9651db7e937958662009c86f9251f76e6068aa1d), [`d2a693d`](https://github.com/JanSharp/VRCJanSharpCommon/commit/d2a693de9e5dfbd078d1f00fe5b4bfd0931f4c49), [`0823886`](https://github.com/JanSharp/VRCJanSharpCommon/commit/08238862ff95e612a40bb45b977bd0cc7873cc3f), [`2915d47`](https://github.com/JanSharp/VRCJanSharpCommon/commit/2915d47e327dcd93db9b13177b46ee749709f811))
- Arrange scripts into sensible categories ([`a929883`](https://github.com/JanSharp/VRCJanSharpCommon/commit/a92988305d6331d2f4a46453f98652d81cd8a0de))
- Make UpdateManager support deregister inside of CustomUpdate handlers ([`db52b3f`](https://github.com/JanSharp/VRCJanSharpCommon/commit/db52b3fd2d40e065f15aeb15cad42b362dc4e11c))
- Update UpdateManager and use SingletonScript attribute ([`613008e`](https://github.com/JanSharp/VRCJanSharpCommon/commit/613008ec15d8969ed97603e689ac1d23ce8f9df3))
- Handle newly instantiated objects during OnBuild by requiring another build attempt ([`f6892f4`](https://github.com/JanSharp/VRCJanSharpCommon/commit/f6892f40509a6e208b63a181829d910db1f27039))
- Add dialogs and notifications to OnBuildUtil ([`f21b4ad`](https://github.com/JanSharp/VRCJanSharpCommon/commit/f21b4ad0f1560c80d4f95078ed64a0312782fc27))
- Improve ArrList performance by using System.Array.Copy instead of loops ([`7c35aca`](https://github.com/JanSharp/VRCJanSharpCommon/commit/7c35aca16c09aa7f38e4b96a36e6453e8ff903c8))
- Add note about m_Target for persistent listeners ([`e87405b`](https://github.com/JanSharp/VRCJanSharpCommon/commit/e87405bf59a4a93333cf38db2c1aa4111472a7b7))
- Use BitConverter for DataStream ([`d6b9053`](https://github.com/JanSharp/VRCJanSharpCommon/commit/d6b9053fe26d99a60a439c8404d729117eadaa01))
- Improve DataStream ReadBytes performance ([`64c7bc4`](https://github.com/JanSharp/VRCJanSharpCommon/commit/64c7bc475e1a517b68788327273df57dbc77b628))
- Allow registering non U# behaviour components for OnBuildUtil ([`aa19aa1`](https://github.com/JanSharp/VRCJanSharpCommon/commit/aa19aa14c6613b484d9c59779839dd13a7920191))
- Add overloads to register OnBuild handlers using System.Type rather than a generic type parameter ([`4364ea4`](https://github.com/JanSharp/VRCJanSharpCommon/commit/4364ea4cc18a0a739addce42c3fab4a859ea5fa9))
- Add ability to register actions to OnBuildUtil, without required associated components in the scene ([`4364ea4`](https://github.com/JanSharp/VRCJanSharpCommon/commit/4364ea4cc18a0a739addce42c3fab4a859ea5fa9))
- Add RegisterTypeCumulative to OnBuildUtil to get the whole list of found objects ([`fd93510`](https://github.com/JanSharp/VRCJanSharpCommon/commit/fd93510ede5e6122bb29bfa99413ad611a0aebb7), [`4364ea4`](https://github.com/JanSharp/VRCJanSharpCommon/commit/4364ea4cc18a0a739addce42c3fab4a859ea5fa9), [`f5ef453`](https://github.com/JanSharp/VRCJanSharpCommon/commit/f5ef45348f070322cd733a7104da859330530037))

### Added

- Add BuildTimeIdAssignment attribute and editor scripting ([`f5a520d`](https://github.com/JanSharp/VRCJanSharpCommon/commit/f5a520da6a5807a25b498385ca55a5e03926c965))
- Add WannaBeArrQueue which manages refs to WannaBeClasses ([`699f7bd`](https://github.com/JanSharp/VRCJanSharpCommon/commit/699f7bd30b2f385f5b08a2263d6bd665bbbec7a5))
- Add WannaBeArrList which manages refs to WannaBeClasses ([`8e54d21`](https://github.com/JanSharp/VRCJanSharpCommon/commit/8e54d218ae01acb48c418901ea175304dde3839b))
- Add getter for private base members (reflection) to EditorUtil ([`d4d1efd`](https://github.com/JanSharp/VRCJanSharpCommon/commit/d4d1efd6718296312f3634f7327aa630dca4a496))
- Add Write byte array overload with `startIndex` and `size` to DataStream ([`eed8d4a`](https://github.com/JanSharp/VRCJanSharpCommon/commit/eed8d4a96d34b48e96bef48a618b494664cfd4f0))
- Add WannaBeClasses and Manager ([`6711e1a`](https://github.com/JanSharp/VRCJanSharpCommon/commit/6711e1aa0a0f2c4be5d716604c2fe142b67f7387), [`b9e6fe4`](https://github.com/JanSharp/VRCJanSharpCommon/commit/b9e6fe45b3ae41fbffe745e5810a97b0a1c14dd8), [`0d7e086`](https://github.com/JanSharp/VRCJanSharpCommon/commit/0d7e0860311feb13328f13988b6d928257385c87), [`d77e4e0`](https://github.com/JanSharp/VRCJanSharpCommon/commit/d77e4e0f792a1222c9943ae2627e5367ae6a62d7), [`070eb62`](https://github.com/JanSharp/VRCJanSharpCommon/commit/070eb62d920b33e3e76fe8b023b1dca2ef730655), [`95e3001`](https://github.com/JanSharp/VRCJanSharpCommon/commit/95e300194c061af2b766881e0e1f10af745eb9c6), [`ffd0625`](https://github.com/JanSharp/VRCJanSharpCommon/commit/ffd062574bf7471d93e98489655991ab1c4bb9ef), [`21466d6`](https://github.com/JanSharp/VRCJanSharpCommon/commit/21466d68d4c3c4459e95fb337db4718a9efd81e4), [`aa2faa5`](https://github.com/JanSharp/VRCJanSharpCommon/commit/aa2faa56aea431f90e1f7bd5196dd98d2fae1d7c), [`068497e`](https://github.com/JanSharp/VRCJanSharpCommon/commit/068497ee7fb738bb4898272dbab45f019fe11b4b), [`c03694a`](https://github.com/JanSharp/VRCJanSharpCommon/commit/c03694a3f39d7d797808578557ce8186a789754b), [`2915d47`](https://github.com/JanSharp/VRCJanSharpCommon/commit/2915d47e327dcd93db9b13177b46ee749709f811))
- Allow detaching already destroyed transforms in BoneAttachmentManager ([`d073b3d`](https://github.com/JanSharp/VRCJanSharpCommon/commit/d073b3d663df72d440be5b582bff9c1c611048ad))
- Pause attachment when bones do not exist ([`7665931`](https://github.com/JanSharp/VRCJanSharpCommon/commit/7665931cc20c3fdd429f365725e5f479abe7323f))
- Add BoneAttachmentManager library ([`cbc23b0`](https://github.com/JanSharp/VRCJanSharpCommon/commit/cbc23b09074d64ccf482f184bccd3613727385cd), [`32b9035`](https://github.com/JanSharp/VRCJanSharpCommon/commit/32b903514777442bf53871cbafe3510ce525589b), [`0beea7e`](https://github.com/JanSharp/VRCJanSharpCommon/commit/0beea7e31251932a7191df191685828e2b428346), [`d1882f7`](https://github.com/JanSharp/VRCJanSharpCommon/commit/d1882f7dbab63ef2179007f4226c998d474b71d5), [`d5fec5c`](https://github.com/JanSharp/VRCJanSharpCommon/commit/d5fec5c5bfa3711856938e128fc5cc135cff12fd), [`34bd6f3`](https://github.com/JanSharp/VRCJanSharpCommon/commit/34bd6f355154d86058ef93bb114136ba8aa5341d))
- Add TrulyPostLateUpdate event scripts can listen for ([`00db213`](https://github.com/JanSharp/VRCJanSharpCommon/commit/00db213dd0cec2a0eaf71a37b1a3bb67b608bee2))
- Add SingletonManager for scripts to resolve references to singletons at runtime ([`acb9ac0`](https://github.com/JanSharp/VRCJanSharpCommon/commit/acb9ac03ec2d29773e274b1e1cfca1f4b74d49b6))
- Add SingletonDependency attribute for use on class without requiring a field with the given type ([`577ac71`](https://github.com/JanSharp/VRCJanSharpCommon/commit/577ac715627ca2dc7ed81fc2a010615e4f93e364))
- Add instantiation of singleton prefab dependencies ([`62b5e35`](https://github.com/JanSharp/VRCJanSharpCommon/commit/62b5e35f15b1d083e8b45c3930ae201025bc4255))
- Add named parameter to an attribute for optional vs required singletons ([`a36db36`](https://github.com/JanSharp/VRCJanSharpCommon/commit/a36db36f6b9b6d2cf82552938e48f6c53135b0b4))
- Add singleton script references through attributes ([`3443fd8`](https://github.com/JanSharp/VRCJanSharpCommon/commit/3443fd8fb9b2b7a8087fbcd8d90a2460c79212a3), [`d11c2d4`](https://github.com/JanSharp/VRCJanSharpCommon/commit/d11c2d44a74d8e6c0a008b260e54e32e46821869), [`e5493a8`](https://github.com/JanSharp/VRCJanSharpCommon/commit/e5493a8830cfba1de9abe9be26973e25fab60e02))
- Sort custom raised events by DefaultExecutionOrder ([`85b6d41`](https://github.com/JanSharp/VRCJanSharpCommon/commit/85b6d411f414daa308f591d015ff0de60b755b95))
- Add attribute based CustomRaisedEvents utility ([`8b399fe`](https://github.com/JanSharp/VRCJanSharpCommon/commit/8b399fef81d8fd41ae0733bc97d2301bace43dc5), [`93a4331`](https://github.com/JanSharp/VRCJanSharpCommon/commit/93a433129f4b54605ed7babe376aaeae3a1a769e))
- Add OnAssemblyLoadUtil which provides a list of all UdonSharpBehaviour types at assembly load time ([`8b399fe`](https://github.com/JanSharp/VRCJanSharpCommon/commit/8b399fef81d8fd41ae0733bc97d2301bace43dc5))
- Add sprites for UIs used by other JanSharp packages ([`e4304da`](https://github.com/JanSharp/VRCJanSharpCommon/commit/e4304da765e18192d541ba6b0b5a5c0ace323fdf), [`43dc1da`](https://github.com/JanSharp/VRCJanSharpCommon/commit/43dc1dadc0d769f5d4d95bffeac6e1f8aab168f7))

## [0.4.3] - 2024-07-28

### Fixed

- Fix ArrList.Insert index out of range exception ([`24df71a`](https://github.com/JanSharp/VRCJanSharpCommon/commit/24df71a83b095dc8f20f8094cc5c7a637ddd6aae))

## [0.4.2] - 2024-06-08

### Changed

- Improve tooltips for UIToggleSendLocalEvent ([`8ca6ab7`](https://github.com/JanSharp/VRCJanSharpCommon/commit/8ca6ab7f3080e1f776102f63bc40636dc511adcf))

### Added

- Add PickupSendLocalEvent to send events when a VRCPickup is picked up, dropped or used while held ([`c70dec1`](https://github.com/JanSharp/VRCJanSharpCommon/commit/c70dec17d12487b7c04d33af18efc663c7e797cc))

## [0.4.1] - 2024-05-27

### Fixed

- Fix Set Toggle Group to itself erroring ([`d3b71ed`](https://github.com/JanSharp/VRCJanSharpCommon/commit/d3b71ed086f5c7cc03a795f4b4c493b738b2818c))

## [0.4.0] - 2024-05-27

_The persistent calls change is breaking because EditorUtils function signatures with UnityEvent/-Base params have been changed._

### Changed

- Deprecate count param for ArrList EnsureCapacity ([`889f438`](https://github.com/JanSharp/VRCJanSharpCommon/commit/889f43863bb6ea222bebc815939619bcaf5b7605))
- Move Run all OnBuild handlers under Tools/JanSharp ([`03e45fe`](https://github.com/JanSharp/VRCJanSharpCommon/commit/03e45fe99fa596dd5bcd81519dceae30bec3392c))

### Added

- Add UIToggleSendLocalEvent script ([`05e5042`](https://github.com/JanSharp/VRCJanSharpCommon/commit/05e50426284a7d58b5fa1995051d845c24572896))
- Add UIToggleGroupSync script ([`3f4b96b`](https://github.com/JanSharp/VRCJanSharpCommon/commit/3f4b96b28d3d41cd99150202b30bf5407bf9a068))
- Add DataStream for binary serialization ([`f82387b`](https://github.com/JanSharp/VRCJanSharpCommon/commit/f82387b580c5d25fbcb832a9dcf73fadddf30f7b), [`f4614b0`](https://github.com/JanSharp/VRCJanSharpCommon/commit/f4614b037746da94c785ce6735c19a34c9b51538), [`403ea35`](https://github.com/JanSharp/VRCJanSharpCommon/commit/403ea353ee7ef88d435d39a88904754492bb6fea), [`3d7acec`](https://github.com/JanSharp/VRCJanSharpCommon/commit/3d7acec36c72b7276d55b2d721d3e11ec771ed4c), [`b0033fe`](https://github.com/JanSharp/VRCJanSharpCommon/commit/b0033fe9dac90a0bc32c8171bbe13aa5999dc0b3), [`6997dcc`](https://github.com/JanSharp/VRCJanSharpCommon/commit/6997dccd0bdc2b669293aecdd83b68b1a72057cc))
- Add ArithBitConverter for float to byte conversion for those still on 2019 ([`d576b8e`](https://github.com/JanSharp/VRCJanSharpCommon/commit/d576b8e76a180b7b3e9c58b9e2ba8643ecca38d9), [`98368ce`](https://github.com/JanSharp/VRCJanSharpCommon/commit/98368ce725c1ad545ceaa87aaac568f4bc1325c2))
- Add Base64 specifically for TryDecode, because we cannot catch and handle exceptions in Udon ([`c20d981`](https://github.com/JanSharp/VRCJanSharpCommon/commit/c20d98111e9c4fdbc6c22d2ad78270fb3333b4de))
- Add CRC32 and third party license notices ([`aa08fd8`](https://github.com/JanSharp/VRCJanSharpCommon/commit/aa08fd8ecc1f7e2a4f20d2c78fc4b8caae515832), [`5c194aa`](https://github.com/JanSharp/VRCJanSharpCommon/commit/5c194aa065f73b2bd4af356f63aa364c44a51fff), [`fb71207`](https://github.com/JanSharp/VRCJanSharpCommon/commit/fb71207abe27814ff245c555cdfa70a1857a5f1b))

### Fixed

- **Breaking:** Make editor script for adding persistent calls actually persistent ([`9878b9e`](https://github.com/JanSharp/VRCJanSharpCommon/commit/9878b9ee5219be74c233ef7e6c29e33e8509908d))

## [0.3.0] - 2024-03-16

_If the 2 editor tools were used, simply install the [`com.jansharp.editor-tools`](https://github.com/JanSharp/VRCEditorTools) package to obtain them again._

### Removed

- **Breaking:** Remove UI Color Changer and Occlusion Visibility Window by splitting them into the new [`com.jansharp.editor-tools`](https://github.com/JanSharp/VRCEditorTools) package ([`cfe3a99`](https://github.com/JanSharp/VRCJanSharpCommon/commit/cfe3a99b60e1d82db415037bb8671f3fc0905a96))

## [0.2.1] - 2023-10-12

### Changed

- Support registering for UdonSharpBehaviour itself, to run an OnBuild handler on every UdonSharpBehaviour ([`d962d98`](https://github.com/JanSharp/VRCJanSharpCommon/commit/d962d983e79f1b8ea53a04643b114bacd4e79b0a))
- Update vrc worlds dependency to 3.4.x ([`4de25c0`](https://github.com/JanSharp/VRCJanSharpCommon/commit/4de25c0f2484ec591200375bfe07071546a23731))

### Removed

- Remove udonsharp dependency as it has been merged into worlds ([`4de25c0`](https://github.com/JanSharp/VRCJanSharpCommon/commit/4de25c0f2484ec591200375bfe07071546a23731))

## [0.2.0] - 2023-08-14

### Changed

- **Breaking:** Support inheritance in OnBuild ([`e823f90`](https://github.com/JanSharp/VRCJanSharpCommon/commit/e823f90bdfe72493d5fc0e025201fe46cc24b0be), [`da7b8c8`](https://github.com/JanSharp/VRCJanSharpCommon/commit/da7b8c8570a718367f46ec7c67821544dea92bc9))

### Added

- Add UIToggleSync ([`748679f`](https://github.com/JanSharp/VRCJanSharpCommon/commit/748679f051b6cc56da5b7b25b9e032a75a4570e7))
- Add UIToggleInteractProxy ([`9ceb055`](https://github.com/JanSharp/VRCJanSharpCommon/commit/9ceb05599860d4c8470c491f25271d08af80f1b3))
- Add Occlusion Visibility Window ([`fbcf53d`](https://github.com/JanSharp/VRCJanSharpCommon/commit/fbcf53d80a726220fadbd2fa6f9e396df16009fb), [`da7b8c8`](https://github.com/JanSharp/VRCJanSharpCommon/commit/da7b8c8570a718367f46ec7c67821544dea92bc9))
- Add EnumerateArrayProperty to EditorUtil ([`78019b9`](https://github.com/JanSharp/VRCJanSharpCommon/commit/78019b90d9b20e92d4b1b19f0cedaf24221084f4))
- Add ConditionalRegisterCustomEventListenerButton to EditorUtil ([`0793f4f`](https://github.com/JanSharp/VRCJanSharpCommon/commit/0793f4f11e0d3b75f37f0fe1af6defa8f02fb971))

## [0.1.3] - 2023-08-05

### Changed

- Change LICENSE.txt to LICENSE.md so Unity sees it in the package manager window ([`9a6b5bb`](https://github.com/JanSharp/VRCJanSharpCommon/commit/9a6b5bb31eee6317bf4c0c9fd82e41f4df1c93b1))

### Added

- Add ArrList and ArrQueue library files ([`c4e76e1`](https://github.com/JanSharp/VRCJanSharpCommon/commit/c4e76e12307a7ee9bbd3c7d660cc30fde3d24686), [`65d0c0e`](https://github.com/JanSharp/VRCJanSharpCommon/commit/65d0c0e26f4d457cc2d087eacafccad381ede854))
- Add vpm dependency on `com.vrchat.worlds` for clarity ([`114cd86`](https://github.com/JanSharp/VRCJanSharpCommon/commit/114cd86db82e4889027cfbcc827b46e00c231c6a))

### Fixed

- Fix build error on publish ([`036bb38`](https://github.com/JanSharp/VRCJanSharpCommon/commit/036bb385cb462063e2b7a13d54af3127170d6deb))
- Fix that OnBuild was ignoring inactive objects ([`a8b2c83`](https://github.com/JanSharp/VRCJanSharpCommon/commit/a8b2c8325d611e514a51540eb9d59e56ede58c6a))

## [0.1.2] - 2023-07-23

### Changed

- **Breaking:** Change GUID of UIColorsChanger as it is no longer a component ([`8cfca22`](https://github.com/JanSharp/VRCJanSharpCommon/commit/8cfca22cdcf38e6badfa228c5b8cd672e76d5f1b))

### Added

- Add LocalToggleOnValueChanged to toggle game objects using UI toggles ([`9bfb223`](https://github.com/JanSharp/VRCJanSharpCommon/commit/9bfb223ff36f76238065e8a616e3ee5165b48d60), [`1ce4c24`](https://github.com/JanSharp/VRCJanSharpCommon/commit/1ce4c24af7e285d32cb1d1fff749b41be000b681))

## [0.1.1] - 2023-07-22

### Added

- Add InteractProxy to pass along VRChat's Interact event ([`3af31b5`](https://github.com/JanSharp/VRCJanSharpCommon/commit/3af31b51d3b8ddc203a033e155ba7eb45af27c16))

## [0.1.0] - 2023-07-17

_First version of this package that is in the VCC listing._

### Added

- Add OnBuildUtil, allowing multiple registrations and a defined order ([`de04745`](https://github.com/JanSharp/VRCJanSharpCommon/commit/de04745880f0ea37345b5fd4e54de94fe7f05368), [`21879fb`](https://github.com/JanSharp/VRCJanSharpCommon/commit/21879fb86915b00da5b044dd242a4c6ff265e0b7), [`17dbab8`](https://github.com/JanSharp/VRCJanSharpCommon/commit/17dbab84b8bb6bad192d67607a5f45c8cd000356), [`a98e0a9`](https://github.com/JanSharp/VRCJanSharpCommon/commit/a98e0a9e90e8eef5c1b7027c67dfac181b04a94b), [`37c33d1`](https://github.com/JanSharp/VRCJanSharpCommon/commit/37c33d14bdd25f18ba71a01c51360b6e1182684d), [`c7be3da`](https://github.com/JanSharp/VRCJanSharpCommon/commit/c7be3dac839a5f667cccd817751888a783d403cc), [`c25fbc9`](https://github.com/JanSharp/VRCJanSharpCommon/commit/c25fbc92d503f770dc1131d7802664f27f3a83a4), [`1c3fb19`](https://github.com/JanSharp/VRCJanSharpCommon/commit/1c3fb190e8f6c96763abc2ba600e9d4bd7b4d25f), [`bb402d6`](https://github.com/JanSharp/VRCJanSharpCommon/commit/bb402d6df1af7e28d51cea9d660b6ea2e4669353))
- Add UpdateManager, handling double registering and deregistering ([`d2ea8a3`](https://github.com/JanSharp/VRCJanSharpCommon/commit/d2ea8a3465938e28e25949fb422b760c607e0378), [`7c7c1a0`](https://github.com/JanSharp/VRCJanSharpCommon/commit/7c7c1a0d77d3b935aa735ea47faa252d2ad2329a), [`5b7e49d`](https://github.com/JanSharp/VRCJanSharpCommon/commit/5b7e49dc182d497003711204bb1a8c1cef69df06), [`d023b07`](https://github.com/JanSharp/VRCJanSharpCommon/commit/d023b07ccddff6be964ae293fd3005d990a4d95f), [`d615fb7`](https://github.com/JanSharp/VRCJanSharpCommon/commit/d615fb7a8b62053c4479ebc8934f3edf89b4ef18), [`335ac10`](https://github.com/JanSharp/VRCJanSharpCommon/commit/335ac10be9d42fd101a9b89ffbf80cba5ae098f3))
- Add UpdateManager prefab ([`73f16ce`](https://github.com/JanSharp/VRCJanSharpCommon/commit/73f16ced0787eb922f492568ec586a2868d9013c))
- Add LocalEventOnInteract to send a custom event to any behaviour locally ([`abd815e`](https://github.com/JanSharp/VRCJanSharpCommon/commit/abd815e72972a05ff48f9a20a59ffa9d02c53bc5))
- Add LocalToggleOnInteract to toggle a single GameObject locally ([`b99f245`](https://github.com/JanSharp/VRCJanSharpCommon/commit/b99f2458b5de112ad58a25d7d378e146fd7112fb))
- Add EditorUtil to help working with SerializedObjects and SerializedProperties and other editor only utils ([`ee4ffb5`](https://github.com/JanSharp/VRCJanSharpCommon/commit/ee4ffb5ffe6218097cd01b94becc93bafb6ad2ca))
- Add UI Color Changer to update colors of selectable UIs ([`1348553`](https://github.com/JanSharp/VRCJanSharpCommon/commit/134855335360925369c9f24b51e7e6922e592167), [`bdd8755`](https://github.com/JanSharp/VRCJanSharpCommon/commit/bdd8755ea6483e13a40c63f260f8de71a1f5a069), [`0456770`](https://github.com/JanSharp/VRCJanSharpCommon/commit/0456770c8541b7ca2e33b69d215c926deab37077))
- Add features list and installation instructions to the README ([`08677d0`](https://github.com/JanSharp/VRCJanSharpCommon/commit/08677d0df3601b46ef734703380856ff5c4bf942), [`63c2d27`](https://github.com/JanSharp/VRCJanSharpCommon/commit/63c2d27715efb4fbecd4f3bb5d1521ae9f7f0fa8))

[1.1.0]: https://github.com/JanSharp/VRCJanSharpCommon/releases/tag/v1.1.0
[1.0.2]: https://github.com/JanSharp/VRCJanSharpCommon/releases/tag/v1.0.2
[1.0.1]: https://github.com/JanSharp/VRCJanSharpCommon/releases/tag/v1.0.1
[1.0.0]: https://github.com/JanSharp/VRCJanSharpCommon/releases/tag/v1.0.0
[0.4.3]: https://github.com/JanSharp/VRCJanSharpCommon/releases/tag/v0.4.3
[0.4.2]: https://github.com/JanSharp/VRCJanSharpCommon/releases/tag/v0.4.2
[0.4.1]: https://github.com/JanSharp/VRCJanSharpCommon/releases/tag/v0.4.1
[0.4.0]: https://github.com/JanSharp/VRCJanSharpCommon/releases/tag/v0.4.0
[0.3.0]: https://github.com/JanSharp/VRCJanSharpCommon/releases/tag/v0.3.0
[0.2.1]: https://github.com/JanSharp/VRCJanSharpCommon/releases/tag/v0.2.1
[0.2.0]: https://github.com/JanSharp/VRCJanSharpCommon/releases/tag/v0.2.0
[0.1.3]: https://github.com/JanSharp/VRCJanSharpCommon/releases/tag/v0.1.3
[0.1.2]: https://github.com/JanSharp/VRCJanSharpCommon/releases/tag/v0.1.2
[0.1.1]: https://github.com/JanSharp/VRCJanSharpCommon/releases/tag/v0.1.1
[0.1.0]: https://github.com/JanSharp/VRCJanSharpCommon/releases/tag/v0.1.0
