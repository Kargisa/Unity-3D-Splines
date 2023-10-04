using UnityEngine;

public struct QuadraticBezier : IBezier, IBezierAuto
{
    public Vector3 p1 { get; set; }
    public Vector3 p2 { get; set; }
    public Vector3 p3 { get; set; }
    public float Length { get; set; }
    public readonly int ResPerMeter => 1000;


    public QuadraticBezier(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
        Length = 0;
        Length = GetLength();
    }

    public readonly float GetLength() => Bezier.EstimateCurveLength(p1, p2, p3, Mathf.CeilToInt(FastLengthEstimation()) * ResPerMeter);

    public readonly float GetLength(int resolution) => Bezier.EstimateCurveLength(p1, p2, p3, resolution);

    public readonly float GetLength(BezierResolution resolution) => Bezier.EstimateCurveLength(p1, p2, p3, (int)resolution);

    public readonly Vector3 GetPoint(float t) => Bezier.QuadratcBezier(p1, p2, p3, t);

    public readonly float FastLengthEstimation() => (Vector3.Distance(p1, p2) + Vector3.Distance(p2, p3)) / 2 + Vector3.Distance(p1, p3) / 2;

}
