using UnityEngine;
using UnityEngine.VR.WSA.Input;
using HoloToolkit.Unity.SpatialMapping;
using Windows.Perception.Spatial.Surfaces;
using System;
using Windows.Foundation;
using Windows.Perception.Spatial;
using HoloToolkit.Unity;
using static HoloToolkit.Unity.SpatialUnderstandingDll;
using Windows.Storage;

public class MainApp : MonoBehaviour {


    public GameObject saveButtonPrefab;

    public GameObject spatialUnderstandingPrefab;

    public GameObject spatialMeshPrefab;

    SpatialMappingManager spatialMappingManager;

    GestureRecognizer recognizer;

    private bool spatialAccepted;
    private StorageFolder storageFolder;
    private MeshInfoFetcher meshInfoFetcher;
    // Use this for initialization
    void Start() {

        spatialMappingManager = new SpatialMappingManager();
        recognizer = new GestureRecognizer();
        recognizer.TappedEvent += Recognizer_TappedEventAsync;
        spatialAccepted = false;
        recognizer.StartCapturingGestures();
        var requestAsync = SpatialSurfaceObserver.RequestAccessAsync();
        requestAsync.Completed = new AsyncOperationCompletedHandler<Windows.Perception.Spatial.SpatialPerceptionAccessStatus>(RequestAccessComp);
        storageFolder = ApplicationData.Current.LocalFolder;

        SpatialUnderstanding.Instance.RequestBeginScanning();

        meshInfoFetcher = new MeshInfoFetcher
        {
            StorageFolderName = storageFolder
        };
    }

    // Update is called once per frame
    void Update() {

    }

    private void Recognizer_TappedEventAsync(InteractionSourceKind source, int tapCount, Ray headRay)
    {
        var direction = headRay.direction;
        var origin = headRay.origin;
        var position = origin + direction * 2.0f;

        Debug.Log("Tapped Event" + tapCount);
        if (saveButtonPrefab.activeSelf)
        {
            saveButtonPrefab.SetActive(false);

        }

        //    List <Mesh> meshes = spatialMappingManager.GetMeshes();
        //    saveButtonPrefab = Instantiate(saveButtonPrefab, position, Quaternion.identity);
        //    saveButtonPrefab.SetActive(true);
        //    Debug.Log("List" + meshes.Count);

        else
        {
            if ((SpatialUnderstanding.Instance.ScanState != SpatialUnderstanding.ScanStates.Scanning) ||
               (!SpatialUnderstanding.Instance.AllowSpatialUnderstanding))
            {
                SpatialUnderstanding.Instance.RequestFinishScan();
                return;
            }

            // Query the current playspace stats
            IntPtr statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
            if (Imports.QueryPlayspaceStats(statsPtr) == 0)
            {
                SpatialUnderstanding.Instance.RequestFinishScan();
                return;
            }

            if (meshInfoFetcher.IsQualityMesh(SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats()))
            {
                //meshInfoFetcher.FetchMeshInfoFromPerception(); // Not in use
                meshInfoFetcher.FetchMeshInfoFromUnderstanding();
                var test = meshInfoFetcher.PublishMeshInfoToFileAsync("meshFile");
                test.Start();
            }
           
        }
    }
    
    private void RequestAccessComp(IAsyncOperation<SpatialPerceptionAccessStatus> asyncInfo, AsyncStatus asyncStatus)
    {
        if (asyncInfo.GetResults().Equals(SpatialPerceptionAccessStatus.Allowed))
        {
            this.spatialAccepted = true;
            Debug.Log("User allowed");
            meshInfoFetcher.CreateBounding();
        }
        else
        {
            Debug.Log("User Not Allowed");
        }
    }
}