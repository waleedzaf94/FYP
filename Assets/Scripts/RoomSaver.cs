using HoloToolkit.Unity;
using UnityEngine;


using System.Collections.Generic;
using HoloToolkit.Unity.SpatialMapping;
using UnityEngine.XR.WSA.Persistence;

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

        public string SaveRoom()
        {
            // if the anchor store is not ready then we cannot save the room mesh
            if (anchorStore == null)
            {
                Debug.Log("Anchor Store Issue");
                return null;
            }

            // delete old relevant anchors
            string[] anchorIds = anchorStore.GetAllIds();
            for (int i = 0; i < anchorIds.Length; i++)
            {
                if (anchorIds[i].Contains(anchorStoreName))
                {
                    anchorStore.Delete(anchorIds[i]);
                }
            }

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

                // make mesh name = anchor name + mesh count
                string meshName = anchorStoreName + meshCount.ToString();
                filter.mesh.name = meshName;

                //Debug.Log("Mesh " + filter.mesh.name + ": " + filter.transform.position + "\n--- rotation " + filter.transform.localRotation + "\n--- scale: " + filter.transform.localScale);
                // add mesh to room meshes for serialization
                roomMeshes.Add(filter.mesh);

                // save world anchor
                //WorldAnchor attachingAnchor = filter.gameObject.GetComponent<WorldAnchor>();
                //if (attachingAnchor == null)
                //{
                //    attachingAnchor = filter.gameObject.AddComponent<WorldAnchor>();
                //    Debug.Log("" + filter.mesh.name + ": Using new anchor...");
                //}
                //else
                //{
                //    Debug.Log("" + filter.mesh.name + ": Deleting existing anchor...");
                //    DestroyImmediate(attachingAnchor);
                //    Debug.Log("" + filter.mesh.name + ": Creating new anchor...");
                //    attachingAnchor = filter.gameObject.AddComponent<WorldAnchor>();
                //}
                //if (attachingAnchor.isLocated)
                //{
                //    if (!anchorStore.Save(meshName, attachingAnchor))
                //        Debug.Log("" + meshName + ": Anchor save failed...");
                //    else
                //        Debug.Log("" + meshName + ": Anchor SAVED...");
                //}
                //else
                //{
                //    attachingAnchor.OnTrackingChanged += AttachingAnchor_OnTrackingChanged;
                //}
            }

            // serialize and save meshes
            return MeshSaver.SaveAsObj(fileName, roomMeshes);
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
    }
}