using System;
using System.Collections;
using System.Collections.Generic;
using Extras;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Utils
{
    public static T RandomEntry<T>(this IList<T> list)
    {
        switch (list.Count)
        {
            case 0:
                return default;
            case 1:
                return list[0];
        }

        var index = Random.Range(0, list.Count);
        index = Mathf.Clamp(index, 0, list.Count - 1);
        return list[index];
    }

    public static Vector2Int ToInt(this Vector2 vector) => new(vector.x.Floor(), vector.y.Floor());
    public static Vector3Int ToInt(this Vector3 vector) => new(vector.x.Floor(), vector.y.Floor(), vector.z.Floor());

    public static Vector3Int To3D(this Vector2Int vector) => new(vector.x, vector.y, 0);
    public static Vector2Int To2D(this Vector3Int vector) => new(vector.x, vector.y);
    public static Vector3 To3D(this Vector2 vector) => new(vector.x, vector.y, 0);
    public static Vector2 To2D(this Vector3 vector) => new(vector.x, vector.y);

    public static Vector2 ToFloat(this Vector2Int vector) => new(vector.x, vector.y);
    public static Vector3 ToFloat(this Vector3Int vector) => new(vector.x, vector.y, vector.z);

    public static Vector3Int Sum(this Vector3Int v, Vector2Int other) => new(other.x + v.x, other.y + v.y, v.z);
    public static Vector3Int Minus(this Vector3Int v, Vector2Int other) => new(other.x - v.x, other.y - v.y, v.z);

    public static Vector2Int Sum(this Vector2Int v, Vector3Int other) => new(other.x + v.x, other.y + v.y);
    public static Vector2Int Minus(this Vector2Int v, Vector3Int other) => new(other.x - v.x, other.y - v.y);

    public static EnumerableBounds Points(this Bounds bounds) => new(bounds);

    public static int Floor(this float num) => Mathf.FloorToInt(num);
    public static int Round(this float num) => Mathf.RoundToInt(num);

    public static void ForEach<T>(this T[,] enumerable, Func<int, int, bool?> action)
    {
        var height = enumerable.GetLength(0);
        var length = enumerable.GetLength(1);

        for (var i = 0; i < height; i++)
        for (var j = 0; j < length; j++)
        {
            var result = action(i, j);
            if (result.HasValue && !result.Value) continue;
            if (result.HasValue) break;
        }
    }
    
    public static string ToKnFormat(long value)
    {
        if (value < 10_000)
            return value.ToString();

        long divisor = 1_000;
        int kCount = 1;

        // Determine how many Ks the number requires
        while (value >= divisor * 1000)
        {
            divisor *= 1000;
            kCount++;

            if (kCount > 4)
                return "A lot...";
        }

        var shortened = value / (float)divisor;
        var prefix = new string('K', kCount);
        return Format(shortened) + prefix;
    }

    private static string Format(float val)
    {
        if (val % 1f == 0)
            return ((int)val).ToString();

        return val.ToString("0.#").Replace('.', ',');
    }

    public static Debounce CreateDebounce(this MonoBehaviour mono)
    {
        return new Debounce(mono);
    }

    public class EnumerableBounds : IEnumerable<Vector2>
    {
        readonly List<Vector2> points = new();

        public EnumerableBounds(Bounds bounds)
        {
            for (var x = bounds.min.x.Floor(); x <= bounds.max.x.Round(); x++)
            for (var y = bounds.min.y.Floor(); y <= bounds.max.y.Round(); y++)
                points.Add(new Vector2(x, y));

            /*foreach (var point in this)
            {
                Debug.Log($"#Bounds#: {point.x},{point.y}");
            }*/
        }

        public IEnumerator<Vector2> GetEnumerator()
        {
            return points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)points).GetEnumerator();
        }
    }
}