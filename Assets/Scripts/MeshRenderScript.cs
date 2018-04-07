using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
#if !UNITY_EDITOR
using System.Threading.Tasks;
#endif
using UnityEngine;


namespace Assets.Scripts
{
    class MeshRenderScript : Singleton<MeshRenderScript>
    {
        [Header("Hologram")]
        public GameObject MeshHolder;
        public Material MeshMaterial;

        private string meshStringList;
        private List<Mesh> meshList;
        private bool _rotateX;
        private bool _rotateY;

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

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private bool _filechanged;

        // Use this for initialization
        void Start()
        {
            meshList = new List<Mesh>();
            _filechanged = false;
            _rotateX = false;
            _rotateY = false;
        }

        // Update is called once per frame
        void Update()
        {
            Update_Mesh();
            Rotate_Mesh();
        }

        private void Rotate_Mesh()
        {
            if (_rotateX)
            {
                MeshHolder.transform.Rotate(Vector3.up * Time.deltaTime * 5);
            }
            if (_rotateY)
            {
                MeshHolder.transform.Rotate(Vector3.right * Time.deltaTime * 5);
            }
        }

        private void Update_Mesh()
        {
            if (_filechanged)
            {
                _filechanged = false;
                DebugDialog.Instance.PrimaryText = "Generating Mesh...";

                Component[] previous = MeshHolder.GetComponentsInChildren<MeshRenderer>();
                foreach (Component comp in previous)
                {
                    //Debug.Log("Destroying..." + comp.name);
                    Destroy(comp.gameObject);
                }
                List<string> meshStringList = splitMesh(Filename);
#if !UNITY_EDITOR
                int j = 0;
                meshStringList.ForEach(i =>
                {
                    Mesh m = ObjImporter.ImportFileAsync(i);
                    GameObject go = new GameObject("ParentPrefab" + i.Substring(0, 10));
                    GazeStabilizer stab = go.AddComponent<GazeStabilizer>();
                    MeshFilter mf = go.AddComponent<MeshFilter>();
                    BoxCollider bc = go.AddComponent<BoxCollider>();
                    bc.center = Vector3.zero;
                    go.transform.SetParent(MeshHolder.transform, false);
                    MeshRenderer mr = go.AddComponent<MeshRenderer>();
                    mr.material = MeshMaterial;
                    mr.allowOcclusionWhenDynamic = true;
                    mr.receiveShadows = true;
                    mf.mesh = m;
                    j++;
                });
                Debug.Log("Parent Generated");
                GazeStabilizer stabalizer = MeshHolder.AddComponent<GazeStabilizer>();
#endif
                DebugDialog.Instance.ClearText();
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

        internal void ToggleRotate(string axis)
        {
            if (axis=="x")
            {
                _rotateX = !_rotateX;
                _rotateY = false;
            }
            if (axis=="y")
            {
                _rotateX = false;
                _rotateY = !_rotateY;
            }
        }
    }

}