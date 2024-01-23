 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GuiManager : MonoBehaviour
{
    public GameObject IDEntry;
    public GameObject Triangulate;
    public GameObject AlignmentCheck;
    public GameObject OjectIconHold;
    public GameObject InstructSwitch;
    public GameObject InstructOutline;
    public GameObject InstructTask;
    public GameObject InstructCueAssist;
    public GameObject InstructCueWarning;
    public GameObject InstructControl;
    public GameObject InstructFinal;
    public GameObject FinalEndScreen;
    public GameObject BreakScreen;

    public Text IDText;
    public VRPointerToCanvas vrPointerToCanvas;
    public GameObject pointer;
    public GameObject ControllerArrow;
    public TextMesh ControllerArrowText;
    public GameObject ControllerText;
    public GameObject environment;
    public StateManager stateManager;
    public CueManager cueManager;
    public ExperimentManager experimentManager;
    public DataLogger dataLogger;

    GameObject temp_block;


    public void InstructSwitchClick() {
      InstructSwitch.SetActive(false);
      InstructOutline.SetActive(true);
    }

    public void InstructOutlineClick() {
      InstructOutline.SetActive(false);
      InstructTask.SetActive(true);
    }

    public void InstructTaskClick() {
      InstructTask.SetActive(false);
      if (experimentManager.trialOdering.Item1 == "CUE") {
        InstructCueAssist.SetActive(true);
        stateManager.step = "CUEASSIST";
      }
      else {
        InstructControl.SetActive(true);
        stateManager.step = "CONTROL";
      }
    }

    public void ShowBreakScreen() {
      if (experimentManager.trialOdering.Item1 == "CUE") {
          cueManager.HideArrow();
          cueManager.Deactivate();
      }
      else {
          cueManager.HideCursor();
      }
      BreakScreen.SetActive(true);
      stateManager.step = "BREAK";
    }

    public void NewConditionScreen() {
      BreakScreen.SetActive(false);
      if (experimentManager.trialOdering.Item1 == "CUE") {
        stateManager.step = "CONTROL";
        InstructControl.SetActive(true);
      }
      else {
        stateManager.step = "CUEASSIST";
        InstructCueAssist.SetActive(true);
      }
    }

    public void InstructCueAssistClick() {
      InstructCueAssist.SetActive(false);
      InstructCueWarning.SetActive(true);
    }

    public void InstructCueWarningClick() {
      InstructCueWarning.SetActive(false);
      InstructFinal.SetActive(true);
    }

    public void InstructControlClick() {
      InstructControl.SetActive(false);
      InstructFinal.SetActive(true);
    }

    public void InstructFinalClick() {
      InstructFinal.SetActive(false);
      experimentManager.StartHoldScreen(-3);
      if ((experimentManager.trialIndex < 43 && experimentManager.trialOdering.Item1 == "CUE") || (experimentManager.trialIndex >= 43 && experimentManager.trialOdering.Item1 == "CONTROL")) { //CUE
          cueManager.Activate();
      }
    }

    public void ShowControllerArrow(bool choice) {
      ControllerArrow.SetActive(choice);
    }

    public void ShowControllerText(bool choice) {
      ControllerText.SetActive(choice);
    }

    public void SetControllerArrowText(string text) {
      ControllerArrowText.text = text;
    }

    public void SetControllerText(string text) {
      ControllerText.GetComponent<TextMesh>().text = text;
    }

    public void IDEntry_Del()
    {
      var str_arr = IDText.text.Split('_');
      if (str_arr.Length != 8 && str_arr.Length > 1)
      {
        IDText.text = str_arr[0].Substring(0, str_arr[0].Length - 1) + new string('_', str_arr.Length);
      }
      else if (str_arr.Length == 1)
      {
        IDText.text = str_arr[0].Substring(0, str_arr[0].Length - 1) + "_";
      }
    }

    public void IDEntry_Okay()
    {
      try {
        experimentManager.SetUserID(int.Parse(IDText.text.Split('_')[0]));
      }
      catch {
        Debug.Log("Invalid ID! Using 0 as new ID.");
        experimentManager.SetUserID(0);
      }
      IDEntry.SetActive(false);
      Triangulate.SetActive(true);
      vrPointerToCanvas.enabled = false;
      ControllerArrowText.text = "CONTINUE";
      pointer.SetActive(false);
      stateManager.step = "TRIANGULATE";
      dataLogger.SetUp(experimentManager.userID);
    }

    public void IDEntry_setText(string number)
    {
      var str_arr = IDText.text.Split('_');
      if (str_arr.Length != 1)
      {
        IDText.text = str_arr[0] + number + new string('_', str_arr.Length - 2);
      }
    }

    public void HideAlign()
    {
      Triangulate.SetActive(false);
    }

    public void ShowAlign()
    {
      AlignmentCheck.SetActive(true);
      Triangulate.SetActive(false);

      vrPointerToCanvas.enabled = true;
      pointer.SetActive(true);
      stateManager.step = "MENU";
    }

    public void Rescan()
    {
      foreach (Transform child in environment.transform)
      {
        GameObject.Destroy(child.gameObject);
      }
      AlignmentCheck.SetActive(false);
      stateManager.obj_index = 0;
      ShowControllerText(true);
      SetControllerText(experimentManager.Objects[stateManager.obj_index].name);
      stateManager.temp_block = Instantiate(experimentManager.Objects[stateManager.obj_index], stateManager.controllerTransform);
      stateManager.temp_block.transform.localPosition = new Vector3(0.000600000028f,-0.0934000015f,0.067900002f);
      stateManager.temp_block.transform.localRotation = new Quaternion(0.0f,-0.492423534f,0.870355725f,0.0f);
      vrPointerToCanvas.enabled = false;
      pointer.SetActive(false);
      stateManager.step = "RESCAN";
    }

    public void ShowMapper()
    {
      Triangulate.SetActive(false);
    }

    public void Align_DonePress()
    {
      vrPointerToCanvas.enabled = false;
      pointer.SetActive(false);
      AlignmentCheck.SetActive(false);
      InstructSwitch.SetActive(true);
      stateManager.step = "SWITCH";
      ShowControllerArrow(true);
      SetControllerArrowText("CONTINUE");
      foreach (Transform child in environment.transform) {
        foreach (Transform trans in child.Find("Offset").transform) {
          trans.gameObject.SetActive(false);
        }
      }
    }

    public void ShowHoldScreen()
    {
      stateManager.step = "HOLD";
    }

    public void ShowEndScreen()
    {
      FinalEndScreen.SetActive(true);
      stateManager.step = "HOLD";
    }

}
