using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;


public class SaveMeshButton : MonoBehaviour, IInputClickHandler
{

    GameObject saveButton;
	// Use this for initialization
	void Start () {
	}

    public void OnInputClicked(InputClickedEventData eventData)
    {
        Debug.Log("Button Clicked");
    }

	// Update is called once per frame
	void Update () {
		
	}
}
