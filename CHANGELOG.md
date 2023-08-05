
# Changelog

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

[0.1.3]: https://github.com/JanSharp/VRCJanSharpCommon/releases/tag/v0.1.3
[0.1.2]: https://github.com/JanSharp/VRCJanSharpCommon/releases/tag/v0.1.2
[0.1.1]: https://github.com/JanSharp/VRCJanSharpCommon/releases/tag/v0.1.1
[0.1.0]: https://github.com/JanSharp/VRCJanSharpCommon/releases/tag/v0.1.0
