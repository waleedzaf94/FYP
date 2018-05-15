using System;
using UnityEngine;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;

public class SpatialUnderstandingState : Singleton<SpatialUnderstandingState>, IInputClickHandler, ISourceStateHandler
{
    public float MinAreaForStats = 5.0f;
    public float MinAreaForComplete = 24.0f;
    public float MinHorizAreaForComplete = 4.0f;
    public float MinWallAreaForComplete = 20.0f;
    public float MinCellQuality = 0.5f;

    private uint trackedHandsCount = 0;

    public TextMesh DebugDisplay;
    public TextMesh DebugSubDisplay;

    private bool _triggered = false;
    public bool HideText = false;

    private bool ready = false;

    public bool DoesScanMeetMinBarForCompletion
    {
        get
        {
            // Only allow this when we are actually scanning
            if ((SpatialUnderstanding.Instance.ScanState != SpatialUnderstanding.ScanStates.Scanning) ||
                (!SpatialUnderstanding.Instance.AllowSpatialUnderstanding))
            {
                return false;
            }

            // Query the current playspace stats
            IntPtr statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
            if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) == 0)
            {
                return false;
            }
            SpatialUnderstandingDll.Imports.PlayspaceStats stats = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats();
            float cellQuality = 0f;
            if (stats.CellCount_IsSeenQualtiy_Seen > 0)
                cellQuality = stats.CellCount_IsSeenQualtiy_Good / stats.CellCount_IsSeenQualtiy_Seen;
            
            // Check our preset requirements
            if ((stats.TotalSurfaceArea > MinAreaForComplete) && (cellQuality > MinCellQuality))
                //||
                //(
                //    (stats.HorizSurfaceArea > MinHorizAreaForComplete) &&
                //    (stats.WallSurfaceArea > MinWallAreaForComplete))
                //)
            {
                return true;
            }
            return false;
        }
    }

    public string PrimaryText
    {
        get
        {
            if (HideText)
                return string.Empty;
            
            // Scan state
            if (SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
            {
                
                switch (SpatialUnderstanding.Instance.ScanState)
                {
                    case SpatialUnderstanding.ScanStates.Scanning:
                        // Get the scan stats
                        IntPtr statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
                        if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) == 0)
                        {
                            return "playspace stats query failed";
                        }

                        // The stats tell us if we could potentially finish
                        if (DoesScanMeetMinBarForCompletion)
                        {
                            return "Tap to finalize";
                        }
                        return "Walk around and scan your room";
                    case SpatialUnderstanding.ScanStates.Finishing:
                        return "Finalizing scan (please wait)";
                    case SpatialUnderstanding.ScanStates.Done:
                        return "Scan complete";
                    default:
                        return "";
                }
            }
            return string.Empty;
        }
    }
  
    public Color PrimaryColor
    {
        get
        {
            ready = DoesScanMeetMinBarForCompletion;
            if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning)
            {
                return ready ? Color.green : Color.red;
            }
            // Special case processing & 
            return PrimaryText.Contains("Finalizing") ? Color.yellow : Color.white;
        }
    }

    public string DetailsText
    {
        get
        {
            if (HideText)
                return string.Empty;

            if (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.None)
            {
                return "";
            }

            // Scanning stats get second priority
            if ((SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning) &&
                (SpatialUnderstanding.Instance.AllowSpatialUnderstanding))
            {
                IntPtr statsPtr = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
                if (SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPtr) == 0)
                {
                    return "Recording stats query failed";
                }
                SpatialUnderstandingDll.Imports.PlayspaceStats stats = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats();

                // Start showing the stats when they are no longer zero
                if (stats.TotalSurfaceArea > MinAreaForStats)
                {
                    SpatialMappingManager.Instance.DrawVisualMeshes = false;
                    string subDisplayText = string.Format("Total Area = {0:0.00}, Floor Area={1:0.00}, Wall Area={2:0.00}", stats.TotalSurfaceArea, stats.HorizSurfaceArea, stats.WallSurfaceArea);
                    return subDisplayText;
                }
                return "";
            }
            return "";
        }
    }

    public bool MeshSaving { get; internal set; } = false;

    private void Update_DebugDisplay()
    {
        // Basic checks
        if (DebugDisplay == null)
        {
            return;
        }
        if (!MeshSaving)
        {
            DebugDisplay.text = PrimaryText;
            DebugDisplay.color = PrimaryColor;
            DebugSubDisplay.text = DetailsText;

        }
    }

    private void Start()
    {
        InputManager.Instance.PushFallbackInputHandler(gameObject);
    }

    private void Update()
    {
        // Updates
        SpatialUnderstanding g = null;
        if (!SpatialUnderstanding.IsInitialized)
        {
            g = SpatialUnderstanding.Instance;
        }

        if (SpatialUnderstanding.Instance != null)
        {
            Update_DebugDisplay();
            if (!_triggered && SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done)
            {
                _triggered = true;
                ready = false;
            }
        }
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (ready &&
            (SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Scanning) &&
            !SpatialUnderstanding.Instance.ScanStatsReportStillWorking)
        {
            _triggered = false;
        }
    }

    void ISourceStateHandler.OnSourceDetected(SourceStateEventData eventData)
    {
        trackedHandsCount++;
    }

    void ISourceStateHandler.OnSourceLost(SourceStateEventData eventData)
    {
        trackedHandsCount--;
    }
}