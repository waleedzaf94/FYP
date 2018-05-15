using HoloToolkit.Unity;
using UnityEngine;


using System.Collections.Generic;
using HoloToolkit.Unity.SpatialMapping;
using UnityEngine.XR.WSA.Persistence;
using System.Threading.Tasks;
using System;
using System.Text;

namespace Assets.Scripts
{

    public class RoomSaver : Singleton<RoomSaver>
    {

        public string fileName { get; set; }             // name of file to store meshes
        public string anchorStoreName { get; set; }      // name of world anchor to store for room

        List<MeshFilter> roomMeshFilters;
        WorldAnchorStore anchorStore;
        int meshCount = 0;

        // Use this for initialization
        void Start()
        {
            Debug.Log("Start Called");
            WorldAnchorStore.GetAsync(AnchorStoreReady);
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        void AnchorStoreReady(UnityEngine.XR.WSA.Persistence.WorldAnchorStore store)
        {
            Debug.Log("Anchor Called");
            anchorStore = store;
        }

        public async Task<string> SaveRoomAsync(string metadata = "")
        {
            // if the anchor store is not ready then we cannot save the room mesh
            //if (anchorStore == null)
            //{
            //    Debug.Log("Anchor Store Issue");
            //    return null;
            //}

            //// delete old relevant anchors
            //string[] anchorIds = anchorStore.GetAllIds();
            //for (int i = 0; i < anchorIds.Length; i++)
            //{
            //    if (anchorIds[i].Contains(anchorStoreName))
            //    {
            //        anchorStore.Delete(anchorIds[i]);
            //    }
            //}

            Debug.Log("Old anchors deleted...");

            // get all mesh filters used for spatial mapping meshes
            roomMeshFilters = SpatialUnderstanding.Instance.UnderstandingCustomMesh.GetMeshFilters() as List<MeshFilter>;
            Debug.Log("Mesh filters fetched...");

            // create new list of room meshes for serialization
            List<Mesh> roomMeshes = new List<Mesh>();

            // cycle through all room mesh filters
            foreach (MeshFilter filter in roomMeshFilters)
            {
                // increase count of meshes in room
                meshCount++;

                string meshName = anchorStoreName + meshCount.ToString();
                filter.mesh.name = meshName;
                
                roomMeshes.Add(filter.mesh);
            }
             return await MeshSaver.SaveAsObjAsync(fileName, roomMeshes, metadata);
            // serialize and save meshes
        }

        private void AttachingAnchor_OnTrackingChanged(UnityEngine.XR.WSA.WorldAnchor self, bool located)
        {
            if (located)
            {
                string meshName = self.gameObject.GetComponent<MeshFilter>().mesh.name;
                if (!anchorStore.Save(meshName, self))
                    Debug.Log("" + meshName + ": Anchor save failed...");
                else
                    Debug.Log("" + meshName + ": Anchor SAVED...");

                self.OnTrackingChanged -= AttachingAnchor_OnTrackingChanged;
            }
        }

        internal string GetStatsAsString()
        {
            var stats = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats();
            StringBuilder builder = new StringBuilder();
            //public float HorizSurfaceArea;              // In m2 : All horizontal faces UP between Ground – 0.15 and Ground + 1.f (include Ground and convenient horiz surface)
            //public float TotalSurfaceArea;              // In m2 : All !
            //public float UpSurfaceArea;                 // In m2 : All horizontal faces UP (no constraint => including ground)
            //public float DownSurfaceArea;               // In m2 : All horizontal faces DOWN (no constraint => including ceiling)
            //public float WallSurfaceArea;               // In m2 : All Vertical faces (not only walls)
            //public float VirtualCeilingSurfaceArea;     // In m2 : estimation of surface of virtual Ceiling.
            //public float VirtualWallSurfaceArea;        // In m2 : estimation of surface of virtual Walls.
            builder.AppendLine("# ms");
            builder.AppendLine($"# tsa {stats.TotalSurfaceArea}");
            builder.AppendLine($"# hsa {stats.HorizSurfaceArea}");
            builder.AppendLine($"# usa {stats.UpSurfaceArea}");
            builder.AppendLine($"# dsa {stats.DownSurfaceArea}");
            builder.AppendLine($"# wsa {stats.WallSurfaceArea}");
            builder.AppendLine($"# vcs {stats.VirtualCeilingSurfaceArea}");
            builder.AppendLine($"# vws {stats.VirtualWallSurfaceArea}");
            builder.AppendLine($"# ccs {stats.CellCount_IsSeenQualtiy_Seen}");
            builder.AppendLine($"# ccg {stats.CellCount_IsSeenQualtiy_Good}");
            builder.AppendLine("# es");
            string statInfo = builder.ToString();
            Debug.Log("statInfo" + statInfo);
            return builder.ToString();
        }
    }
}