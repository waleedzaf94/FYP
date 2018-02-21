using UnityEngine;
using HoloToolkit.Unity;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    class ActionScript : MonoBehaviour
    {
        [Header("Services")]
        [Tooltip("Attach the Azure Service Here")]
        public StorageService storage;
        private string localPath;
        private RoomSaver roomSaver;
        public GameObject library;


        void Start()
        {
            roomSaver = gameObject.AddComponent<RoomSaver>();
            library.SetActive(false);
        }

        void Update()
        {
        }


        public void TappedStartScan()
        {
            SpatialUnderstandingState.Instance.ShowMesh();

            SpatialUnderstanding.Instance.RequestBeginScanning();
        }

        public void TappedReset()
        {
        }

        public void TappedLibrary()
        {
            if (SpatialUnderstanding.Instance.ScanState != SpatialUnderstanding.ScanStates.None)
            {
                SpatialUnderstanding.Instance.RequestFinishScan();
            }
            SpatialUnderstandingState.Instance.HideMesh();
            storage.GetBlobList();
            library.SetActive(true);
        }

        public void TappedSave()
        {
            if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done)
            {
                roomSaver.fileName = "mesh_save_test";
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

    }
}