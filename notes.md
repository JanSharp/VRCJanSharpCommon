
# Ideas

MoveToRoot script which moves a given object to the root of the scene on Start. Needs a manager to ensure even disabled objects with this component on them get moved. How different is this to a parent constraint?

- [ ] look at newly exposed BitConverter
- [ ] look at newly exposed StringBuilder
- [x] editor utility which automatically instantiates prefab instances for system dependencies, recursively
  - [x] support this in the singleton editor utility
- [x] add optional vs required setting to singleton, required being the default
- [x] editor script which assigns build time IDs to objects as well as keeping track of the highest id inside of a given manager script
- [x] add a flag on build handlers can set which indicates that objects have been instantiated during the on build process, which should cancel the current build and show a popup requesting the build to be restarted at the end
- [ ] add new stuff to the docs, and while you're there make docs better
