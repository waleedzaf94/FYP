using UnityEngine;
using HoloToolkit.Unity;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections;

namespace Assets.Scripts
{
    class ActionScript : MonoBehaviour
    {
        private string localPath;
        [SerializeField]
        public SpatialUnderstanding spatial;
        private bool _resetSpatialUnderstanding;
        private bool _startScan;
        private bool _tappedHelp;

        private void Start()
        {
            _resetSpatialUnderstanding = false;
            _tappedHelp = false;
        }

        private void Update()
        {
            if (_resetSpatialUnderstanding)
            {
                ResetScanner();
            }
            if (_startScan)
            {
                StartScanner();
            }
        }

        private void StartScanner()
        {
            SpatialUnderstandingState.Instance.HideText = false;
            if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done ||
                 SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.None
                )
            {
                SpatialUnderstanding.Instance.RequestBeginScanning();
                _startScan = false;
            }
            //Debug.Log("Spatial State " + SpatialUnderstanding.Instance.ScanState);
        }

        private void ResetScanner()
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
                //Debug.Log("Spatial State " + SpatialUnderstanding.Instance.ScanState);
            }
            SpatialUnderstandingState.Instance.MeshSaving = false;
        }

        public void TappedStartScan()
        {
            _startScan = true;
            ViewManager.Instance.ShowMesh();
            Debug.Log("Spatial State " + SpatialUnderstanding.Instance.ScanState);
            SpatialUnderstandingState.Instance.MeshSaving = false;
        }

        public void TappedReset()
        {
            Debug.Log("Tapped Reset");
            _resetSpatialUnderstanding = true;
        }

        public void TappedLibrary()
        {
            ViewManager.Instance.InitializeLibrary();
            //CosmoScript.Instance.QueryMeshCollection();
            StorageService.Instance.GetBlobList();
            if (SpatialUnderstanding.IsInitialized && SpatialUnderstanding.Instance.ScanState != SpatialUnderstanding.ScanStates.None)
            {
                SpatialUnderstanding.Instance.RequestFinishScan();
            }
        }

        public async Task TappedSave()
        {
            if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done)
            {
                SpatialUnderstandingState.Instance.MeshSaving = true;
                // Save the file temporarily
                DebugDialog.Instance.PrimaryText = "Saving Mesh...";
                string rowkey = DateTime.Now.ToString("yyyyMMdd");
                string fn = "mesh_" + rowkey + "T" +DateTime.Now.ToString("HHmm");
                RoomSaver.Instance.fileName = fn;
                RoomSaver.Instance.anchorStoreName = "mesh_test_anchor";
                string metadata = RoomSaver.Instance.GetStatsAsString();
                string  localpath = await RoomSaver.Instance.SaveRoomAsync(metadata);
                Debug.Log("File Name: " + localPath);
                Debug.Log("MeshInfo " + fn);
                // Prepare file for push to Azure Storage
                MeshInfo tempInfo = new MeshInfo(rowkey)
                {
                    playspaceStats = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats(),
                    metadata = metadata,
                    localpath = localpath,
                    filename = fn+".obj" //Not working Path.GetFileName(localPath)
                };
                Debug.Log(tempInfo.filename);
                //Debug.Log("Wall Area " + SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats().WallSurfaceArea);
                StorageService.Instance.PutObjectToBlob(tempInfo);
            }
        }

        public void TappedFinalize()
        {
            if ((SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning) &&
                !SpatialUnderstanding.Instance.ScanStatsReportStillWorking)
            {
                if (SpatialUnderstandingState.Instance.DoesScanMeetMinBarForCompletion)
                {
                    SpatialUnderstanding.Instance.RequestFinishScan();
                }
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
            if (!_tappedHelp)
            {
                _tappedHelp = true;
                DebugDialog.Instance.PrimaryText = @"Welcome to HISS. 
Please select a model from panel to view or press Record to create a new recording.
Press Record again and scan your room. Wait for Finalize to appear before clicking finalize.
On complete, click save to upload mesh to server.
";
            }
            else
            {
                _tappedHelp = false;
                DebugDialog.Instance.ClearText();
            }
        }

        public void TappedRecordingView()
        {
            ViewManager.Instance.InitializeRecording();
        }

        public void TappedRefresh()
        {
            // CosmoScript.Instance.QueryMeshCollection();
            StorageService.Instance.GetBlobList();
        }

        public void TappedBackToLibrary()
        {
            ViewManager.Instance.InitializeLibrary();
        }

        public void TappedMeshInfo() => ModelViewer.Instance.meshInfoText.gameObject.SetActive(true);

        public void TappedRotateX()
        {
            ModelViewer.Instance.ToggleRotate("x");
        }

        public void TappedRotateY()
        {
            ModelViewer.Instance.ToggleRotate("y");
        }

    }
}