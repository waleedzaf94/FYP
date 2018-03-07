using System;
using System.Collections;
using System.Collections.Generic;
using Azure.StorageServices;
using UnityEngine;
using UnityEngine.UI;


namespace Assets.Scripts
{
    class PopulateLibrary : MonoBehaviour
    {

        [Header("Azure Storage Service")]
        public StorageService StorageService;

        private List<Blob> blobList;
        private bool _loaded;
        public Button prefab;
        private bool _populated;

        // Use this for initialization
        void Start()
        {
            blobList = new List<Blob>();
        }

        // Update is called once per frame
        void Update()
        {
            if (_loaded && !_populated)
            {
                PopulateGrid();
            }
        }

        public void SetBlobs(Blob[] blobs)
        {
            blobList = new List<Blob>(blobs);
            _loaded = true;
            blobList.ForEach(GetBlobInfo);
        }

        private void GetBlobInfo(Blob obj)
        {
            Debug.Log("Blob name: " + obj.Name);
            Debug.Log("Blob Properties: " + obj.Properties.BlobType);
        }

        private void PopulateGrid()
        {
            blobList.ForEach(MakePrefab);
            _populated = true;
        }

        private void MakePrefab(Blob obj)
        {
            Button newObject;
            newObject = Instantiate(prefab, transform);
            newObject.GetComponent<Button>().GetComponentInChildren<Text>().text = obj.Name;
            newObject.GetComponent<Button>().onClick.AddListener(() => ObjectClicked(obj));
            Debug.Log("Prefab Generated");
        }

        private void ObjectClicked(Blob selectedBlob)
        {
            Debug.Log("Selected: " + selectedBlob.Name);
            StorageService.GetBlobAsText(selectedBlob.Name);
        }
    }

}