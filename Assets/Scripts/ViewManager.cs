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
        public SpatialUnderstandingCustomMesh SpatialUnderstandingMesh;
        public Material OccludedMaterial;
        public Material MeshMaterial;

        private bool _timeToHideMesh;
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
            InitializeLibrary();
        }

        private void Update()
        {
            if (_timeToHideMesh)
            {
                HideMesh();
                _timeToHideMesh = false;
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
        }

        public void ShowMesh()
        {           
            Debug.Log("Calling Show");
            SpatialUnderstandingMesh.DrawProcessedMesh = true;
        }
    }
}
