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
        if (t < 0 && t > 1)
            throw new System.ArgumentException("t must be between 0 and 1", nameof(t));

        Vector3 a = QuadratcBezier(p1, p2, p3, t);
        Vector3 b = QuadratcBezier(p2, p3, p4, t);
        return Vector3.Lerp(a, b, t);
    }

    /// <summary>
    /// Estimates the length of the Cubic Beziere in meters
    /// </summary>
    /// <param name="p1">anchor 1</param>
    /// <param name="p2">controle 1</param>
    /// <param name="p3">controle 2</param>
    /// <param name="p4">anchor 2</param>
    /// <param name="resolution">the amount of divisions along the curve <c>[higher == more precision]</c></param>
    /// <returns>The estimated length of the Beziere Curve in meters</returns>
    public static float EstimateCurveLength(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, int resolution)
    {
        resolution = Mathf.Max(resolution, 0);

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

    /// <summary>
    /// Estimates the length of the Quadratic Beziere in meters
    /// </summary>
    /// <param name="p1">anchor 1</param>
    /// <param name="p2">controle 1</param>
    /// <param name="p3">anchor 2</param>
    /// <param name="resolution">the amount of divisions along the curve <c>[higher == more precision]</c></param>
    /// <returns>The estimated length of the Beziere Curve in meters</returns>
    public static float EstimateCurveLength(Vector3 p1, Vector3 p2, Vector3 p3, int resolution)
    {
        resolution = Mathf.Max(resolution, 0);

        float length = 0;
        Vector3 previousPoint = p1;
        for (int i = 1; i <= resolution; i++)
        {
            float t = (float)i / resolution;
            Vector3 currentPoint = QuadratcBezier(p1, p2, p3, t);
            length += Vector3.Distance(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
        return length;
    }

    /// <summary>
    /// Gets the T value with a certain distance from a origin
    /// </summary>
    /// <param name="p1">anchor 1</param>
    /// <param name="p2">controle 1</param>
    /// <param name="p3">controle 2</param>
    /// <param name="p4">anchor 2</param>
    /// <param name="start">The value T from where the distance starts from</param>
    /// <param name="resolution">the amount of divisions along the curve <c>[higher == more precision]</c></param>
    /// <param name="distance">The distnace from a <c>start</c> value T</param>
    /// <returns>The value T that is <c>length</c> away from <c>start</c></returns>
    public static float GetTFromDistance(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, int resolution, float start, float distance)
    {
        int dir = float.IsNegative(distance) ? -1 : 1;
        start = Mathf.Clamp01(start);
        Vector3 previousPoint = CubicBezier(p1, p2, p3, p4, start);
        
        float currentLength = 0;
        int currentSegment = 0;
        while (currentLength < Mathf.Abs(distance) && currentSegment < resolution)
        {
            float t = (float)currentSegment / resolution * dir + start;
            if (t >= 1)
                return Mathf.Clamp01(t);
            Vector3 currentPoint = CubicBezier(p1, p2, p3, p4, t);
            currentLength += Vector3.Distance(previousPoint, currentPoint);
            previousPoint = currentPoint;
            currentSegment++;
        }
        float finalT = Mathf.Clamp01(((float)currentSegment - 1) / resolution * dir + start);
        return finalT;
    }

    /// <summary>
    /// Gets the T value with a certain distance from a origin
    /// </summary>
    /// <param name="p1">anchor 1</param>
    /// <param name="p2">controle 1</param>
    /// <param name="p3">controle 2</param>
    /// <param name="start">The value T from where the distance starts from</param>
    /// <param name="resolution">the amount of divisions along the curve <c>[higher == more precision]</c></param>
    /// <param name="distance">The distnace from a <c>start</c> value T</param>
    /// <returns>The value T that is <c>length</c> away from <c>start</c></returns>
    public static float GetTFromDistance(Vector3 p1, Vector3 p2, Vector3 p3, int resolution, float start, float distance)
    {
        int dir = float.IsNegative(distance) ? -1 : 1;
        start = Mathf.Clamp01(start);
        Vector3 previousPoint = QuadratcBezier(p1, p2, p3, start);

        float currentLength = 0;
        int currentSegment = 0;
        while (currentLength < Mathf.Abs(distance) && currentSegment < resolution)
        {
            float t = (float)currentSegment / resolution * dir + start;
            if (t >= 1)
                return Mathf.Clamp01(t);
            Vector3 currentPoint = QuadratcBezier(p1, p2, p3, t);
            currentLength += Vector3.Distance(previousPoint, currentPoint);
            previousPoint = currentPoint;
            currentSegment++;
        }
        float finalT = Mathf.Clamp01(((float)currentSegment - 1) / resolution * dir + start);
        return finalT;
    }

    /// <summary>
    /// Gets all values T with distance <c>segmentLength</c> from each other
    /// </summary>
    /// <param name="p1">anchor 1</param>
    /// <param name="p2">controle 1</param>
    /// <param name="p3">controle 2</param>
    /// <param name="p4">anchor 2</param>
    /// <param name="segmentLength">The distnace between two neighboring <c>T</c> values</param>
    /// <param name="resolution">the amount of divisions along the curve <c>[higher == more precision]</c></param>
    /// <returns>All values <c>T</c> that are equally distaned from each other</returns>
    public static float[] GetEqualDistancesT(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float segmentLength, int resolution)
    {
        if (segmentLength <= 0)
            throw new System.ArgumentException("segmentLength must not be smaller than 0", nameof(segmentLength));
        if (resolution <= 0)
            throw new System.ArgumentException("resolution must not be smaller than 0", nameof(resolution));

        float curveLength = EstimateCurveLength(p1, p2, p3, p4, resolution);
        int numPoints = Mathf.FloorToInt(curveLength / segmentLength);
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

    /// <summary>
    /// Gets all values T with distance <see>segmentLength</c> from each other
    /// </summary>
    /// <param name="p1">anchor 1</param>
    /// <param name="p2">controle 1</param>
    /// <param name="p3">controle 2</param>
    /// <param name="segmentLength">The distnace between two neighboring <c>T</c> values</param>
    /// <param name="resolution">the amount of divisions along the curve <c>[higher == more precision]</c></param>
    /// <returns>All values <c>T</c> that are equally distaned from each other</returns>
    public static float[] GetEqualDistancesT(Vector3 p1, Vector3 p2, Vector3 p3, float segmentLength, int resolution)
    {
        segmentLength = Mathf.Max(segmentLength, 0f);
        resolution = Mathf.Max(resolution, 0);

        float curveLength = EstimateCurveLength(p1, p2, p3, resolution);
        int numPoints = Mathf.FloorToInt(curveLength / segmentLength);
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
                Vector3 currentPoint = QuadratcBezier(p1, p2, p3, t);
                currentLength += Vector3.Distance(previousPoint, currentPoint);
                previousPoint = currentPoint;
                currentSegment++;
            }
            ts[i - 1] = (float)(currentSegment - 1) / resolution;
        }
        return ts;
    }

}
