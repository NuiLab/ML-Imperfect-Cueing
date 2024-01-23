using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelperFunctions : MonoBehaviour
{
  public Vector3 StringToVector3(string sVector)
  {
     if (sVector.StartsWith ("(") && sVector.EndsWith (")")) {
         sVector = sVector.Substring(1, sVector.Length-2);
     }
     string[] sArray = sVector.Split(',');
     Vector3 result = new Vector3(
         float.Parse(sArray[0]),
         float.Parse(sArray[1]),
         float.Parse(sArray[2]));

     return result;
  }

  public Quaternion StringToQuaternion(string sVector)
  {
     if (sVector.StartsWith ("(") && sVector.EndsWith (")")) {
         sVector = sVector.Substring(1, sVector.Length-2);
     }
     string[] sArray = sVector.Split(',');
     Quaternion result = new Quaternion(
         float.Parse(sArray[0]),
         float.Parse(sArray[1]),
         float.Parse(sArray[2]),
         float.Parse(sArray[3]));

     return result;
  }
}
