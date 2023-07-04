using UnityEngine;

public static class CustomHelper 
{
   public static Vector3 ReturnPosInTopDown(Vector3 originalPos)
   {
      return new Vector3(originalPos.x, 0, originalPos.z);
   }
}