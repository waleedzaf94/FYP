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
        private bool _resetSpatialUnderstanding;
        private bool _startScan;

        private void Start()
        {
            _resetSpatialUnderstanding = false;
        }

        private void Update()
        {
            if (_resetSpatialUnderstanding)
            {
                SpatialUnderstandingState.Instance.HideText = true;
                if ((SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning) &&
                   !SpatialUnderstanding.Instance.ScanStatsReportStillWorking)
                {
                    Debug.Log("Resetting Mesh");
                    DebugDialog.Instance.PrimaryText = "Resetting Mesh...";
                    SpatialUnderstanding.Instance.RequestFinishScan();
                }
                if
                    (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.ReadyToScan ||
                        SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done
                    )
                {
                    ViewManager.Instance.HideMesh();
                    DebugDialog.Instance.PrimaryText = "Reset Complete";
                    Debug.Log("Reset Complete");
                    _resetSpatialUnderstanding = false;
                    Debug.Log("Spatial State " + SpatialUnderstanding.Instance.ScanState);
                }
            }
            if (_startScan)
            {
                SpatialUnderstandingState.Instance.HideText = false;

                if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done ||
                     SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.None
                    )
                {
                    SpatialUnderstanding.Instance.RequestBeginScanning();
                    _startScan = false;
                }
                Debug.Log("Spatial State " + SpatialUnderstanding.Instance.ScanState);
            }
        }

        public void TappedStartScan()
        {
            _startScan = true;
            ViewManager.Instance.ShowMesh();
            Debug.Log("Spatial State " + SpatialUnderstanding.Instance.ScanState);
        }

        public void TappedReset()
        {
            Debug.Log("Tapped Reset");
            _resetSpatialUnderstanding = true;
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
                RoomSaver.Instance.fileName = "mesh_save_test" + DateTime.Now.ToString("yyyy_MM_dd_HH:mm:ss");
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

        public void TappedHologram()
        {
        }

        public void TappedHelp()
        {
            DebugDialog.Instance.PrimaryText = "Help Opened";
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

        public void TappedRotateX()
        {
            MeshRenderScript.Instance.ToggleRotate("x");
        }

        public void TappedRotateY()
        {
            MeshRenderScript.Instance.ToggleRotate("y");
        }

    }
}