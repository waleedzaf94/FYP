using HoloToolkit.Unity;
using System;

[Serializable()]
public class MeshInfo
{
    public MeshInfo()
    {

    }

    public MeshInfo(string rowKey, string partitionKey = "obj") {
        filename = rowKey;
    }

    public string filename;
    public SpatialUnderstandingDll.Imports.PlayspaceStats playspaceStats;
    public string inputContainer;
    public string outputContainer; 
    internal string localpath;
    internal string metadata;

    public float TotalSurfaceArea { get; internal set; }
    public float HorizSurfaceArea { get; internal set; }
    public float UpSurfaceArea { get; internal set; }
    public float VirtualWallSurfaceArea { get; internal set; }
    public float VirtualCeilingSurfaceArea { get; internal set; }
    public float WallSurfaceArea { get; internal set; }
    public float DownSurfaceArea { get; internal set; }
}