using HoloToolkit.Unity;
using System;

[Serializable()]
public class MeshInfo
{
    public MeshInfo(string rowKey, string partitionKey = "obj") {
        filename = rowKey;
    }

    public string filename;
    public SpatialUnderstandingDll.Imports.PlayspaceStats playspaceStats;
    public string inputContainer;
    public string outputContainer; 
    internal string localpath;
}