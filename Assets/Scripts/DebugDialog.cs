using HoloToolkit.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDialog : Singleton<DebugDialog> {

    public TextMesh DebugDisplay;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        DebugDisplay.text = String.IsNullOrEmpty(PrimaryText) ? String.Empty : PrimaryText ;
    }


    public string PrimaryText
    {
        get;
        set;
    }

    internal void ClearText()
    {
        PrimaryText = String.Empty;
    }
}
