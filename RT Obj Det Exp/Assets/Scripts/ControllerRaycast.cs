using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ControllerRaycast : MonoBehaviour
{
    public GameObject dot;
    public VRPointerToCanvas vrPointerToCanvas;
    public LineRenderer lineRenderer;
    public float def_length = 5.0f;

    float length;
    RaycastHit hit;
    Ray ray;
    Vector3 endPosition;
    PointerEventData eventData;


    private void CreateRaycst()
    {
      ray = new Ray(transform.position, transform.forward);
      Physics.Raycast(ray, out hit, length);
    }

    void Update()
    {
        PointerEventData data = vrPointerToCanvas.GetData();
        length = data.pointerCurrentRaycast.distance == 0 ? def_length : data.pointerCurrentRaycast.distance;
        CreateRaycst();
        endPosition = this.transform.position + (this.transform.forward * length);
        if (hit.collider != null)
        {
          endPosition = hit.point;
        }
        dot.transform.position = endPosition;
        lineRenderer.SetPosition(0, this.transform.position);
        lineRenderer.SetPosition(1, endPosition);
    }
}
