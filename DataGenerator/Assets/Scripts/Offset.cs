using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Offset : MonoBehaviour
{
    void Start()
    {
      var center = new Vector3(0,0,0);
      var centroid = new Vector3(0,0,0);
      for (int i = 0; i < this.transform.childCount; i++)
      {
          var ch_obj = this.transform.GetChild(i);
          centroid += ch_obj.transform.localPosition;

      }
      centroid /= (this.transform.childCount);

      this.transform.localPosition = this.transform.localPosition + (center-centroid);
    }
}
