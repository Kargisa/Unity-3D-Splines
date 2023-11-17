using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.Splines;

public struct CubicBezier : IBezier, IBezierAuto
{
    public Vector3 p1 { get; set; }
    public Vector3 p2 { get; set; }
    public Vector3 p3 { get; set; }
    public Vector3 p4 { get; set; }
    public Quaternion r1 { get; set; }
    public Quaternion r2 { get; set; }
    public float Length { get => GetEstimatedLength(); }
    public readonly int ResPerMeter => 1000;


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

    public readonly float GetEstimatedLength(BezierResolution resolution) => Bezier.EstimateCurveLength(p1, p2, p3, p4, (int)resolution); 

    public readonly Vector3 GetPoint(float t) => Bezier.CubicBezier(p1, p2, p3, p4, t);

    public readonly float FastLengthEstimation() => new QuadraticBezier(p1, (3.0f * p3 - p4 + 3.0f * p2 - p1) / 4.0f, p3).Length;

    public readonly float TFromDistance(int resolution, float start, float distance) => Bezier.GetTFromDistance(p1, p2, p3, p4, resolution, start, distance);

    public readonly float[] EqualDistancesT(float distance, int resolution) => Bezier.GetEqualDistancesT(p1, p2, p3, p4, distance, resolution);
}
