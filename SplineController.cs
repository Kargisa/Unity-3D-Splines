using UnityEngine;

public class SplineController : MonoBehaviour
{
#if UNITY_EDITOR
    [HideInInspector]
    public bool pathFoldOut = false;
    [HideInInspector]
    public bool rotationsFoldOut = false;
    [HideInInspector]
    public bool infoFoldOut = false;
    [HideInInspector]
    public bool checkpointsFoldOut = false;
    [HideInInspector]
    public bool isRotate = false;
    [HideInInspector]
    public SplineCustomizer custom;
    [HideInInspector]
    public bool customizerFoldOut = true;
#endif

    [HideInInspector]
    public Path path;

    /// <summary>
    /// Creates an new path at <c>SplineController.transform.position</c> in local space
    /// </summary>
    public void CreatePath()
    {
        path = new Path(transform.position);
    }

    /// <summary>
    /// Calculate the position on the path <b>in local space</b>
    /// </summary>
    /// <param name="t">progress clamped between <c>[0 and NumSegments]</c></param>
    /// <returns>Position on the spline [local space]</returns>
    public Vector3 CalculatePosition(float t)
    {
        int numSegments = path.NumSegments;
        t = Mathf.Clamp(t, 0, numSegments);
        int currentSegment = Mathf.FloorToInt(t) + (t == numSegments ? -1 : 0);
        float segmentT = t - currentSegment;

        Vector3[] points = path.GetPointsInSegment(currentSegment);

        Vector3 p = Bezier.CubicBezier(points[0], points[1], points[2], points[3], segmentT);

        return p;
    }

    /// <summary>
    /// Calculate the position on the path <b>in world space</b>
    /// </summary>
    /// <param name="t">progress clamped between <c>[0 and NumSegments]</c></param>
    /// <returns>Position on the spline [world space]</returns>
    public Vector3 CalculatePositionWorld(float t) => transform.TransformPoint(CalculatePosition(t));

    /// <summary>
    /// Calculates the Rotation on the spline <b>in local space</b> at the given progress <c>t</c>
    /// </summary>
    /// <param name="t">progress clamped between <c>[0 and NumSegments]</c></param>
    /// <returns>Rotation on the spline [local space]</returns>
    public Quaternion CalculateRotation(float t)
    {
        int numSegments = path.NumSegments;
        t = Mathf.Clamp(t, 0, numSegments);
        int currentSegment = Mathf.FloorToInt(t) + (t == numSegments ? -1 : 0);
        float segmentT = t - currentSegment;

        return Quaternion.Slerp(path.GetRotation(currentSegment), path.GetRotation(currentSegment + 1), segmentT);
    }

    /// <summary>
    /// Calculates the Rotation on the spline <b>in world space</b> at the given progress <c>t</c>
    /// </summary>
    /// <param name="t">progress clamped between <c>[0 and NumSegments]</c></param>
    /// <returns>Rotation on the spline [world space]</returns>
    public Quaternion CalculateRotationWorld(float t) => transform.rotation * CalculateRotation(t);

    /// <summary>
    /// Use custom resolution for the bezier length calculations
    /// </summary> 
    /// <param name="resolution">The resolution of the Bezier</param>
    /// <returns>Length of the spline in <c>meters</c></returns>
    public float CalculateSplineLength(int resolution)
    {
        float length = 0;
        for (int i = 0; i < path.NumSegments; i++)
        {
            Vector3[] points = path.GetPointsInSegment(i);
            length += Bezier.EstimateCurveLength(points[0], points[1], points[2], points[3], resolution);
        }
        return length;
    }
    
    /// <summary>
    /// Calculates the length of the spline based on the given resolution
    /// </summary> 
    /// <param name="resolution">The resolution option of the Bezier</param>
    /// <returns>Length of the spline in <c>meters</c></returns>
    public float CalculateSplineLength(BezierResolution resolution = BezierResolution.High) => CalculateSplineLength((int)resolution);


    public override string ToString() => $"path: {path}";
}
