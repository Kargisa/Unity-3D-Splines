using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Bezier
{
    public static Vector3 QuadratcBezier(Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        Vector3 a = Vector3.Lerp(p1, p2, t);
        Vector3 b = Vector3.Lerp(p2, p3, t);
        return Vector3.Lerp(a, b, t);
    }
    
    public static Vector3 CubicBezier(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t)
    {
        Vector3 a = QuadratcBezier(p1, p2, p3, t);
        Vector3 b = QuadratcBezier(p2, p3, p4, t);
        return Vector3.Lerp(a, b, t);
    }

}
