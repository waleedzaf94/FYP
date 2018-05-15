using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
#if !UNITY_EDITOR
using System.Threading.Tasks;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    class ModelViewer : Singleton<ModelViewer>
    {
        [Header("Hologram")]
        public GameObject MeshHolder;
        public Material MeshMaterial;
        public Text meshInfoText;


        private List<Mesh> testMeshList;
        private string meshStringList;
        private bool _infoUpdated;
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
        private MeshInfo meshInfo;

        // Use this for initialization
        void Start()
        {
            meshList = new List<Mesh>();
            _filechanged = false;
            _rotateX = false;
            _rotateY = false;
            meshInfoText.text = "";
            testMeshList = new List<Mesh>();
        }

        // Update is called once per frame
        void Update()
        {
            Update_MeshAsync();
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

        private async void Update_MeshAsync()
        {
            if (_infoUpdated )
            {
                _infoUpdated = false;
                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"Floor Area         {meshInfo.HorizSurfaceArea}m\xB2");
                builder.AppendLine($"Wall Area          {meshInfo.WallSurfaceArea}m\xB2");
                builder.AppendLine($"Ceiling Area       {meshInfo.VirtualCeilingSurfaceArea}m\xB2");
                builder.AppendLine($"Total Surface Area {meshInfo.TotalSurfaceArea}m\xB2");
                Debug.Log(builder.ToString());
                meshInfoText.text = builder.ToString();
            }
            if (_filechanged)
            {
                _filechanged = false;
                DebugDialog.Instance.PrimaryText = "Generating Mesh...";
                testMeshList.Clear();
                meshInfoText.text = string.Empty;
                Component[] previous = MeshHolder.GetComponentsInChildren<MeshRenderer>();
                foreach (Component comp in previous)
                {
                    //Debug.Log("Destroying..." + comp.name);
                    Destroy(comp.gameObject);
                }
                List<string> meshStringList = SplitMesh(Filename);
                //                const string V = @"o Cube
                //v 1.000000 -1.000000 -1.000000
                //v 1.000000 -1.000000 1.000000
                //v -1.000000 -1.000000 1.000000
                //v -1.000000 -1.000000 -1.000000
                //v 1.000000 1.000000 -1.000000
                //v 0.999999 1.000000 1.000001
                //v -1.000000 1.000000 1.000000
                //v -1.000000 1.000000 -1.000000
                //vt 2.094305 1.396205
                //vt 1.396205 2.094305
                //vt 0.698100 1.396205
                //vt 1.396205 0.698100
                //vt 5.000000 5.000000
                //vt 2.792410 5.000000
                //vt 2.792410 2.792410
                //vt 5.000000 2.792410
                //vt 2.792410 2.094305
                //vt 2.094305 2.792410
                //vt 0.698100 2.792410
                //vt 0.000000 2.094305
                //vt 0.000000 0.698100
                //vt 0.698100 0.000000
                //vt 2.792410 0.698100
                //vt 2.094305 0.000000
                //f 1/1/1 2/2/2 3/3/3
                //f 1/1/1 3/3/3 4/4/4
                //f 5/5/5 8/8/6 7/7/7
                //f 5/5/5 7/7/7 6/6/8
                //f 1/1/1 5/5/9 6/6/10
                //f 1/1/1 6/6/10 2/2/2
                //f 2/2/2 6/6/11 7/7/12
                //f 2/2/2 7/7/12 3/3/3
                //f 3/3/3 7/7/13 8/8/14
                //f 3/3/3 8/8/14 4/4/4
                //f 5/5/15 1/1/1 4/4/4
                //f 5/5/15 4/4/4 8/8/16";
                //                List<string> meshStringList = new List<string>();
                //                meshStringList.Add(V) ;

                meshStringList.ForEach(i =>
                {
                    StartCoroutine(BuildSubmesh(i));
                });
                Debug.Log("Parent Generated");
                GazeStabilizer stabalizer = MeshHolder.AddComponent<GazeStabilizer>();
                DebugDialog.Instance.ClearText();
                Debug.Log("Mesh List" + testMeshList.Count);
                string t = await MeshSaver.SaveAsObjAsync("test_mesh_out", testMeshList);
                Debug.Log(t);
            }

        }

        public IEnumerator BuildSubmesh(string i)
        {
            Mesh m;
            //yield return 
            yield return (m = MeshLoader.BuildMesh(i));
            GenerateSubmesh(m, i);
            testMeshList.Add(m);
            Debug.Log("Added mesh to list");
            //Debug.Log("Coroutine started");
            //Debug.Log("Reached end of the thing");
        }

        private void GenerateSubmesh(Mesh m, string i)
        {
            GameObject go = new GameObject("ParentPrefab" + i.Substring(0, 10));
            GazeStabilizer stab = go.AddComponent<GazeStabilizer>();
            MeshFilter mf = go.AddComponent<MeshFilter>();
            //BoxCollider bc = go.AddComponent<BoxCollider>();
            //bc.center = Vector3.zero;
            go.transform.SetParent(MeshHolder.transform, false);
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = MeshMaterial;
            mr.allowOcclusionWhenDynamic = true;
            mr.receiveShadows = true;
            mf.mesh = m;
        }

        private List<string> SplitMesh(string filePath)
        {
            string meshInfo = File.ReadAllText(filePath);
            string meshes;
            ExtractInfo(meshInfo, out meshes);
            List<string> submeshInfo = new List<string>();
            string[] submeshes = meshes.Split('o');
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

        private void ExtractInfo(string original, out string meshes)
        {
            int i = original.IndexOf('o');
            if (i > 1)
            {
                string a = original.Substring(0, i - 1);
                meshes = original.Substring(i);
                string[] lines = a.Split('\n');
                //float HorizSurfaceArea = 0, TotalSurfaceArea = 0, UpSurfaceArea = 0;
                //float DownSurfaceArea = 0, WallSurfaceArea = 0, VirtualCeilingSurfaceArea = 0, VirtualWallSurfaceArea = 0;
                meshInfo = new MeshInfo();
                foreach (string line in lines)
                {
                    string l = line.Trim();
                    if (l == "#" || l == "") continue;
                    string[] words = Regex.Split(l, "\\s");
                    if (words.Length > 2)
                    {
                        string code = words[1];
                        int index = 2;
                        switch (code)
                        {
                            case "tsa":
                                meshInfo.TotalSurfaceArea = float.Parse(words[index]);
                                break;
                            case "hsa":
                                meshInfo.HorizSurfaceArea = float.Parse(words[index]);
                                break;
                            case "usa":
                                meshInfo.UpSurfaceArea = float.Parse(words[index]);
                                break;
                            case "dsa":
                                meshInfo.DownSurfaceArea = float.Parse(words[index]);
                                break;
                            case "wsa":
                                meshInfo.WallSurfaceArea = float.Parse(words[index]);
                                break;
                            case "vcs":
                                meshInfo.VirtualCeilingSurfaceArea = float.Parse(words[index]);
                                break;
                            case "vws":
                                meshInfo.VirtualWallSurfaceArea = float.Parse(words[index]);
                                break;
                        }
                    }
                }
                _infoUpdated = true;
                return;
            }
            meshes = original;
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