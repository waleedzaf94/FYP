using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA.Input;
using HoloToolkit.Unity.SpatialMapping;
using Windows.Perception.Spatial.Surfaces;
using System.Threading.Tasks;
using System;
using Windows.Foundation;
using Windows.Perception.Spatial;
using HoloToolkit.Unity;
using static HoloToolkit.Unity.SpatialUnderstandingDll;
using System.Runtime.InteropServices;
using Windows.Storage;

public class MainApp : MonoBehaviour {


    public GameObject saveButtonPrefab;

    public GameObject spatialUnderstandingPrefab;

    public GameObject spatialMeshPrefab;

    SpatialMappingManager spatialMappingManager;

    GestureRecognizer recognizer;

    SpatialSurfaceObserver surfaceObserver;
    private SpatialLocator locator;
    private SpatialCoordinateSystem baseCoordinateSystem;
    private SpatialBoundingVolume boundingVolume;
    private bool spatialAccepted;
    private StorageFolder storageFolder;

    // Use this for initialization
    void Start() {

        spatialMappingManager = new SpatialMappingManager();
        recognizer = new GestureRecognizer();
        SpatialUnderstanding.Instance.RequestBeginScanning();
        surfaceObserver = new SpatialSurfaceObserver();
        recognizer.TappedEvent += Recognizer_TappedEvent;
        spatialAccepted = false;
        recognizer.StartCapturingGestures();
        var requestAsync = SpatialSurfaceObserver.RequestAccessAsync();
        requestAsync.Completed = new AsyncOperationCompletedHandler<Windows.Perception.Spatial.SpatialPerceptionAccessStatus>(requestAccessComp);
        storageFolder = ApplicationData.Current.LocalFolder;

    }

    // Update is called once per frame
    void Update() {

    }

    private void Recognizer_TappedEvent(InteractionSourceKind source, int tapCount, Ray headRay)
    {
        //var direction = headRay.direction;

        //var origin = headRay.origin;

        //var position = origin + direction * 2.0f;

        Debug.Log("Tapped Event" + tapCount);
        //if (saveButtonPrefab.activeSelf)
        //{
        //    saveButtonPrefab.SetActive(false);

        //}
        //else
        //{
        //    List <Mesh> meshes = spatialMappingManager.GetMeshes();
        //    saveButtonPrefab = Instantiate(saveButtonPrefab, position, Quaternion.identity);
        //    saveButtonPrefab.SetActive(true);
        //    Debug.Log("List" + meshes.Count);
        //}

        if ((SpatialUnderstanding.Instance.ScanState != SpatialUnderstanding.ScanStates.Scanning) ||
                (!SpatialUnderstanding.Instance.AllowSpatialUnderstanding))
        {
            SpatialUnderstanding.Instance.RequestFinishScan();
            return;
        }

       

        // Query the current playspace stats
        IntPtr statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
        if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) == 0)
        {
            SpatialUnderstanding.Instance.RequestFinishScan();
            return;
        }

        var playspaceStats = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats();
        Debug.Log("Is working: " + playspaceStats.IsWorkingOnStats);
        Debug.Log("Wall SA: " + playspaceStats.WallSurfaceArea);
        Debug.Log("TotalSurfaceArea: " + playspaceStats.TotalSurfaceArea);

        int meshCount;
        IntPtr meshList;
        SpatialUnderstanding.Instance.UnderstandingSourceMesh.GetInputMeshList(out meshCount, out meshList);

        Imports.MeshData[] result = new Imports.MeshData[meshCount];
        int sizePointerToABC = Marshal.SizeOf(new Imports.MeshData());
        for (int i = 0; i < meshCount; i++)
        {
            result[i] = Marshal.PtrToStructure<Imports.MeshData>(new IntPtr(meshList.ToInt32() + (i * sizePointerToABC)));
        }

        foreach (Imports.MeshData m in result)
        {
            processMeshAsync(m);
        }

        Debug.Log("Mesh Count: " + meshCount);

        
        //if (this.spatialAccepted)
        //{
        //    surfaceObserver.SetBoundingVolume(this.boundingVolume);
        //    var surfaceInfoList = surfaceObserver.GetObservedSurfaces();
        //    Debug.Log("KeyCount" + surfaceInfoList.Count);
        //    foreach (var test in surfaceInfoList)
        //    {
        //        Debug.Log("GUID:" + test.Key.ToString());
        //        //var meshAsync = test.Value.TryComputeLatestMeshAsync(500.0);
        //        //meshAsync.Completed = new AsyncOperationCompletedHandler<SpatialSurfaceMesh>(printMeshInfo);
        //    }
        //}
    }

    private async Task processMeshAsync(Imports.MeshData m)
    {
        Vector3[] vertices = new Vector3[m.vertCount];
        Vector3[] vertNormals = new Vector3[m.vertCount];
        int sizePointerToVertices = Marshal.SizeOf(new Vector3());
        string fileName = m.meshID + "_sample.txt";
        StorageFile sampleFile = await storageFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
        StorageFile getFile = await storageFolder.GetFileAsync(fileName);
        await FileIO.WriteTextAsync(getFile, "ID:" + m.meshID );
        for (int i = 0; i < m.vertCount; i++)
        {
            vertices[i] = Marshal.PtrToStructure<Vector3>(new IntPtr(m.verts.ToInt32() + (i * sizePointerToVertices)));
            vertNormals[i] = Marshal.PtrToStructure<Vector3>(new IntPtr(m.normals.ToInt32() + (i * sizePointerToVertices)));
            await FileIO.AppendTextAsync(getFile,  "Vertix: " + vertices[i].ToString() + "  Normal: " + vertNormals[i].ToString());
        }
        Int32[] indices = new Int32[m.indexCount];
        int sizePointerToIndices = Marshal.SizeOf<Int32>();
        for (int j = 0; j < m.indexCount; j ++)
        {
            indices[j] = Marshal.PtrToStructure<Int32>(new IntPtr(m.indices.ToInt32() + (j * sizePointerToIndices)));
            await FileIO.AppendTextAsync(getFile, "Index: " + indices[j]);
        }
    }

    private void requestAccessComp(IAsyncOperation<SpatialPerceptionAccessStatus> asyncInfo, AsyncStatus asyncStatus)
    {
        Debug.Log("User Accepted");
        if (asyncInfo.GetResults().Equals(SpatialPerceptionAccessStatus.Allowed))
        {
            this.spatialAccepted = true;
            Debug.Log("User allowed");
            this.locator = SpatialLocator.GetDefault();

            var frameOfReference = this.locator.CreateStationaryFrameOfReferenceAtCurrentLocation();

            this.baseCoordinateSystem = frameOfReference.CoordinateSystem;
            this.boundingVolume = SpatialBoundingVolume.FromBox(
                  this.baseCoordinateSystem,
                  new SpatialBoundingBox()
                  {
                      Center = new System.Numerics.Vector3(0, 0, 0),
                      Extents = new System.Numerics.Vector3(4, 4, 4)
                  }
            );
        }
        else
        {
            Debug.Log("User Not Allowed");
        }
    }

    private void printMeshInfo(IAsyncOperation<SpatialSurfaceMesh> asyncInfo, AsyncStatus asyncStatus)
    {
        var mesh = asyncInfo.GetResults();
        Debug.Log("Mesh Coordinate System" + mesh.CoordinateSystem.ToString());
    }
}
