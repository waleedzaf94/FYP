using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace Assets.Scripts
{
    class MeshRenderScript : MonoBehaviour
    {


        [Header("Hologram")]
        public GameObject MeshHolder;
        public Material[] MeshMaterials;
        public MeshFilter MeshFilterPrefab;

        private string meshStringList;
        private List<Mesh> meshList;
        private List<GameObject> previousObjects;
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
            previousObjects = new List<GameObject>();
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
                foreach (GameObject g in previousObjects)
                {
                    Debug.Log(g.name);
                    Destroy(g.gameObject);
                }
                previousObjects.Clear();
                List<string> meshStringList = splitMesh(Filename);
                foreach (string meshString in meshStringList)
                {
                    meshList.Add(new ObjImporter().ImportFile(meshString));
                }
                foreach(Mesh m in meshList)
                {
                    GameObject go = new GameObject("ParentPrefab" + m.name);
                    MeshFilter mf = go.AddComponent<MeshFilter>();
                    BoxCollider bc = go.AddComponent<BoxCollider>();
                    bc.center = Vector3.zero;
                    go.transform.SetParent(MeshHolder.transform, false);
                    MeshRenderer mr = go.AddComponent<MeshRenderer>();
                    mr.material = MeshMaterials[0];
                    mr.allowOcclusionWhenDynamic = true;
                    mr.receiveShadows = true;
                    mf.mesh = m;
                    previousObjects.Add(go);
                }
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