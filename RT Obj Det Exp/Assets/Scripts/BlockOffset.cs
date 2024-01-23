using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockOffset : MonoBehaviour
{
    // This script auto algins the objects and sphere
    public GameObject sphere;

    public Bounds GrowBounds(Bounds a, Bounds b)
    {
       Vector3 max = Vector3.Max ( a.max, b.max );
       Vector3 min = Vector3.Min( a.min, b.min );

       a = new Bounds( (max + min)*0.5f, max - min );
       return a;
    }

    static Bounds GetLocalBoundsForObject(GameObject go)
    {
        var referenceTransform = go.transform;
        var b = new Bounds(Vector3.zero, Vector3.zero);
        RecurseEncapsulate(referenceTransform, ref b);
        return b;

        void RecurseEncapsulate(Transform child, ref Bounds bounds)
        {
            var mesh = child.GetComponent<MeshFilter>();
            if (mesh)
            {
                var lsBounds = mesh.sharedMesh.bounds;
                var wsMin = child.TransformPoint(lsBounds.center - lsBounds.extents);
                var wsMax = child.TransformPoint(lsBounds.center + lsBounds.extents);
                bounds.Encapsulate(referenceTransform.InverseTransformPoint(wsMin));
                bounds.Encapsulate(referenceTransform.InverseTransformPoint(wsMax));
            }
            foreach (Transform grandChild in child.transform)
            {
                RecurseEncapsulate(grandChild, ref bounds);
            }
        }
    }

    void Start()
    {
      var bounds = GetLocalBoundsForObject(this.transform.gameObject);
      var pos = bounds.center;
      var y_pos = bounds.center.y;

      // Keep it at the same height but center along x and z axis
      pos = new Vector3(-pos.x, 0.0f, -pos.z);
      // this.transform.localPosition = pos.center - (pos.extents / 2.0f);
      this.transform.localPosition = this.transform.localPosition + pos;
      this.transform.parent.localScale *= 0.1f;

      // Move the highlighting sphere to the center of the y axis
      sphere.transform.localPosition = new Vector3(0.0f, y_pos, 0.0f);
      sphere.transform.localScale *= 1.25f;
    }
}
