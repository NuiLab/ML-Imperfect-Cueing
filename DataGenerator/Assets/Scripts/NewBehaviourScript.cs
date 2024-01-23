using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public Transform obj;
    // Start is called before the first frame update
    void Start()
    {
      float xdiff = 0.0F;
      float ydiff = 0.0F;
      float zdiff = 0.0F;

      for (int i = 0; i < obj.childCount; i++)
      {
          var ch_obj = obj.GetChild(i);
          if (ch_obj.tag == "anchor")
          {
              xdiff = 0.0F - ch_obj.transform.localPosition.x;
              ydiff = 0.0F - ch_obj.transform.localPosition.y;
              zdiff = 0.0F - ch_obj.transform.localPosition.z;
          }
      }
      for (int i = 0; i < obj.childCount; i++)
      {
          var ch_obj = obj.GetChild(i);

          print("Block Name: \n" + ch_obj +  "Vector3(" + (ch_obj.transform.localPosition.x + xdiff) + "," + (ch_obj.transform.localPosition.y + ydiff) + "," + (ch_obj.transform.localPosition.z + zdiff) + ")");
      }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
