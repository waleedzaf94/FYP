using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Assets.Scripts
{
    class MeshRenderScript : MonoBehaviour
    {


        [Header("Hologram")]
        public GameObject MeshHolder;
        public Material[] MeshMaterials;

        private string filename;

        public string Filename {
            get {
                return filename;
            }
            set {
                Debug.Log("Filename reset to " + value);
                filename = value;
                _filechanged = true;
            }
        }

        private bool _filechanged = false;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (_filechanged)
            {
                if (filename.Length > 0)
                {
                    _filechanged = false;
                    foreach (Transform child in MeshHolder.transform)
                    {
                        Destroy(child.gameObject);
                    }

                    GameObject mesh = OBJLoader.LoadOBJFile(filename, MeshMaterials);
                    mesh.transform.localPosition = new Vector3(0, -0.2f, 1.5f);
                    mesh.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    Instantiate(mesh, MeshHolder.transform);
                    Debug.Log("Mesh Instantiated");
                    foreach (Transform child in MeshHolder.transform)
                    {
                        child.gameObject.SetActive(true);
                    }
                    //Mesh holderMesh = new Mesh();
                    //ObjImporter newMesh = new ObjImporter();
                    //holderMesh = newMesh.ImportFile(filename);

                    //MeshRenderer renderer = MeshHolder.AddComponent<MeshRenderer>();
                    //MeshFilter filter = MeshHolder.AddComponent<MeshFilter>();
                    //filter.mesh = holderMesh;
                }
            }
        }
    }

}