using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightManager : MonoBehaviour
{
    public GameObject HighlightSphere;

    public void Enable(bool correctObject)
    {
      HighlightSphere.GetComponent<Outline>().enabled = true;
      if (correctObject)
      {
        HighlightSphere.GetComponent<Outline>().OutlineColor = Color.red;
      }
    }

    public void Disable()
    {
      HighlightSphere.GetComponent<Outline>().enabled = false;
      HighlightSphere.GetComponent<Outline>().OutlineColor = Color.white;
    }
}
