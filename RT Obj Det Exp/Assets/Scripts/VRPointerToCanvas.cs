using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// This is for showing the vrpointer and selecting the worldspace canvas with controller

public class VRPointerToCanvas : BaseInputModule
{
    public Camera mCamera;

    GameObject curr_obj = null;
    PointerEventData data;

    protected override void Awake()
    {
      base.Awake();
      data = new PointerEventData(eventSystem);
    }

    public override void Process()
    {
      data.Reset();
      data.position = new Vector2(mCamera.pixelWidth / 2, mCamera.pixelHeight / 2);

      eventSystem.RaycastAll(data, m_RaycastResultCache);
      data.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
      curr_obj = data.pointerCurrentRaycast.gameObject;

      m_RaycastResultCache.Clear();
      HandlePointerExitAndEnter(data, curr_obj);
    }

    public PointerEventData GetData()
    {
      return data;
    }

    public void ControllerClick()
    {
      if (curr_obj != null && curr_obj.GetComponent<Button>() != null)
      {
        curr_obj.GetComponent<Button>().onClick.Invoke();
      }
    }
}
