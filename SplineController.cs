using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem.UI;


#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

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
    private bool isStatic = true;

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

    public Path Path => path;

    [SerializeField, HideInInspector]
    private float _length;

    public float Length => GetLength();

    public Matrix4x4 localWorldMatrix
    {
        get
        {
            Matrix4x4 matrix = new();
            matrix.SetTRS(transform.position, transform.rotation, Vector3.one);
            return matrix;
        }
    }

    public Matrix4x4 worldLocalMatrix
    {
        get
        {
            Matrix4x4 matrix = new();
            matrix.SetTRS(transform.position, transform.worldToLocalMatrix.rotation, Vector3.one);
            return matrix;
        }
    }

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
    public Vector3 GetPositionWorld(float t) => TransformPoint(GetPosition(t));

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
    public float GetLength(int resolution) => Path.GetLength(resolution);

#if UNITY_EDITOR
    /// <summary>
    /// Automatically calculates the length of the spline
    /// </summary> 
    /// <returns>Length of the spline in <c>meters</c></returns>
    public float GetLength() => IsStatic && Application.isPlaying ? _length : Path.GetLength();
#else
    /// <summary>
    /// Automatically calculates the length of the spline
    /// </summary> 
    /// <returns>Length of the spline in <c>meters</c></returns>
    public float GetLength() => IsStatic ? _length : Path.GetLength();
#endif

    public Vector3 TransformPoint(Vector3 point)
    {
        return localWorldMatrix.MultiplyPoint(point);
    }

    public Vector3 InvTransformPoint(Vector3 point)
    {
        return worldLocalMatrix.MultiplyPoint(point);
    }

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

    private readonly object _bufferLock = new();

    private readonly List<CancellationTokenSource> _tokenSources = new();

    /// <summary>
    /// Editor method
    /// </summary>
    public void RecalculateArrowBuffer()
    {
        CancellationTokenSource source = new();
        CancellationToken token = source.Token;

        Matrix4x4 matrix = localWorldMatrix;
        var task = Task.Factory.StartNew((state) =>
        {
            foreach (var tSource in _tokenSources)
            {
                if (tSource.Token == token)
                    continue;
                tSource.Cancel();
            }
            _tokenSources.Clear();
            _tokenSources.Add(source);

            if (token.IsCancellationRequested) return;
            lock (_bufferLock)
            {
                if (token.IsCancellationRequested) return;
                List<float[]> current = new();


                for (int i = 0; i < Path.NumSegments; i++)
                {
                    if (token.IsCancellationRequested) return;
                    current.Add(Path.GetBezierOfSegment(i).Transform(matrix).EqualDistancePoints(custom.arrowDistance));
                }

                if (token.IsCancellationRequested) return;
                bufferedArrowDistribution = current;
            }
        }, source.Token);
    }

    /// <summary>
    /// Editor fieldh
    /// </summary>
    [HideInInspector]
    public bool baking = false;

    private readonly object _bakeLock = new();

    private void BakeLength()
    {
        if (!IsStatic) return;
        baking = true;

        Matrix4x4 lTwM = localWorldMatrix;
        Task.Factory.StartNew((state) =>
        {
            try
            {
                Path path = (Path)state;
                int numSegs = path.NumSegments;
                int[] reses = new int[numSegs];
                for (int i = 0; i < numSegs; i++)
                {
                    reses[i] = Mathf.CeilToInt(path.GetBezierOfSegment(i).Length) * 10000;
                }

                lock (_bakeLock)
                {
                    _length = path.GetLength(reses);
                }

                baking = false;
                Debug.Log($"Baking finished with a length of {_length} units!");
            }
            catch (Exception)
            {
                baking = false;
                Debug.LogError("Baking failed");
            }
        }, Path);
    }

    public void EditorSceneManager_sceneSaving(UnityEngine.SceneManagement.Scene scene, string path)
    {
        BakeLength();
    }
#endif
}
