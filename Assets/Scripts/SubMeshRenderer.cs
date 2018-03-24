using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if (WINDOWS_UWP) 
using System.Threading.Tasks;
#endif
using UnityEngine;

namespace Assets.Scripts
{
    class SubMeshRenderer : MonoBehaviour
    {
        private bool _rendered;

        public string MeshInfoString { set; get; }
        public GameObject parent { get; set; }
        public MeshFilter MeshFilterPrefab { get; private set; }
        public void Start()
        {
            _rendered = false;
        }

        public void Update()
        {
            if (!_rendered)
            {
                if (!String.IsNullOrEmpty(MeshInfoString))
                {
                    MeshFilterPrefab = new MeshFilter();
                    _rendered = true;
                }
            }
        }

        private void CreateSubMesh()
        {
            Mesh m = ObjImporter.ImportFileAsync(MeshInfoString);
            if (MeshFilterPrefab != null)
            {
                Debug.Log("Adding");
                MeshFilterPrefab.mesh = m;
                Instantiate(MeshFilterPrefab, parent.transform);
                _rendered = true;
            }
        }
    }
}
