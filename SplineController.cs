using System.Data.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

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

    //private void Start()
    //{
    //    CubicBezier b = path.GetBezierOfSegment(2);
    //    Debug.Log("L " + b.Length);
    //    Debug.Log(b.FastLengthEstimation());
    //}

    /// <summary>
    /// Creates an new path at (0, 0, 0) in local space
    /// </summary>
    public void CreatePath()
    {
        path = new Path(new Vector3(0, 0, 0));
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

        CubicBezier bezier = path.GetBezierOfSegment(currentSegment);

        return bezier.GetPoint(segmentT);

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
            CubicBezier bezier = path.GetBezierOfSegment(i);
            length += bezier.GetEstimatedLength(resolution);
        }
        return length;
    }

    /// <summary>
    /// Automatically calculates the length of the spline
    /// </summary> 
    /// <returns>Length of the spline in <c>meters</c></returns>
    public float CalculateSplineLength()
    {
        float length = 0;
        for (int i = 0; i < path.NumSegments; i++)
        {
            CubicBezier bezier = path.GetBezierOfSegment(i);
            length += bezier.Length;
        }
        return length;
    }


    public override string ToString() => $"path: {path}";
}
