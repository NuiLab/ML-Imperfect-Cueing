using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddDepth : MonoBehaviour
{
    float scale = 0.0f;
    
    void Start()
    {
      scale = this.transform.localScale.x;
    }

    void Update()
    {
        var size = Random.Range(1.0f, 2.5f);
        this.transform.localScale = new Vector3(scale * size, scale * size, scale * size);
    }
}
