using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddBlur : MonoBehaviour
{
    void Update()
    {
      UnityEngine.Rendering.Universal.UniversalAdditionalCameraData uac = this.GetComponent<Camera>().GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
      uac.renderPostProcessing = (Random.Range(0, 2) == 1);
    }
}
