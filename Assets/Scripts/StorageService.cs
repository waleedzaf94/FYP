using Azure.StorageServices;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;
using RESTClient;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts
{
    class StorageService : Singleton<StorageService>
    {
        [Header("Azure Storage Service")]
        [SerializeField]
        private string storageAccount;
        [SerializeField]
        private string accessKey;
        [SerializeField]
        public string InputContainer;
        [SerializeField]
        private string OutputContainer;

        private StorageServiceClient client;
        private BlobService blobService;


        [Header("Library Fields")]
        [SerializeField]
        private PopulateLibrary LibraryManager;
        private string currentFile;


        private MeshInfo currentOutputMesh;
        private MeshInfo currentInputMesh;

        public MeshInfo CurrentInputMesh
        {
            get
            {
                return currentInputMesh;
            }

            set
            {
                currentInputMesh = value;
            }
        }

        public MeshInfo CurrentOutputMesh
        {
            get
            {
                return currentOutputMesh;
            }

            set
            {
                currentOutputMesh = value;
            }
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        void Start()
        {
            client = StorageServiceClient.Create(storageAccount, accessKey);
            blobService = client.GetBlobService();
            GetBlobList();
            currentFile = ""; 
        }

        public void PutObjectBlob(MeshInfo mesh)
        {
            CurrentInputMesh = mesh;
            string filename = mesh.filename;
            string stringArray = File.ReadAllText(mesh.localpath);
            Debug.Log("filename gotten: " + filename);
            // This is a lie
            DebugDialog.Instance.PrimaryText = "Mesh Saved Successfully!";
            StartCoroutine(blobService.PutTextBlob(PutObjectCompleted, stringArray, InputContainer, filename));
        }

        private void PutObjectCompleted(RestResponse obj)
        {
            Debug.Log("Status Code: " + obj.StatusCode);
            Debug.Log(obj.Content);
            if (obj.IsError)
                Debug.Log(obj.ErrorMessage);

            // TODO find full file url
            CurrentInputMesh.inputContainer = InputContainer;
            CurrentInputMesh = null;
        }

        internal void GetBlobAsText(string filename)
        {
            if (currentFile.Equals(filename))
            {
                ViewManager.Instance.InitializeVisualization();
                return;
            }
            else
            {
                //string outputContainer = string.IsNullOrEmpty(mesh.outputContainer) ? OutputContainer : mesh.outputContainer  ;
                DebugDialog.Instance.PrimaryText = "Retrieving Mesh...";
                string resourcePath = OutputContainer + "/" + filename;
                currentFile = filename;
                //currentOutputMesh = mesh;
                //MeshRenderScript.Instance.MeshInformation = CurrentOutputMesh;
                StartCoroutine(blobService.GetTextBlob(GetTextBlobCompleteAsync, resourcePath));
            }
        }

        private async void GetTextBlobCompleteAsync(RestResponse response)
        {
            if (response.IsError)
            {
                Debug.Log(response.ErrorMessage + " Error getting blob:" + response.Content);
                return;
            }
            Debug.Log("Received blob:" + response.Content.Length);
            string filename = await MeshSaver.SaveStringAsTemporaryMeshAsync(response.Content);
            Debug.Log("Mesh Saved At " + filename);
            MeshRenderScript.Instance.Filename = filename;
            ViewManager.Instance.InitializeVisualization();
        }


        internal void GetBlobList()
        {
            StartCoroutine(blobService.ListBlobs(ListBlobsCompleted, OutputContainer));
        }

        private void ListBlobsCompleted(IRestResponse<BlobResults> response)
        {
            if (response.IsError)
            {
                //Log.Text(label, "Failed to get list of blobs", "List blob error: " + response.ErrorMessage, Log.Level.Error);
                Debug.Log("Error on fetch from blob");
                return;
            }
            //Log.Text(label, "Loaded blobs: " + response.Data.Blobs.Length, "Loaded blobs: " + response.Data.Blobs.Length);
            LibraryManager.SetBlobs(response.Data.Blobs);
        }
    }
}
