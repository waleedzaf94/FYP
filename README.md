# HISS: Holo Indoor Spatial Scanner
## Overview

HISS is a HoloLens application developed as part of the Final Year Project 2017-18, Department of Computer Science, The University of Hong Kong. 

The project webiste's full documentation are available on the [project website](http://i.cs.hku.hk/fyp/2017/fyp17010/).



The application has two features: 

1. **Recording View**: Uses the SpatialUnderstanding Prefab to record and store mesh.
2. **Visualization View**: Access processed meshes from Azure blob storage and display library & mesh as hologram


## Installation & Dependencies
### Dependencies
1. [Mixed Reality Toolkit](https://github.com/Microsoft/MixedRealityToolkit-Unity)
2. [RESTClient for Unity](https://github.com/Unity3dAzure/RESTClient)
3. [StorageServices for Unity](https://github.com/Unity3dAzure/StorageServices
)
3. Unity 2017.2
4. Visual Studios 2017

### Installation 
1. Clone project and update submodules 
2. Open project in Unity 2017.2
3. Open build settings and build for UWP as C# project
4. Open built project using Visual Studios 2017
5. Deploy project as `x86 Release` build on HoloLens

## Issues
1. WorldAnchor may get lost due to movement during download/upload of mesh
2. Malicious App Behaviour: Old recorded mesh may not be cleared if save has already been called once.  
