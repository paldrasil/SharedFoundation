using System.Collections.Generic;
using UnityEngine;

namespace Shared.Foundation
{
    public class PolygonUtil
    {
        //Get minimum distance between a position and a line that goes from a to b
        //public static float GetEdgeDist(Vector3 pos, Vector3 a, Vector3 b)
        //{
        //    a.y = 0f;
        //    b.y = 0f;
        //    pos.y = 0f;

        //    Ray ray = new Ray(a, b - a);
        //    return Vector3.Cross(ray.direction, pos - ray.origin).magnitude;
        //}

        //Find if a point is inside a polygon (list of point is the polygon edge points)
        public static bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
        {
            Vector2 start;
            Vector2 end = polygon[polygon.Count - 1];

            int i = 0;
            bool is_inside = false;
            while (i < polygon.Count)
            {
                start = end;
                end = polygon[i++];
                is_inside ^= (end.y > point.y ^ start.y > point.y)
                    && ((point.x - end.x) < (point.y - end.y) * (start.x - end.x) / (start.y - end.y));
            }
            return is_inside;
        }

        //Get area size of a polygon
        //public static float AreaSizePolygon(Transform[] polygon)
        //{
        //    float area_size = 0;
        //    for (int i = 0; i < polygon.Length; i++)
        //    {
        //        if (i != polygon.Length - 1)
        //        {
        //            float mA = polygon[i].position.x * polygon[i + 1].position.z;
        //            float mB = polygon[i + 1].position.x * polygon[i].position.z;
        //            area_size = area_size + (mA - mB);
        //        }
        //        else
        //        {
        //            float mA = polygon[i].position.x * polygon[0].position.z;
        //            float mB = polygon[0].position.x * polygon[i].position.z;
        //            area_size = area_size + (mA - mB);
        //        }
        //    }
        //    return Mathf.Abs(area_size * 0.5f);
        //}

        //Return minimum XY of polygon
        public static Vector2 GetPolygonMin(List<Vector2> polygon)
        {
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            foreach (Vector2 point in polygon)
            {
                if (point.x < min.x)
                    min.x = point.x;
                if (point.y < min.y)
                    min.y = point.y;
            }
            return min;
        }

        //Return maximum XY of polygon
        public static Vector2 GetPolygonMax(List<Vector2> polygon)
        {
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            foreach (Vector2 point in polygon)
            {
                if (point.x > max.x)
                    max.x = point.x;
                if (point.y > max.y)
                    max.y = point.y;
            }
            return max;
        }
    }
}
