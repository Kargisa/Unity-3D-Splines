using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public struct CubicBezier : IBezier, IBezierAuto, IFormattable
{
    public Vector3 p1 { get; set; }
    public Vector3 p2 { get; set; }
    public Vector3 p3 { get; set; }
    public Vector3 p4 { get; set; }
    public Quaternion r1 { get; set; }
    public Quaternion r2 { get; set; }
    public readonly float Length => GetEstimatedLength();
    public readonly int ResPerMeter => 100;


    public CubicBezier(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Quaternion r1, Quaternion r2)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
        this.p4 = p4;
        this.r1 = r1;
        this.r2 = r2;
    }

    public CubicBezier(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
        this.p4 = p4;
        this.r1 = Quaternion.identity;
        this.r2 = Quaternion.identity;
    }

    public Vector3 this[int i]
    {
        get
        {
            return i switch
            {
                0 => p1,
                1 => p2,
                2 => p3,
                3 => p4,
                _ => throw new System.IndexOutOfRangeException(),
            };
        }
        set
        {
            switch (i)
            {
                case 0:
                    p1 = value;
                    break;
                case 1:
                    p2 = value;
                    break;
                case 2:
                    p3 = value;
                    break;
                case 3:
                    p4 = value;
                    break;
                default:
                    throw new System.IndexOutOfRangeException();
            }
        }
    }

    public readonly float GetEstimatedLength() => Bezier.EstimateCurveLength(p1, p2, p3, p4, Mathf.CeilToInt(FastLengthEstimation()) * ResPerMeter);

    public readonly float GetEstimatedLength(int resolution) => Bezier.EstimateCurveLength(p1, p2, p3, p4, resolution);
    
    public readonly float FastLengthEstimation() => new QuadraticBezier(p1, (3.0f * p3 - p4 + 3.0f * p2 - p1) / 4.0f, p3).FastLengthEstimation();
    
    public readonly Vector3 GetPoint(float t) => Bezier.CubicBezier(p1, p2, p3, p4, t);

    public readonly float PointFromDistance(int resolution, float start, float distance) => Bezier.GetPointFromDistance(p1, p2, p3, p4, resolution, start, distance);

    public readonly float[] EqualDistancePoints(float distance, int resolution) => Bezier.GetEqualDistancePoints(p1, p2, p3, p4, distance, resolution);

    public readonly float PointFromDistance(float start, float distance)
    {
        int resolution;
        if (distance < 1)
            resolution = Mathf.CeilToInt(1 / distance * ResPerMeter);
        else
            resolution = Mathf.CeilToInt(distance * ResPerMeter);
        
        return PointFromDistance(resolution, start, distance);

    }

    public readonly float[] EqualDistancePoints(float distance)
    {
        float resolution;
        float fastLength = FastLengthEstimation();
        if (fastLength < 1)
            resolution = 1 / fastLength * ResPerMeter;
        else
            resolution = fastLength * ResPerMeter;
        return EqualDistancePoints(distance, Mathf.CeilToInt(resolution / Mathf.Min(1, distance)));
    }

    public readonly CubicBezier Transform(Matrix4x4 transformation)
    {
        Quaternion matrixQuad = transformation.rotation;

        return new CubicBezier(
            transformation.MultiplyPoint(p1),
            transformation.MultiplyPoint(p2),
            transformation.MultiplyPoint(p3),
            transformation.MultiplyPoint(p4),
            matrixQuad * r1,
            matrixQuad * r2
            );
        
    }

    public override string ToString() => ToString(null, null);

    public string ToString(string format) => ToString(format, null);
    
    public string ToString(string format, IFormatProvider formatProvider)
    {
        if (string.IsNullOrEmpty(format))
            format = "F2";

        formatProvider ??= System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        return string.Format(
            formatProvider, 
            "p1: {0} \np2: {1} \np3: {2} \np4: {3} \nr1: {4} \nr2 {5}", 
            p1.ToString(format, formatProvider), 
            p2.ToString(format, formatProvider), 
            p3.ToString(format, formatProvider), 
            p4.ToString(format, formatProvider), 
            r1.ToString(format, formatProvider), 
            r2.ToString(format, formatProvider));
    }
}
