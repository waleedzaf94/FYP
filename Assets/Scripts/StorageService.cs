using Azure.StorageServices;
using HoloToolkit.Unity.SpatialMapping;
using RESTClient;
using System.IO;
using UnityEngine;

namespace Assets.Scripts
{
    class StorageService : MonoBehaviour
    {
        [Header("Azure Storage Service")]
        [SerializeField]
        private string storageAccount;
        [SerializeField]
        private string accessKey;
        [SerializeField]
        private string InputContainer;
        [SerializeField]
        private string OutputContainer;

        private StorageServiceClient client;
        private BlobService blobService;


        [Header("Library Fields")]
        [SerializeField]
        private TextMesh Label;
        [SerializeField]
        private PopulateLibrary LibraryManager;
        [SerializeField]
        private MeshRenderScript MeshRenderHolder;


        void Start()
        {
            client = StorageServiceClient.Create(storageAccount, accessKey);
            blobService = client.GetBlobService();
            GetBlobList();
        }

        public void PutObjectBlob(string localPath)
        {
            string filename = Path.GetFileName(localPath);
            string stringArray = File.ReadAllText(localPath);
            Debug.Log("filename gotten: " + filename);
            StartCoroutine(blobService.PutTextBlob(PutObjectCompleted, stringArray, InputContainer, filename));
        }

        internal void GetBlobList()
        {
            StartCoroutine(blobService.ListBlobs(ListBlobsCompleted, OutputContainer));
        }

        internal void GetBlobAsText(string filename)
        {
            string resourcePath = OutputContainer + "/" + filename;
            Label.text = "Loading Mesh...";
            StartCoroutine(blobService.GetTextBlob(GetTextBlobComplete, resourcePath));
        }

        private void GetTextBlobComplete(RestResponse response)
        {
            if (response.IsError)
            {
                Debug.Log(response.ErrorMessage + " Error getting blob:" + response.Content);
                return;
            }
            Debug.Log("Get blob:" + response.Content.Length);
            string filename = MeshSaver.SaveStringAsTemporaryMesh(response.Content);
            Debug.Log("Mesh Saved At " + filename);
            MeshRenderHolder.Filename = filename;
        }

        public void PutObjectCompleted(RestResponse obj)
        {
            Debug.Log(obj.StatusCode);
            if (obj.IsError)
                Debug.Log(obj.ErrorMessage);
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
            ReloadBlobList(response.Data.Blobs);
        }

        private void ReloadBlobList(Blob[] blobs)
        {
            //label.text = "Blobs received: " + blobs.Length;
            LibraryManager.SetBlobs(blobs);
        }
    }
}
