using HoloToolkit.Unity.SpatialMapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    static class ObjLoader
    {
        //Step 1 
        //dynamic display a cube DONE -> read cube from string DONE -> read cube from obj file DONE -> read multiple cubes from obj file DONE
        //read regular obj file 

        public static  Mesh CreateMesh(float width = 5f, float height = 5f)
        {
            Mesh m = new Mesh();
            m.name = "ScriptedMesh";
            m.vertices = new Vector3[] {
                 new Vector3(-width, -height, 0.01f),
                 new Vector3(width, -height, 0.01f),
                 new Vector3(width, height, 0.01f),
                 new Vector3(-width, height, 0.01f)
            };
            m.uv = new Vector2[] {
                 new Vector2 (0, 0),
                 new Vector2 (0, 1),
                 new Vector2(1, 1),
                 new Vector2 (1, 0)
             };
            m.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
            m.RecalculateNormals();
            m.RecalculateBounds();
            Debug.Log("Mesh Cube Created");
            return m;
        }


        public static GameObject CreateMeshObject(Material mat, float width = 0.05f, float height = 0.05f)
        {
            GameObject go = new GameObject("ParentPrefab");
            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = mat;
            mf.mesh = CreateMesh(width, height);
            return go;
        }

        public static GameObject ImportMeshObject(Material mat, string filepath)
        {
            GameObject go = new GameObject("ParentPrefab"+filepath);
            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = mat;
            Debug.Log("Starting Import");
            mf.mesh = new ObjImporter().ImportFile(filepath);
            Debug.Log(go.activeSelf + "IMport Success?");
            return go;
        }
    }
}
