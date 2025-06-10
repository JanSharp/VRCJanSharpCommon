
# Ideas

MoveToRoot script which moves a given object to the root of the scene on Start. Needs a manager to ensure even disabled objects with this component on them get moved. How different is this to a parent constraint?

- [x] look at newly exposed BitConverter
- [ ] look at newly exposed StringBuilder
- [x] look at Encoding.UTF8, pretty sure that's also newly exposed because otherwise I'd have used that to begin with
- [x] editor utility which automatically instantiates prefab instances for system dependencies, recursively
  - [x] support this in the singleton editor utility
- [x] add optional vs required setting to singleton, required being the default
- [x] editor script which assigns build time IDs to objects as well as keeping track of the highest id inside of a given manager script
- [x] add a flag on build handlers can set which indicates that objects have been instantiated during the on build process, which should cancel the current build and show a popup requesting the build to be restarted at the end
- [x] add new stuff to the docs, and while you're there make docs better
- [ ] should bone attachment automatically detach when a player leaves as well as when an attached transform no longer has any children - nothing is attached to it anymore?
- [x] use System.Buffer.BlockCopy rather than System.Array.Copy... I guess?
- [x] make stream param for read functions not `ref`, because whey the heck are they `ref`?
- [x] add `decimal` read and write functions to `DataStream`
- [ ] better explain singletons in the readme
- [ ] better explain build time id assignment in the readme
- [ ] think about optional vpm dependencies
- [ ] remove the WannaBeConstructor, because unlike normal default constructors, this one would get "raised" even when a non default constructor gets used, which does not make sense and is annoying to think about and deal with
- [ ] look at the wanna be class prefab creation editor script to make sure it is using the correct api
- [x] api for interpolation manager to change or cancel ongoing interpolations
- [ ] add editor scripting for LocalToggleMultipleOnInteract
  - [ ] only show the 2 interaction fields when the checkbox is enabled
  - [ ] the activate text is actually grayed out and automatically matches the interaction text of the UB itself
