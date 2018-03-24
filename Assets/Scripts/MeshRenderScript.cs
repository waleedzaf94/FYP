using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
#if !UNITY_EDITOR
using System.Threading.Tasks;
#endif
using UnityEngine;


namespace Assets.Scripts
{
    class MeshRenderScript : MonoBehaviour
    {
        [Header("Hologram")]
        public GameObject MeshHolder;
        public Material MeshMaterial;

        private string meshStringList;
        private List<Mesh> meshList;
        public string Filename {
            get {
                return meshStringList;
            }
            set {
                Debug.Log("Filename reset to " + value);
                meshStringList = value;
                _filechanged = true;
            }
        }

        private bool _filechanged;

        // Use this for initialization
        void Start()
        {
            meshList = new List<Mesh>();
            _filechanged = false;
        }

        // Update is called once per frame
        void Update()
        {
            Update_Mesh();
        }

        private void Update_Mesh()
        {
            if (_filechanged)
            {
                _filechanged = false;

                //Instantiate(ObjLoader.CreateMeshObject(MeshMaterials[0]), transform);
                foreach (Transform child in MeshHolder.transform)
                {
                    Destroy(child.gameObject);
                }
                List<string> meshStringList = splitMesh(Filename);
#if !UNITY_EDITOR
                var meshBag = new ConcurrentBag<Mesh>();
                List<Task> taskList = new List<Task>();
                meshStringList.ForEach(i =>
                {
                    GameObject go = new GameObject("ParentPrefab" + i.Substring(0,10));
                    Mesh m = ObjImporter.ImportFileAsync(i);
                    MeshFilter mf = go.AddComponent<MeshFilter>();
                    BoxCollider bc = go.AddComponent<BoxCollider>();
                    bc.center = Vector3.zero;
                    go.transform.SetParent(MeshHolder.transform, false);
                    MeshRenderer mr = go.AddComponent<MeshRenderer>();
                    mr.material = MeshMaterial;
                    mr.allowOcclusionWhenDynamic = true;
                    mr.receiveShadows = true;
                    mf.mesh = m;
                });
#endif
                DebugDialog.Instance.ClearText();
                ViewManager.Instance.InitializeVisualization();
            }

        }

        private List<string> splitMesh(string filePath)
        {
            string meshInfo = File.ReadAllText(filePath);
            List<string> submeshInfo = new List<string>();
            string[] submeshes = meshInfo.Split('o');
            foreach (string s in submeshes)
            {
                s.Trim();
                if (s.Length > 10)
                {
                    submeshInfo.Add(s);
                }
            }
            return submeshInfo;
        }
    }

}