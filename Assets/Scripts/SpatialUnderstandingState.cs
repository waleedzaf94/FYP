﻿using System;
using UnityEngine;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;

public class SpatialUnderstandingState : Singleton<SpatialUnderstandingState>, IInputClickHandler, ISourceStateHandler
{
    public float MinAreaForStats = 5.0f;
    public float MinAreaForComplete = 50.0f;
    public float MinHorizAreaForComplete = 10.0f;
    public float MinWallAreaForComplete = 35.0f;

    private int counter = 0;

    private uint trackedHandsCount = 0;

    public TextMesh DebugDisplay;
    public TextMesh DebugSubDisplay;
    public SpatialUnderstandingCustomMesh SpatialUnderstandingMesh;
    public Material OccludedMaterial;
    public Material MeshMaterial;



    private bool _triggered = false;
    public bool HideText = false;

    private bool ready = false;

    private string _spaceQueryDescription;
    private bool _timeToHideMesh;

    // private RoomSaver roomSaver;

    public string SpaceQueryDescription
    {
        get
        {
            return _spaceQueryDescription;
        }
        set
        {
            _spaceQueryDescription = value;
        }
    }

    public void HideMesh()
    {
        _timeToHideMesh = true;
        HideText = true;
        //SpatialUnderstanding.Instance.UnderstandingCustomMesh.MeshMaterial = OccludedMaterial;
        SpatialUnderstandingMesh.MeshMaterial = OccludedMaterial;
        Debug.Log("Calling Hide");
    }

    public void ShowMesh()
    {
        _timeToHideMesh = false;
        SpatialUnderstandingMesh.MeshMaterial = MeshMaterial;
        Debug.Log("Calling Show");
    }


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

            // Check our preset requirements
            if ((stats.TotalSurfaceArea > MinAreaForComplete) ||
                (stats.HorizSurfaceArea > MinHorizAreaForComplete) ||
                (stats.WallSurfaceArea > MinWallAreaForComplete))
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

            // Display the space and object query results (has priority)
            if (!string.IsNullOrEmpty(SpaceQueryDescription))
            {
                return SpaceQueryDescription;
            }

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
                            return "When ready, air tap to finalize your scan";
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
                if (trackedHandsCount > 0)
                {
                    return ready ? Color.green : Color.red;
                }
                return ready ? Color.yellow : Color.white;
            }

            // If we're looking at the menu, fade it out
            float alpha = 1.0f;

            // Special case processing & 
            return (!string.IsNullOrEmpty(SpaceQueryDescription)) ?
                (PrimaryText.Contains("processing") ? new Color(1.0f, 0.0f, 0.0f, 1.0f) : new Color(1.0f, 0.7f, 0.1f, alpha)) :
                new Color(1.0f, 1.0f, 1.0f, alpha);
        }
    }

    public string DetailsText
    {
        get
        {
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
                    string subDisplayText = string.Format("totalArea={0:0.0}, horiz={1:0.0}, wall={2:0.0}", stats.TotalSurfaceArea, stats.HorizSurfaceArea, stats.WallSurfaceArea);
                    subDisplayText += string.Format("\nnumFloorCells={0}, numCeilingCells={1}, numPlatformCells={2}", stats.NumFloor, stats.NumCeiling, stats.NumPlatform);
                    subDisplayText += string.Format("\npaintMode={0}, seenCells={1}, notSeen={2}", stats.CellCount_IsPaintMode, stats.CellCount_IsSeenQualtiy_Seen + stats.CellCount_IsSeenQualtiy_Good, stats.CellCount_IsSeenQualtiy_None);
                    return subDisplayText;
                }
                return "";
            }
            return "";
        }
    }

    private void Update_DebugDisplay()
    {
        // Basic checks
        if (DebugDisplay == null)
        {
            return;
        }
        DebugDisplay.text = PrimaryText;
        DebugDisplay.color = PrimaryColor;
        DebugSubDisplay.text = DetailsText;
    }

    private void Start()
    {
        InputManager.Instance.PushFallbackInputHandler(gameObject);
    }

    private void Update()
    {
        // Updates
        Update_DebugDisplay();

        if (!_triggered && SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done)
        {
            _triggered = true;
            ready = false;
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
        if (_timeToHideMesh)
        {
            HideMesh();
            _timeToHideMesh = false;
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