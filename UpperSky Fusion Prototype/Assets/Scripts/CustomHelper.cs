using UnityEngine;

public static class CustomHelper 
{
   public static Vector3 ReturnPosInTopDown(Vector3 originalPos)
   {
      return new Vector3(originalPos.x, 0, originalPos.z);
   }

   public static float ReturnDistanceInTopDown(Vector3 pos1, Vector3 pos2)
   {
      return Vector3.Distance(ReturnPosInTopDown(pos1), ReturnPosInTopDown(pos2));
   }
}