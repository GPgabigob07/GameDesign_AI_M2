using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Utils
{
    public static T RandomEntry<T>(this IList<T> list)
    {
        var index = Random.Range(0, list.Count);
        return list[index];
    }

    public static Vector2Int ToInt(this Vector2 vector) => new(vector.x.Floor(), vector.y.Floor());
    public static Vector3Int ToInt(this Vector3 vector) => new(vector.x.Floor(), vector.y.Floor(), vector.z.Floor());
    
    public static Vector3Int To3D(this Vector2Int vector) => new(vector.x, vector.y, 0);
    public static Vector2Int To2D(this Vector3Int vector) => new(vector.x, vector.y);
    
    public static Vector2 ToFloat(this Vector2Int vector) => new(vector.x, vector.y);
    public static Vector3 ToFloat(this Vector3Int vector) => new(vector.x, vector.y, vector.z);
    
    public static Vector3Int Sum(this Vector3Int v, Vector2Int other) => new(other.x + v.x, other.y + v.y, v.z);
    public static Vector3Int Minus(this Vector3Int v, Vector2Int other) => new(other.x - v.x, other.y - v.y, v.z);    
    
    public static Vector2Int Sum(this Vector2Int v, Vector3Int other) => new(other.x + v.x, other.y + v.y);
    public static Vector2Int Minus(this Vector2Int v, Vector3Int other) => new(other.x - v.x, other.y - v.y);

    public static int Floor(this float num) => Mathf.FloorToInt(num);
    public static int Round(this float num) => Mathf.RoundToInt(num);
}