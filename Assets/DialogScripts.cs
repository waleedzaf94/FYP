//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX.Dialogs;
using HUX.Interaction;
using HUX.Receivers;
using System.Collections;
using UnityEngine;
using HUX.Focus;

public class DialogScripts : InteractionReceiver
{
    public GameObject DialogPrefab;
    public GameObject[] LaunchDialogButtons;
    public TextMesh Result;

    [Header("Help Options")]
    public string HelpTitle = "Help";
    [TextArea]
    public string HelpMessage = "Help Text Goes Here";
    SimpleDialog.ButtonTypeEnum HelpButton = SimpleDialog.ButtonTypeEnum.Close;

    [Header("Dialog 2 options")]
    public string Dialog2Title = "Yes No Dialog";
    [TextArea]
    public string Dialog2Message = "This is a message for dialog 2. Longer messages will be wrapped automatically. However you still need to be aware of overflow.";
    SimpleDialog.ButtonTypeEnum Dialog2Button1 = SimpleDialog.ButtonTypeEnum.Yes;
    SimpleDialog.ButtonTypeEnum Dialog2Button2 = SimpleDialog.ButtonTypeEnum.No;

    protected bool launchedDialog;

    protected override void OnTapped(GameObject obj, InteractionManager.InteractionEventArgs eventArgs)
    {
        base.OnTapped(obj, eventArgs);
        if (launchedDialog)
            return;

        SimpleDialog.ButtonTypeEnum buttons = SimpleDialog.ButtonTypeEnum.Close;
        string title = string.Empty;
        string message = string.Empty;

        switch (obj.name)
        {
            default:
                launchedDialog = true;
                StartCoroutine(LaunchDialogOverTime(HelpButton, HelpTitle, HelpMessage));
                break;

            case "RecordButton":
                setResultText("Recording...");
                closeMenu("InitalMenu");
                openMenu("OnRecordMenu");
                break;
            
            case "LibraryButton":
                title = Dialog2Title;
                message = Dialog2Message;
                buttons = Dialog2Button1 | Dialog2Button2;
                launchedDialog = true;
                StartCoroutine(LaunchDialogOverTime(buttons, title, message));
                break;

            case "StopButton":
                setResultText("Recording Stopped");
                closeMenu("OnRecordMenu");
                openMenu("InitalMenu");
                break;

            case "CancelButton":
                setResultText("Recording Cancelled");
                closeMenu("OnRecordMenu");
                openMenu("InitalMenu");
                break;
        }       
    }

    protected IEnumerator LaunchDialogOverTime(SimpleDialog.ButtonTypeEnum buttons, string title, string message)
    {
        // Disable all our buttons
        foreach (GameObject buttonGo in Interactibles)
        {
            buttonGo.SetActive(false);
        }
        Result.gameObject.SetActive(false);

        SimpleDialog dialog = SimpleDialog.Open(DialogPrefab, buttons, title, message);
        dialog.OnClosed += OnClosed;

        // Wait for dialog to close
        while (dialog.State != SimpleDialog.StateEnum.Closed)
        {
            yield return null;
        }

        // Enable all our buttons
        foreach (GameObject buttonGo in Interactibles)
        {
            buttonGo.SetActive(true);
        }
        Result.gameObject.SetActive(true);
        launchedDialog = false;
        yield break;
    }

    protected void closeMenu(string menuName)
    {
        GameObject menu = GameObject.Find(menuName);
        if(menu!= null)
            menu.SetActive(false);
        else
            setResultText("Cannot find menu" + menuName);
    }
    
    protected void openMenu(string menuName)
    {
        GameObject menu = GameObject.Find(menuName);
        if (menu != null)
            menu.SetActive(true);
        else
            setResultText("Cannot find menu" + menuName);
    }

    protected void OnClosed(SimpleDialogResult result)
    {
        setResultText("Dialog result: " + result.Result.ToString());
    }

    void setResultText(string str)
    {
        Result.text = str;
    }

}
