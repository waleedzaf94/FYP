using UnityEngine;
using HoloToolkit.Unity;

namespace Assets.Scripts
{
    class ActionScript : MonoBehaviour
    {
        [Header("Services")]
        [Tooltip("Attach the Azure Service Here")]
        public StorageService storage;

        private string localPath;
        private RoomSaver roomSaver;

        void Start()
        {
            roomSaver = gameObject.AddComponent<RoomSaver>();
        }

        void Update()
        {
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