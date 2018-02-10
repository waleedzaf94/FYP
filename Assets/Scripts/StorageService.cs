using Azure.StorageServices;
using RESTClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private string container;

        private StorageServiceClient client;
        private BlobService blobService;

        void Start()
        {
            client = StorageServiceClient.Create(storageAccount, accessKey);
            blobService = client.GetBlobService();
        }

        public void PutObjectBlob(string localPath)
        {
            string filename = Path.GetFileName(localPath);
            string stringArray = File.ReadAllText(localPath);
            Debug.Log("filename gotten: " + filename);
            StartCoroutine(blobService.PutTextBlob(PutObjectCompleted, stringArray, container, filename));
            //StartCoroutine(blobService.PutImageBlob(PutImageCompleted, objectBytes, container, filename, "image/png"));
        }

        public void PutObjectCompleted(RestResponse obj)
        {
            //throw new NotImplementedException();
            Debug.Log(obj.StatusCode);
            if (obj.IsError)
                Debug.Log(obj.ErrorMessage);
        }
    }
}
