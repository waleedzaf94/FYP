using HoloToolkit.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    class ViewManager : Singleton<ViewManager>
    {
        public GameObject LibraryView;
        public GameObject RecordingView;
        public GameObject VisualizationView;
        public GameObject SpatialUnderstandingObject;
        public GameObject SpatialUnderstandingPrefab;
        public Transform SpatialTransform;
        private bool _timeToHideMesh;

        public SpatialUnderstandingCustomMesh SpatialUnderstandingMesh;
        public Material OccludedMaterial;
        public Material MeshMaterial;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private void Start()
        {
            _timeToHideMesh = true;
            //ToggleChildren(LibraryView, false);
            LibraryView.SetActive(true);
            VisualizationView.SetActive(false);
            RecordingView.SetActive(false);
        }

        private void ToggleChildren(GameObject parent, bool visibility)
        {
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                var child = parent.transform.GetChild(i).gameObject;
                if (child != null)
                    child.SetActive(visibility);
            }
            Debug.Log(visibility + " Hiding/Showing all children - done  " + parent.transform.childCount);
        }

        private void Update()
        {
            if (_timeToHideMesh)
            {
                _timeToHideMesh = false;
                HideMesh();
                Debug.Log("Updating manager");
            }
        }

        public void InitializeLibrary()
        {
            DebugDialog.Instance.ClearText();
            Debug.Log("Library View Called");
            _timeToHideMesh = true;
            RecordingView.SetActive(false);
            //ToggleChildren(RecordingView, true);
            VisualizationView.SetActive(false);
            LibraryView.SetActive(true);
        }

        public void InitializeVisualization()
        {
            DebugDialog.Instance.ClearText();
            _timeToHideMesh = true;
            Debug.Log("Visualization View Called");
            RecordingView.SetActive(false);
            VisualizationView.SetActive(true);
            LibraryView.SetActive(false);
        }

        public void InitializeRecording()
        {
            DebugDialog.Instance.ClearText();
            Debug.Log("Recording View Called");
            _timeToHideMesh = false;
            //ResetMesh();
            ShowMesh();
            RecordingView.SetActive(true);
            VisualizationView.SetActive(false);
            LibraryView.SetActive(false);
        }

        public void HideMesh()
        {
            Debug.Log("Calling Hide");

            SpatialUnderstandingMesh.DrawProcessedMesh = false;
            //SpatialUnderstanding.Instance.UnderstandingCustomMesh.MeshMaterial = OccludedMaterial;
            //SpatialUnderstandingMesh.MeshMaterial = OccludedMaterial;
            //_timeToHideMesh = false;
        }

        public void ShowMesh()
        {           
            Debug.Log("Calling Show");
            SpatialUnderstandingMesh.DrawProcessedMesh = true;
        }

        public void ResetMesh()
        {
            Debug.Log("Resetting mesh");
            //NEED TO PUT CHECK FOR STATE HERE
            HideMesh();
            Destroy(SpatialUnderstanding.Instance);

            //SpatialUnderstanding spatial = gameObject.GetComponent<SpatialUnderstanding>();
            //Transform parent = SpatialUnderstandingObject.GetParentRoot().transform;
            //DestroyImmediate(SpatialUnderstandingObject);
            //GameObject newSpatial = Instantiate(SpatialUnderstandingPrefab, SpatialTransform);
            //SpatialUnderstanding.Instance.UnityFastInvoke_Awake();
            //SpatialUnderstanding newSpatial = gameObject.AddComponent<SpatialUnderstanding>();
            //Instantiate(gameObject.AddComponent<SpatialUnderstanding>());
            //SpatialUnderstandingMesh = newSpatial.UnderstandingCustomMesh;
        }

    }
}
