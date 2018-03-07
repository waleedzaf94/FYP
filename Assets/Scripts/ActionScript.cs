using UnityEngine;
using HoloToolkit.Unity;
using UnityEngine.SceneManagement;
using System;

namespace Assets.Scripts
{
    class ActionScript : MonoBehaviour
    {
        [Header("Services")]
        [Tooltip("Attach the Azure Service Here")]
        public StorageService storage;
        [Tooltip("Attach the Vie Manager Here")]
        public ViewManager ViewManager;

        private string localPath;
        private RoomSaver roomSaver;


        void Start()
        {
            roomSaver = gameObject.AddComponent<RoomSaver>();
        }

        void Update()
        {
        }


        public void TappedStartScan()
        {
            if (!ViewManager.RecordingView.activeSelf)
            {
                ViewManager.InitializeRecording();
            }
            SpatialUnderstanding.Instance.RequestBeginScanning();
        }

        public void TappedReset()
        {
            Debug.Log("Tapped Reset");
            ViewManager.ResetMesh();
        }

        public void TappedLibrary()
        {
            ViewManager.InitializeLibrary();
            storage.GetBlobList();
            if (SpatialUnderstanding.Instance.ScanState != SpatialUnderstanding.ScanStates.None)
            {
                SpatialUnderstanding.Instance.RequestFinishScan();
            }
        }

        public void TappedSave()
        {
            if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done)
            {
                roomSaver.fileName = "mesh_save_test" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                Debug.Log("Saving to file" + roomSaver.fileName);
                roomSaver.anchorStoreName = "mesh_test_anchor";
                localPath = roomSaver.SaveRoom();
                Debug.Log("File Name: " + localPath);
                storage.PutObjectBlob(localPath);
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
            ViewManager.InitializeVisualization();
        }

        public void TappedHelp()
        {

        }

        public void TappedRecordingView()
        {
            ViewManager.InitializeRecording();
        }

        public void TappedRefresh()
        {
            storage.GetBlobList();
        }

        public void TappedBackToLibrary()
        {
            ViewManager.InitializeLibrary();
        }


        public void TappedMeshInfo()
        {

        }

        public void TappedChangeView()
        {

        }

    }
}