using UnityEngine;

public struct QuadraticBezier : IBezier, IBezierAuto
{
    public Vector3 p1 { get; set; }
    public Vector3 p2 { get; set; }
    public Vector3 p3 { get; set; }
    public Quaternion r1 { get; set; }
    public Quaternion r2 { get; set; }
    public float Length { get => GetEstimatedLength(); }
    public readonly int ResPerMeter => 1000;


    public QuadraticBezier(Vector3 p1, Vector3 p2, Vector3 p3, Quaternion r1, Quaternion r2)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
        this.r1 = r1;
        this.r2 = r2;
    }

    public QuadraticBezier(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
        this.r1 = Quaternion.identity;
        this.r2 = Quaternion.identity;
    }

    public readonly float GetEstimatedLength() => Bezier.EstimateCurveLength(p1, p2, p3, Mathf.CeilToInt(FastLengthEstimation()) * ResPerMeter);

    public readonly float GetEstimatedLength(int resolution) => Bezier.EstimateCurveLength(p1, p2, p3, resolution);

    public readonly float GetEstimatedLength(BezierResolution resolution) => Bezier.EstimateCurveLength(p1, p2, p3, (int)resolution);

    public readonly Vector3 GetPoint(float t) => Bezier.QuadratcBezier(p1, p2, p3, t);

    public readonly float FastLengthEstimation() => (Vector3.Distance(p1, p2) + Vector3.Distance(p2, p3)) / 2 + Vector3.Distance(p1, p3) / 2;

    public readonly float TFromDistance(int resolution, float start, float distance) => Bezier.GetTFromDistance(p1, p2, p3, resolution, start, distance);

    public readonly float[] EqualDistancesT(float distance, int resolution) => Bezier.GetEqualDistancesT(p1, p2, p3, distance, resolution);

}
