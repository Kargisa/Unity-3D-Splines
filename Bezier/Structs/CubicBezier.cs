using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph;
using UnityEngine;

public struct CubicBezier : IBezier, IBezierAuto
{
    public Vector3 p1 { get; set; }
    public Vector3 p2 { get; set; }
    public Vector3 p3 { get; set; }
    public Vector3 p4 { get; set; }
    public float Length { get; set; }
    public readonly int ResPerMeter => 1000;


    public CubicBezier(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
        this.p4 = p4;
        Length = 0;
        Length = GetLength();
    }

    public readonly float GetLength() => Bezier.EstimateCurveLength(p1, p2, p3, p4, Mathf.CeilToInt(FastLengthEstimation()) * ResPerMeter);

    public readonly float GetLength(int resolution) => Bezier.EstimateCurveLength(p1, p2, p3, p4, resolution);

    public readonly float GetLength(BezierResolution resolution) => Bezier.EstimateCurveLength(p1, p2, p3, p4, (int)resolution); 

    public readonly Vector3 GetPoint(float t) => Bezier.CubicBezier(p1, p2, p3, p4, t);

    public readonly float FastLengthEstimation() => new QuadraticBezier(p1, (3.0f * p3 - p4 + 3.0f * p2 - p1) / 4.0f, p3).Length;

}
