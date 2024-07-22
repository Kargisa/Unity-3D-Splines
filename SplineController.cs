using System.Collections.Generic;
using System.Linq;
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

    [HideInInspector]
    public List<float[]> bufferedArrowDistribution = new();

#endif
    [SerializeField, HideInInspector]
    private bool isStatic;

    public bool IsStatic
    {
        get => isStatic;
        set => isStatic = value;
    }

    /// <summary>
    /// The logic of the spline
    /// </summary>
    [SerializeField, HideInInspector]
    private Path path;

    public Path Path { get => path; }

    private float _length;

    /// <summary>
    /// All checkpoints created on the path
    /// </summary>
    [SerializeField, HideInInspector]
    private List<PathCheckpoint> checkpoints;

    public List<PathCheckpoint> Checkpoints => checkpoints;

    /// <summary>
    /// The last added checkpoint
    /// </summary>
    public PathCheckpoint LastCheckPoint => checkpoints[^1];

    private void Start()
    {
        if (IsStatic)
        {
            _length = GetLength();
        }

        Debug.Log("HI");
        /*
         * Resolutions per meter
         * 
         * 50 => 8.41389
         * 100 => 8.414043
         * 1000 => 8.414093
         * 
         * FULL => 8.414089
        */
    }

    private void Reset()
    {
        CreatePath();
    }

    /// <summary>
    /// Creates an new path at (0, 0, 0) in local space
    /// </summary>
    public void CreatePath()
    {
        checkpoints = new();
        path = new(new(0, 0, 0));
        CreateCheckpoint();
    }

    /// <summary>
    /// Creates an new path at the given position in local space
    /// </summary>
    public void CreatePath(Vector3 position)
    {
        checkpoints = new();
        path = new(position);
        CreateCheckpoint();
    }

    /// <summary>
    /// Calculate the position on the path <b>in local space</b>
    /// </summary>
    /// <param name="t">progress clamped between <c>[0 and NumSegments]</c></param>
    /// <returns>Position on the spline [local space]</returns>
    public Vector3 GetPosition(float t)
    {
        int numSegments = Path.NumSegments;
        t = Mathf.Clamp(t, 0, numSegments);
        int currentSegment = Mathf.FloorToInt(t) + (t == numSegments ? -1 : 0);
        float segmentT = t - currentSegment;

        CubicBezier bezier = Path.GetBezierOfSegment(currentSegment);

        return bezier.GetPoint(segmentT);

    }

    /// <summary>
    /// Calculate the position on the path <b>in world space</b>
    /// </summary>
    /// <param name="t">progress clamped between <c>[0 and NumSegments]</c></param>
    /// <returns>Position on the spline [world space]</returns>
    public Vector3 GetPositionWorld(float t) => transform.TransformPoint(GetPosition(t));

    /// <summary>
    /// Calculates the Rotation on the spline <b>in local space</b> at the given progress <c>t</c>
    /// </summary>
    /// <param name="t">progress clamped between <c>[0 and NumSegments]</c></param>
    /// <returns>Rotation on the spline [local space]</returns>
    public Quaternion GetRotation(float t)
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
    public Quaternion GetRotationWorld(float t) => transform.rotation * GetRotation(t);

    /// <summary>
    /// Use custom resolution for the bezier length calculations
    /// </summary> 
    /// <param name="resolution">The resolution for each Bezier</param>
    /// <returns>the length of the spline</returns>
    public float GetLength(int resolution) => GetLength(resolution);

    /// <summary>
    /// Automatically calculates the length of the spline
    /// </summary> 
    /// <returns>Length of the spline in <c>meters</c></returns>
    public float GetLength() => IsStatic ? _length : path.GetLength();

    /// <summary>
    /// Creates a new checkpoint with the current values
    /// </summary>
    public void CreateCheckpoint()
    {
        checkpoints.Add(new PathCheckpoint(Path.Points.ToList(), Path.Rotations.ToList(), Path.IsClosed, Path.Is2D));
    }

    /// <summary>
    /// Deletes the last checkpoint created
    /// </summary>
    /// <returns>success or fail (eg. delete last checkpoint = fail)</returns>
    public bool DeleteCheckpoint()
    {
        if (checkpoints.Count - 1 <= 0)
            return false;
        checkpoints.RemoveAt(checkpoints.Count - 1);
        return true;
    }

    /// <summary>
    /// Deletes the checkpoint at the given index
    /// </summary>
    /// <returns>success or fail (eg. delete last checkpoint = fail)</returns>
    public bool DeleteCheckpoint(int i)
    {
        if (checkpoints.Count - 1 <= 0)
            return false;
        checkpoints.RemoveAt(i);
        return true;
    }

    /// <summary>
    /// Loads the last checkpoint created
    /// </summary>
    public void LoadCheckpoint()
    {
        Path.Points.Clear();
        Path.Points.AddRange(LastCheckPoint.points);

        Path.Rotations.Clear();
        Path.Rotations.AddRange(LastCheckPoint.rotations);

        Path.IsClosed = LastCheckPoint.isClosed;
        Path.Is2D = LastCheckPoint.is2D;
    }

    /// <summary>
    /// Loads the checkpoint at the given index
    /// </summary>
    public void LoadCheckpoint(int i)
    {
        Path.Points.Clear();
        Path.Points.AddRange(checkpoints[i].points);
        
        Path.Rotations.Clear();
        Path.Rotations.AddRange(checkpoints[i].rotations);

        Path.IsClosed = checkpoints[i].isClosed;
        Path.Is2D = checkpoints[i].is2D;
    }

    public override string ToString() => $"path: {Path}";

#if UNITY_EDITOR
    /// <summary>
    /// Editor method
    /// </summary>
    public void RecalculateArrowBuffer()
    {
        bufferedArrowDistribution.Clear();
        for (int i = 0; i < Path.NumSegments; i++)
        {
            bufferedArrowDistribution.Add(Path.GetBezierOfSegment(i).Transform(transform.localToWorldMatrix).EqualDistancePoints(custom.arrowDistance));
        }
    }

#endif
}
