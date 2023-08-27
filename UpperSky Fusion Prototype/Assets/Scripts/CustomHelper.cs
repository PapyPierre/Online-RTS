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
   
   public static Vector2 GenerateRandomPosIn2DArea(Vector2 area)
   {
      return new Vector2(
         Random.Range(-area.x,area.x), 
         Random.Range(-area.y,area.y));
   }
   public static Vector3 GenerateRandomPosIn2DArea(Vector3 area)
   {
      return new Vector2(
         Random.Range(-area.z,area.z), 
         Random.Range(-area.x,area.x));
   } 
   public static Vector2 GenerateRandomPosIn2DArea(float height, float width) 
   {
      return new Vector2(
         Random.Range(-width, width),
         Random.Range(-height, height));
   }
}