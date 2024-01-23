using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomLight : MonoBehaviour
{
    void Update()
    {
      this.transform.GetComponent<Light>().intensity = Random.Range(0.01f, 0.5f);
    }
}
