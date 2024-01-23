using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
  public Transform center;
  public GameObject environment;
  public Transform controllerTransform;

  public VRPointerToCanvas vrPointerToCanvas;
  public GuiManager guiManager;
  public CueManager cueManager;
  public ExperimentManager experimentManager;
  public HelperFunctions helperFunctions;

  public GameObject temp_block;
  public string step = "MENU";
  public int obj_index = 0;

  bool triggerValue;
  bool trigger;
  bool fakeTrigger = false;
  bool isPressed = false;
  Vector3 a;
  Vector3 b;
  Vector3 c;
  Vector3 pos_a;
  Vector3 pos_b;
  Vector3 pos_c;
  Quaternion rot_a;
  Quaternion rot_b;
  Quaternion rot_c;
  UnityEngine.XR.InputDevice device;

  void Start()
  {
    var devices = new List<UnityEngine.XR.InputDevice>();
    UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, devices);

    if(devices.Count > 0){
        device = devices[0];
        Debug.Log(string.Format("Device name '{0}' with role '{1}'", device.name, device.characteristics.ToString()));
    }
    else{
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, devices);
        if(devices.Count > 0){
            device = devices[0];
            Debug.Log(string.Format("Device name '{0}' with role '{1}'", device.name, device.characteristics.ToString()));
        }
        else {
          Debug.Log("No Controller Found!");
        }
    }
  }

  public void simulatePress()
  {
    fakeTrigger = true;
    isPressed = false;
  }

  void ScanInBlock(GameObject Object)
  {
    experimentManager.CurrObjects[obj_index] = Instantiate(Object, environment.transform);
    var local_pos = environment.transform.InverseTransformPoint(temp_block.transform.position);
    experimentManager.CurrObjects[obj_index].transform.localPosition = local_pos;
    experimentManager.CurrObjects[obj_index].transform.rotation = temp_block.transform.rotation;
    experimentManager.CurrObjects[obj_index].name = Object.name;

    var local_rot = experimentManager.CurrObjects[obj_index].transform.localRotation;
    var loc_pos_str = "("+local_pos.x.ToString()+", "+local_pos.y.ToString()+", "+local_pos.z.ToString()+")";
    var loc_rot_str = "("+local_rot[0].ToString()+", "+local_rot[1].ToString()+", "+local_rot[2].ToString()+", "+local_rot[3].ToString()+")";
    PlayerPrefs.SetString("pos_"+Object.name, loc_pos_str);
    PlayerPrefs.SetString("rot_"+Object.name, loc_rot_str);
    PlayerPrefs.Save();
    Destroy(temp_block);

    if (obj_index < experimentManager.Objects.Length - 1) {
      obj_index++;
      guiManager.SetControllerText(experimentManager.Objects[obj_index].name);
      temp_block = Instantiate(experimentManager.Objects[obj_index], controllerTransform);
      temp_block.transform.localPosition = new Vector3(0.000600000028f,-0.0934000015f,0.067900002f);
      temp_block.transform.localRotation = new Quaternion(0.0f,-0.492423534f,0.870355725f,0.0f);
    }
    else {
      guiManager.ShowControllerText(false);
      step = "MENU";
      PlayerPrefs.SetInt("saved_map", 1);
      PlayerPrefs.Save();
      experimentManager.LoadBlocks();
      guiManager.ShowAlign();
    }
  }

  void UploadPracticeBlocks()
  {
    Vector3 center_coords = (a + b + c) / 3.0f;
    environment.transform.position = center_coords;
    Vector3[] positions = new Vector3[]{pos_a, pos_b, pos_c};
    Quaternion[] rotations = new Quaternion[]{rot_a, rot_b, rot_c};
    for (int i = 0; i < 3; i++){
      var local_pos = environment.transform.InverseTransformPoint(positions[i]);
      var loc_pos = "("+local_pos.x.ToString()+", "+local_pos.y.ToString()+", "+local_pos.z.ToString()+")";
      var loc_rot = "("+rotations[i][0].ToString()+", "+rotations[i][1].ToString()+", "+rotations[i][2].ToString()+", "+rotations[i][3].ToString()+")";
      PlayerPrefs.SetString("pos_"+experimentManager.PracticeObjectNames[i], loc_pos);
      PlayerPrefs.SetString("rot_"+experimentManager.PracticeObjectNames[i], loc_rot);
      environment.transform.GetChild(i).position -= center_coords;
    }
    PlayerPrefs.Save();

    if (PlayerPrefs.GetInt("saved_map", 0) == 0) {
      guiManager.SetControllerText(experimentManager.Objects[obj_index].name);
      temp_block = Instantiate(experimentManager.Objects[obj_index], controllerTransform);
      temp_block.transform.localPosition = new Vector3(0.000600000028f,-0.0934000015f,0.067900002f);
      temp_block.transform.localRotation = new Quaternion(0.0f,-0.492423534f,0.870355725f,0.0f);
      step = "OBJ";
    }
    else {
      guiManager.ShowControllerText(false);
      experimentManager.LoadBlocks();
      guiManager.ShowAlign();
      step = "MENU";
    }
  }

  void ScanInPracticeBlock(int index) {
    switch (index) {
      case 0:
        pos_a = temp_block.transform.position;
        rot_a = temp_block.transform.rotation;
        break;
      case 1:
        pos_b = temp_block.transform.position;
        rot_b = temp_block.transform.rotation;
        break;
      case 2:
        pos_c = temp_block.transform.position;
        rot_c = temp_block.transform.rotation;
        break;
    }
    var permBlock = Instantiate(experimentManager.PracticeObjects[index], environment.transform);
    permBlock.transform.position = temp_block.transform.position;
    permBlock.transform.rotation = temp_block.transform.rotation;
    permBlock.name = experimentManager.PracticeObjectNames[index];
    GameObject.Destroy(temp_block);
    if (index < 2) {
      temp_block = Instantiate(experimentManager.PracticeObjects[index + 1], controllerTransform);
      temp_block.transform.localPosition = new Vector3(0.000600000028f,-0.0934000015f,0.067900002f);
      temp_block.transform.localRotation = new Quaternion(0.0f,-0.492423534f,0.870355725f,0.0f);
      guiManager.SetControllerText(experimentManager.PracticeObjectNames[index + 1]);
      step = experimentManager.PracticeObjectNames[index + 1];
    }
  }

  void Update()
  {
    trigger = device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerValue);
    if ((trigger && triggerValue) || fakeTrigger) {
      if (!isPressed) {
        isPressed = true;
        switch(step) {
          case "MENU":
            vrPointerToCanvas.ControllerClick();
            break;
          case "TRIANGULATE":
            temp_block = Instantiate(experimentManager.PracticeObjects[0], controllerTransform);
            temp_block.transform.localPosition = new Vector3(0.000600000028f,-0.0934000015f,0.067900002f);
            temp_block.transform.localRotation = new Quaternion(0.0f,-0.492423534f,0.870355725f,0.0f);
            guiManager.ShowControllerArrow(false);
            guiManager.ShowControllerText(true);
            guiManager.SetControllerText("A");
            guiManager.HideAlign();
            step = "A";
            break;
          case "A":
            a = center.transform.position;
            ScanInPracticeBlock(0);
            break;
          case "B":
            b = center.transform.position;
            ScanInPracticeBlock(1);
            break;
          case "C":
            c = center.transform.position;
            ScanInPracticeBlock(2);
            UploadPracticeBlocks();
            break;
          case "OBJ":
            ScanInBlock(experimentManager.Objects[obj_index]);
            break;
          case "RESCAN":
            ScanInBlock(experimentManager.Objects[obj_index]);
            break;
          case "SWITCH":
            guiManager.InstructSwitchClick();
            step = "OUTLINE";
            break;
          case "OUTLINE":
            guiManager.InstructOutlineClick();
            step = "TASK";
            break;
          case "TASK":
            guiManager.InstructTaskClick();
            break;
          case "CUEASSIST":
            guiManager.InstructCueAssistClick();
            step = "CUEWARNING";
            break;
          case "CUEWARNING":
            guiManager.InstructCueWarningClick();
            step = "FINAL";
            break;
          case "CONTROL":
            guiManager.InstructControlClick();
            step = "FINAL";
            break;
          case "FINAL":
            guiManager.InstructFinalClick();
            step = "HOLD";
            break;
          case "RELEASE":
            experimentManager.StopHoldScreen();
            break;
          case "SEARCH":
            experimentManager.ControllerSelect();
            break;
          case "BREAK":
            guiManager.NewConditionScreen();
            break;
        }
      }
    }
    else if (isPressed) {
        isPressed = false;
    }
    fakeTrigger = false;
  }
}
