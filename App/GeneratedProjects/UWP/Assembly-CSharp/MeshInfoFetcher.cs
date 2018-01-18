using System;
using System.Threading.Tasks;
using HoloToolkit.Unity;
using Windows.Perception.Spatial;
using static HoloToolkit.Unity.SpatialUnderstandingDll;
using System.Runtime.InteropServices;
using Windows.Storage;
using UnityEngine;
using Windows.Perception.Spatial.Surfaces;
using Windows.Foundation;
using System.Collections.Concurrent;

public class MeshInfoFetcher
{
    public ConcurrentDictionary<int, MeshInfo> MeshList { get; private set; }

    public struct MeshInfo
    {
        public MeshData MeshData;
        public string VertexString;
        public string VNormalString;
        public string IndexString;
    }

    private SpatialLocator locator;
    private SpatialCoordinateSystem baseCoordinateSystem;
    private SpatialBoundingVolume boundingVolume;
    private SpatialSurfaceObserver SurfaceObserver { get; }
    public StorageFolder StorageFolderName { get; internal set; }

    // TODO: 
    // Need to set thresholds for IsQualityMesh 

    public MeshInfoFetcher()
    {
        SurfaceObserver = new SpatialSurfaceObserver();
        MeshList = new ConcurrentDictionary<int, MeshInfo>();
    }

    internal void CreateBounding()
    {
        this.locator = SpatialLocator.GetDefault();
        var frameOfReference = this.locator.CreateStationaryFrameOfReferenceAtCurrentLocation();
        this.baseCoordinateSystem = frameOfReference.CoordinateSystem;
        this.boundingVolume = SpatialBoundingVolume.FromBox(
              this.baseCoordinateSystem,
              new SpatialBoundingBox()
              {
                  Center = new System.Numerics.Vector3(0, 0, 0),
                  Extents = new System.Numerics.Vector3(4, 4, 4)
              }
        );
    }

    internal bool IsQualityMesh(Imports.PlayspaceStats playspaceStats)
    {
        return true;
    }

    internal void FetchMeshInfoFromUnderstanding()
    {
        SpatialUnderstanding.Instance.UnderstandingSourceMesh.GetInputMeshList(out int meshCount, out IntPtr meshList);

        Imports.MeshData[] result = new Imports.MeshData[meshCount];
        int sizePointerToABC = Marshal.SizeOf(new Imports.MeshData());
        for (int i = 0; i < meshCount; i++)
        {
            result[i] = Marshal.PtrToStructure<Imports.MeshData>(new IntPtr(meshList.ToInt32() + (i * sizePointerToABC)));
        }

        foreach (Imports.MeshData m in result)
        {
            ProcessMesh(m);
        }
        Debug.Log("Mesh Count: " + meshCount);
    }

    internal void FetchMeshInfoFromPerception()
    {
        SurfaceObserver.SetBoundingVolume(this.boundingVolume);
        var surfaceInfoList = SurfaceObserver.GetObservedSurfaces();
        Debug.Log("KeyCount" + surfaceInfoList.Count);
        foreach (var test in surfaceInfoList)
        {
            Debug.Log("GUID:" + test.Key.ToString());
            var meshAsync = test.Value.TryComputeLatestMeshAsync(500.0);
            meshAsync.Completed = new AsyncOperationCompletedHandler<SpatialSurfaceMesh>(ProcessMesh);
        }
    }

    internal async Task PublishMeshInfoToFileAsync(string fileName)
    {
        StorageFile sampleFile = await StorageFolderName.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
        StorageFile getFile = await StorageFolderName.GetFileAsync(fileName);
        await FileIO.WriteTextAsync(getFile, "ID:" + m.meshID);
        foreach (var m in MeshList)
        {
            MeshInfo val = m.Value;
            string[] meshStrings = { $"VERTEX: \n {val.VertexString}", $"NORMALS: \n {val.VNormalString}", $"INDEX: \n {val.IndexString}" };
            await FileIO.AppendLinesAsync(getFile, meshStrings);
        }
    }

    // Should make this async
    private void ProcessMesh(Imports.MeshData m)
    {
        Vector3[] vertices = new Vector3[m.vertCount];
        Vector3[] vertNormals = new Vector3[m.vertCount];
        int sizePointerToVertices = Marshal.SizeOf(new Vector3());
        string indexString = "";
        string vertexString = "";
        string vNormalString = "";
        //string fileName = m.meshID + "_sample.txt";
        for (int i = 0; i < m.vertCount; i++)
        {
            vertices[i] = Marshal.PtrToStructure<Vector3>(new IntPtr(m.verts.ToInt32() + (i * sizePointerToVertices)));
            vertNormals[i] = Marshal.PtrToStructure<Vector3>(new IntPtr(m.normals.ToInt32() + (i * sizePointerToVertices)));
            //await FileIO.AppendTextAsync(getFile, "Vertix: " + vertices[i].ToString() + "  Normal: " + vertNormals[i].ToString());
            vertexString += $"{vertices[i].x} {vertices[i].y} {vertices[i].z} \n";
            vNormalString += $"{vertNormals[i].x} {vertNormals[i].y} {vertNormals[i].z} \n";
        }
        Int32[] indices = new Int32[m.indexCount];
        int sizePointerToIndices = Marshal.SizeOf<Int32>();
        for (int j = 0; j < m.indexCount; j++)
        {
            int a  = Marshal.PtrToStructure<Int32>(new IntPtr(m.indices.ToInt32() + (j * sizePointerToIndices)));
            int b = Marshal.PtrToStructure<Int32>(new IntPtr(m.indices.ToInt32() + ((j+1) * sizePointerToIndices)));
            int c = Marshal.PtrToStructure<Int32>(new IntPtr(m.indices.ToInt32() + ((j+2) * sizePointerToIndices)));
            //await FileIO.AppendTextAsync(getFile, "Index: " + indices[j]);
            indexString += $"{a} {b} {c} \n";
            indices[j] = a;
            indices[++j] = b;
            indices[++j] = c;
        }

        MeshData mData = new MeshData()
        {
            Verts = vertices,
            Normals = vertNormals,
            Indices = indices,
            MeshID = m.meshID,
            LastUpdateID = m.lastUpdateID,
            Transform = m.transform
        };

        MeshInfo meshInfo = new MeshInfo()
        {
            MeshData = mData,
            IndexString = indexString,
            VertexString = vertexString,
            VNormalString = vNormalString
        };

        MeshList.AddOrUpdate(m.meshID, meshInfo, (key, oldValue) => meshInfo);
    }

    private void ProcessMesh(IAsyncOperation<SpatialSurfaceMesh> asyncInfo, AsyncStatus asyncStatus)
    {
        var mesh = asyncInfo.GetResults();
        Debug.Log("Mesh Coordinate System" + mesh.CoordinateSystem.ToString());
    }
}