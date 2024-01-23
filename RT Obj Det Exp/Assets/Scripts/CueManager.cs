using NetMQ;
using AsyncIO;
using UnityEngine;
using NetMQ.Sockets;
using Varjo.XR;
using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

public class CueManager : MonoBehaviour
{
    [SerializeField] float widthOffserCorrection;
    [SerializeField] float heightOffserCorrection;
  public Camera main_camera;
  public GameObject arrow;
  public GameObject cursor;
  public GameObject cue_plane;
  public ExperimentManager experimentManager;
  public DataLogger dataLogger;
  public static int classes = 40;
  public float distance;
  public float cursorDistance = 1.0f;
  public string cueingType = "ML"; // ML, WOZ

  bool running = false;
  bool cursor_running = false;
  bool arrow_active = false;
  Thread thread;

  int index = 0;
  float width = 0.0f;
  float height = 0.0f;
  float width_offset = 0.0f;
  float height_offset = 0.0f;
  float x_min_pixels = 0.0f;
  float x_max_pixels = 0.0f;
  float y_min_pixels = 0.0f;
  float y_max_pixels = 0.0f;

  Ray ray;
  Ray ray2;
  RaycastHit hit;
  RaycastHit hit2;
  Transform camera_transform = null;
  Vector3 up = new Vector3(0.0f, 0.0f, 0.0f);
  Vector3 center = new Vector3(0.0f, 0.0f, 0.0f);
  Vector3 left = new Vector3(0.0f, 0.0f, 0.0f);
  Vector3 top_left = new Vector3(0.0f, 0.0f, 0.0f);
  Vector3 top_right = new Vector3(0.0f, 0.0f, 0.0f);
  Vector3[,] positions = new Vector3[40, 4];
  float[] confidence = new float[40];
  float[] parsed_message = new float[classes * 6];
  public float sizeTY;


  void Awake()
  {
    VarjoMixedReality.StartRender();
  }
  GameObject[] test = new GameObject[40];
  void Start()
  {
    width = distance * 1.0433f - 0.0401f;
    height = distance * 0.6365f - 0.063f;
    width_offset = distance * 0.5305f - 0.0213f + widthOffserCorrection;
    height_offset = ((distance * 0.1881f) - 0.0188f) + heightOffserCorrection;
  }

  public void Activate()
  {
    running = true;
    thread = new Thread(ReceiveCoordinates);
    thread.Start();
  }

  public void Deactivate()
  {
    running = false;
    thread.Join();
  }

  public void ShowCursor()
  {
    cursor_running = true;
    cursor.SetActive(true);
  }

  public void HideCursor()
  {
    cursor_running = false;
    cursor.SetActive(false);
  }

  public void ShowArrow()
  {
    arrow_active = true;
    arrow.SetActive(true);
  }

  public void HideArrow()
  {
    arrow_active = false;
    arrow.SetActive(false);
  }

  void ReceiveCoordinates()
  {
    byte[] message = null;
    ForceDotNet.Force();
    using (SubscriberSocket client = new SubscriberSocket())
    {
      client.Connect("tcp://localhost:5555");
      client.Subscribe("");
      while (running)
      {
        if (client.TryReceiveFrameBytes(out message)) Buffer.BlockCopy(message, 0, parsed_message, 0, message.Length);
      }
    }
    NetMQConfig.Cleanup();
  }

  void OnDestroy()
  {
    running = false;
    thread.Join();
  }
  GameObject go;
  void Update()
  {
    if (running) {
      camera_transform = main_camera.transform;
      center = camera_transform.position + (camera_transform.forward * distance);
      left = Vector3.Cross(camera_transform.forward, camera_transform.up).normalized;

      if (experimentManager.cueAidType == "ML") {
        for (int i = 0; i < classes; i++) {
          if (parsed_message[i * 6] == 1.0) {
            index = (i * 6) + 1;
            x_min_pixels = parsed_message[index] * width;
            x_max_pixels = parsed_message[index + 1] * width;
            y_min_pixels = parsed_message[index + 2] * height;
            y_max_pixels = parsed_message[index + 3] * height;

            top_left = center + (camera_transform.up.normalized * height_offset) + (left * width_offset);
            top_right = top_left - (left * x_max_pixels);
            top_left = top_left - (left * x_min_pixels);

            positions[i, 0] = top_left - (camera_transform.up.normalized * y_min_pixels); // top_left
            positions[i, 1] = top_left - (camera_transform.up.normalized * y_max_pixels); // bottom_left
            positions[i, 2] = top_right - (camera_transform.up.normalized * y_max_pixels); // bottom_right
            positions[i, 3] = top_right - (camera_transform.up.normalized * y_min_pixels); // top_right
            confidence[i] = parsed_message[index + 4];
          }
        }
      }


      if (arrow_active) {
        if (experimentManager.ObjectNumber < 0) {
          center = experimentManager.CurrPracticeObjects[experimentManager.ObjectNumber + 3].GetComponent<HighlightManager>().HighlightSphere.transform.position;
        }
        else {
          if (experimentManager.cueAidType == "WOZ") {
            center = experimentManager.CurrObjects[experimentManager.ObjectNumber - 1].GetComponent<HighlightManager>().HighlightSphere.transform.position;
          }
          else {
            index = experimentManager.ObjectNumber - 1;
            center = (positions[index, 0] + positions[index, 1] + positions[index, 2] + positions[index, 3]) / 4.0f;
            dataLogger.LogCuePos(center, confidence[index]);
          }
        }
        center = Vector3.ProjectOnPlane(center, cue_plane.transform.forward) + Vector3.Dot(cue_plane.transform.position, cue_plane.transform.forward) * cue_plane.transform.forward;
        if (Physics.Raycast(camera_transform.position, camera_transform.forward, out hit) && hit.transform.parent != null) {
          if (experimentManager.selected_object != null && experimentManager.selected_object != hit.transform.parent.gameObject){
            experimentManager.selected_HM.Disable();
          }
          experimentManager.selected_object = hit.transform.parent.gameObject;
          experimentManager.selected_HM = experimentManager.selected_object.GetComponent<HighlightManager>();
          experimentManager.selected_HM_Outline = experimentManager.selected_HM.HighlightSphere.GetComponent<Outline>();
          if (experimentManager.ObjectNumber > 0) {
            dataLogger.CUE_DIST = (center - (Vector3.ProjectOnPlane((experimentManager.CurrObjects[experimentManager.ObjectNumber - 1].GetComponent<HighlightManager>().HighlightSphere.transform.position), cue_plane.transform.forward) + Vector3.Dot(cue_plane.transform.position, cue_plane.transform.forward) * cue_plane.transform.forward)).magnitude.ToString();
          }
          if ((arrow.transform.position - center).magnitude * 10.0f < sizeTY) {
            experimentManager.selected_HM.Enable(true);
            arrow.SetActive(false);
          }
          else {
            experimentManager.selected_HM.Enable(false);
            experimentManager.selected_HM_Outline.OutlineColor = Color.white;
            ShowArrow();
          }
        }
        else if (experimentManager.selected_object != null) {
          experimentManager.selected_HM.Disable();
          experimentManager.selected_object = null;
          experimentManager.selected_HM = null;
          experimentManager.selected_HM_Outline = null;
          ShowArrow();
        }
        else ShowArrow();
        arrow.transform.LookAt(center, cue_plane.transform.right);
      }
    }
    else if (cursor_running) {
      camera_transform = main_camera.transform;
      if (Physics.Raycast(camera_transform.position, camera_transform.forward, out hit) && hit.transform.parent != null) {
        if (experimentManager.selected_object != null && experimentManager.selected_object != hit.transform.parent.gameObject){
          experimentManager.selected_HM.Disable();
        }
        experimentManager.selected_object = hit.transform.parent.gameObject;
        experimentManager.selected_HM = experimentManager.selected_object.GetComponent<HighlightManager>();
        experimentManager.selected_HM.Enable(false);
        cursor.transform.position = hit.point;
      }
      else if (experimentManager.selected_object != null) {
        experimentManager.selected_HM.Disable();
        experimentManager.selected_object = null;
        experimentManager.selected_HM = null;
        cursor.transform.position = camera_transform.position + (camera_transform.forward * cursorDistance);
      }
      else cursor.transform.position = camera_transform.position + (camera_transform.forward * cursorDistance);;
    }
  }
}















// if (experimentManager.selected_object.name == (ObjectNumber > 0 ? ObjectNames[ObjectNumber - 1] : PracticeObjectNames[ObjectNumber + 3])) {
