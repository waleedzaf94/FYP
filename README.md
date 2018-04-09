# HoloLens Application 

### Overview

Two features: 

1. Recording View: Uses the SpatialUnderstanding Prefab to record and store mesh.
2. Visualization View: Access processed meshes from Azure blob storage and display library & mesh as hologram

### Tasks
- [x] Save Single Mesh
- [x] Simple Library Menu 
- [x] Download Mesh from Blob 
- [x] Visualize Downloaded Mesh 
- [ ] Accounts 
- [ ] Extra Info For Mesh

### Issues 
- [x] Older mesh is not getting destroyed
- [x] Reset and Restart Recording View: Reset happens properly when it is in ScanState == Scanning otherwise, mesh is not destroyed. 
    
### Improvements 
- [ ] Make the Library better 
- [x] Loading Screens 
- [ ] Make mesh generation async - Need to figure this out 
