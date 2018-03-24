using UnityEngine;
using HoloToolkit.Unity;
using UnityEngine.SceneManagement;
using System;

namespace Assets.Scripts
{
    class ActionScript : MonoBehaviour
    {
        private string localPath;
        [SerializeField]
        public SpatialUnderstanding spatial;

       

        private void Start()
        {
            Debug.Log("Initialized Action Script");
        }

        public void TappedStartScan()
        {
            //if (!ViewManager.RecordingView.activeSelf)
            //{
            //    ViewManager.InitializeRecording();
            //}
            SpatialUnderstanding instance;
            if (!SpatialUnderstanding.IsInitialized)
            {
                instance = Instantiate(spatial);
            }
            else
            {
                instance = SpatialUnderstanding.Instance;

            }
            SpatialUnderstanding.Instance.RequestBeginScanning();
            Debug.Log("Spatial State " + SpatialUnderstanding.Instance.ScanState);
        }

        public void TappedReset()
        {
            Debug.Log("Tapped Reset");
            ViewManager.Instance.ResetMesh();
        }

        public void TappedLibrary()
        {
            ViewManager.Instance.InitializeLibrary();
            StorageService.Instance.GetBlobList();
            if (SpatialUnderstanding.IsInitialized && SpatialUnderstanding.Instance.ScanState != SpatialUnderstanding.ScanStates.None)
            {
                SpatialUnderstanding.Instance.RequestFinishScan();
            }
        }

        public void TappedSave()
        {
            if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done)
            {
                DebugDialog.Instance.PrimaryText = "Saving Mesh...";
                RoomSaver.Instance.fileName = "mesh_save_test" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                Debug.Log("Room Saver is Initialized");
                Debug.Log("Saving to file" + RoomSaver.Instance.fileName);
                RoomSaver.Instance.anchorStoreName = "mesh_test_anchor";
                localPath = RoomSaver.Instance.SaveRoom();
                Debug.Log("File Name: " + localPath);
                StorageService.Instance.PutObjectBlob(localPath);
            }
        }

        public void TappedFinalize()
        {
            if ((SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning) &&
                !SpatialUnderstanding.Instance.ScanStatsReportStillWorking)
            {
                SpatialUnderstanding.Instance.RequestFinishScan();
            }
        }

        public void TappedTestObject()
        {
            ViewManager.Instance.InitializeVisualization();
        }

        public void TappedHelp()
        {

        }

        public void TappedRecordingView()
        {
            ViewManager.Instance.InitializeRecording();
        }

        public void TappedRefresh()
        {
            StorageService.Instance.GetBlobList();
        }

        public void TappedBackToLibrary()
        {
            ViewManager.Instance.InitializeLibrary();
        }


        public void TappedMeshInfo()
        {

        }

        public void TappedChangeView()
        {

        }

    }
}