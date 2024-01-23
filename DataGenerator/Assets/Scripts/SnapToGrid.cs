using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapToGrid : MonoBehaviour
{
    public bool snap = false;
    public bool ZERO = false;
    private void OnDrawGizmos()
    {
      if (snap){
        Snap();
      }
    }

    void Snap()
    {
        var r = GetComponent<Renderer>();
        if (r == null)
            return;

        var bounds = r.bounds;
        var min = new Vector3(
          bounds.center.x - bounds.extents.x,
          bounds.center.y - bounds.extents.y,
          bounds.center.z + bounds.extents.z
        );

        var offs = transform.position - bounds.center;

        var position = new Vector3(
          Mathf.Round(min.x / 0.1579762f) * 0.1579762f,
          Mathf.Round(min.y / 0.1907831f) * 0.1907831f,
          Mathf.Round(min.z / 0.1574707f) * 0.1574707f
        );
        if (ZERO){
          position = Vector3.zero;
        }
        transform.position = (bounds.center + (position - min)) + offs;
    }
}
