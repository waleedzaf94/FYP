using HoloToolkit.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    class ViewManager : MonoBehaviour
    {
        public GameObject LibraryView;
        public GameObject RecordingView;
        public GameObject VisualizationView;


        private bool _timeToHideMesh;

        public SpatialUnderstandingCustomMesh SpatialUnderstandingMesh;
        public Material OccludedMaterial;
        public Material MeshMaterial;

        private void Start()
        {
            _timeToHideMesh = true;
            //ToggleChildren(LibraryView, false);
            LibraryView.SetActive(false);
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
            Debug.Log("Library View Called");
            HideMesh();
            RecordingView.SetActive(false);
            //ToggleChildren(RecordingView, true);
            VisualizationView.SetActive(false);
            LibraryView.SetActive(true);
        }

        public void InitializeVisualization()
        {
            HideMesh();
            Debug.Log("Visualization View Called");
            RecordingView.SetActive(false);
            VisualizationView.SetActive(true);
            LibraryView.SetActive(false);
        }

        public void InitializeRecording()
        {
            Debug.Log("Recording View Called");
            _timeToHideMesh = false;
            ResetMesh();
            ShowMesh();
            RecordingView.SetActive(true);
            VisualizationView.SetActive(false);
            LibraryView.SetActive(false);
        }

        public void HideMesh()
        {
            //SpatialUnderstanding.Instance.UnderstandingCustomMesh.MeshMaterial = OccludedMaterial;
            //SpatialUnderstandingMesh.MeshMaterial = OccludedMaterial;
            Debug.Log("Calling Hide");
            //_timeToHideMesh = false;
        }

        public void ShowMesh()
        {
            SpatialUnderstandingMesh.MeshMaterial = MeshMaterial;
            Debug.Log("Calling Show");
        }

        public void ResetMesh()
        {
            Debug.Log("Resetting mesh");
            //SpatialUnderstanding spatial = gameObject.GetComponent<SpatialUnderstanding>();
            //Destroy(SpatialUnderstanding.Instance);
            SpatialUnderstanding.Instance.UnityFastInvoke_Awake();
            //SpatialUnderstanding newSpatial = gameObject.AddComponent<SpatialUnderstanding>();
            //Instantiate(gameObject.AddComponent<SpatialUnderstanding>());
            //SpatialUnderstandingMesh = newSpatial.UnderstandingCustomMesh;
        }

    }
}
