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

    /// <summary>
    /// Estimates the length of the Cubic Beziere in meters
    /// </summary>
    /// <param name="p1">point 1</param>
    /// <param name="p2">point 2</param>
    /// <param name="p3">point 3</param>
    /// <param name="p4">point 4</param>
    /// <param name="resolution">the amount of divisions along the curve <c>[higher == more precision]</c> </param>
    /// <returns>The estimated distance of the Beziere Curve in meters</returns>
    public static float EstimateCurveLength(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, int resolution)
    {
        float length = 0;
        Vector3 previousPoint = p1;
        for (int i = 1; i <= resolution; i++)
        {
            float t = (float)i / resolution;
            Vector3 currentPoint = CubicBezier(p1, p2, p3, p4, t);
            length += Vector3.Distance(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
        return length;
    }

    public static float[] GetEqualDistancesT(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float segmentLength, int resolution)
    {
        float curveLength = EstimateCurveLength(p1, p2, p3, p4, resolution);
        int numPoints = Mathf.CeilToInt(curveLength / segmentLength);
        float[] ts = new float[numPoints];

        int currentSegment = 0;
        float currentLength = 0f;
        Vector3 previousPoint = p1;

        for (int i = 1; i <= numPoints; i++)
        {
            float targetLength = i * segmentLength;
            while (currentLength < targetLength && currentSegment < resolution)
            {
                float t = (float)currentSegment / resolution;
                Vector3 currentPoint = CubicBezier(p1, p2, p3, p4, t);
                currentLength += Vector3.Distance(previousPoint, currentPoint);
                previousPoint = currentPoint;
                currentSegment++;
            }
            ts[i-1] = (float)(currentSegment - 1) / resolution;
        }
        return ts;
    }

}
