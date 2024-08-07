using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// Defines a path for a spline in <b>local space</b>
/// </summary>
[System.Serializable]
public class Path
{
    [SerializeField, HideInInspector]
    private List<Vector3> points;

    [SerializeField, HideInInspector]
    private List<Quaternion> rotations;

    [SerializeField, HideInInspector]
    private bool is2D;

    [SerializeField, HideInInspector]
    private bool isClosed;

    public Path(Vector3 center)
    {
        points = new List<Vector3>() {
            center + Vector3.left,
            center + (Vector3.left + Vector3.up) * 0.5f,
            center + (Vector3.right + Vector3.down) * 0.5f,
            center + Vector3.right,
        };

        rotations = new List<Quaternion>() {
            Quaternion.identity,
            Quaternion.identity
        };

        Is2D = true;
    }

    /// <returns>Point in local space at index <c>i</c></returns>
    public Vector3 this[int i]
    {
        get => points[i];
        set { points[i] = value; }
    }

    public List<Vector3> Points => points;

    public List<Quaternion> Rotations => rotations;

    public float Length { get => GetLength(); }

    public int NumPoints { get => points.Count; }

    public int NumSegments { get => points.Count / 3; }

    public int NumRotations { get => rotations.Count; }

    /// <summary>
    /// Checks if points is null
    /// </summary>
    public bool IsNull { get => points == null; }

    public bool Is2D
    {
        get => is2D;
        set
        {
            if (value)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    points[i] = (Vector2)points[i];
                }
            }
            is2D = value;
        }
    }

    public bool IsClosed
    {
        get => isClosed;
        set => isClosed = value;
    }

    /// <summary>
    /// Gets the quaternion at index i
    /// </summary>
    /// <param name="i">Index of the quaternion</param>
    public Quaternion GetRotation(int i) => rotations[LoopRotationsIndex(i)];

    /// <summary>
    /// Adds a new segment at the point
    /// </summary>
    /// <param name="point">Position of the segment</param>
    /// <param name="rotation">Rotation of the segment</param>
    public void AddSegment(Vector3 point, Quaternion rotation)
    {
        if (IsClosed)
        {
            InsertSegment(NumSegments - 1, point, rotation);
            return;
        }

        if (Is2D)
        {
            points.Add((Vector2)(points[^1] * 2 - points[^2]));
            points.Add((Vector2)((points[^1] + point) * 0.5f));
            points.Add((Vector2)point);
        }
        else
        {
            points.Add(points[^1] * 2 - points[^2]);
            points.Add((points[^1] + point) * 0.5f);
            points.Add(point);
        }
        rotations.Add(rotation);
    }

    /// <summary>
    /// Adds a new segement with an offset of half the distance to the last Segment
    /// </summary>
    public void AddSegment()
    {
        Vector3 newPoint;
        Quaternion newQuad;

        if (!IsClosed)
        {
            newPoint = points[^1] - ((points[^1] + points[^4]) * 0.5f) + points[^1];
            newQuad = Quaternion.identity;
        }
        else
        {
            newPoint = (points[^2] + points[0]) / 2;
            newQuad = Quaternion.Slerp(rotations[^1], rotations[0], 0.5f);
        }

        AddSegment(newPoint, newQuad);
    }

    /// <summary>
    /// Insert a new segment at the index i with a given position
    /// </summary>
    /// <param name="i">Index of the new segment</param>
    /// <param name="pos">Position of the new anchor of the segment</param>
    public void InsertSegment(int i, Vector3 pos) => InsertSegment(i, pos, Quaternion.Slerp(rotations[i], rotations[LoopRotationsIndex(i + 1)], 0.5f));

    /// <summary>
    /// Insert a new segment at the index i with a given position and rotation
    /// </summary>
    /// <param name="i">Index of the new segment</param>
    /// <param name="pos">Position of the new anchor of the segment</param>
    /// <param name="quad">Rotation of the new anchor of the segment</param>
    public void InsertSegment(int i, Vector3 pos, Quaternion quad)
    {
        int index = i * 3 + 2;
        points.Insert(index, pos);
        points.Insert(index, pos);
        points.Insert(index, pos);

        rotations.Insert(i + 1, quad);

    }

    /// <summary>
    /// Deletes the segment at index i
    /// </summary>
    /// <param name="i">Index of the segment to delete</param>
    public void DeleteSegment(int i)
    {
        if (i % 3 != 0)
            return;
        if (NumSegments < 2 || (isClosed && NumSegments <= 2))
            return;

        if (i == 0)
        {
            if (isClosed)
            {
                points[^1] = points[2];
            }
            points.RemoveRange(0, 3);
        }
        else if (i == points.Count - 1 && !isClosed)
            points.RemoveRange(i - 2, 3);
        else
            points.RemoveRange(i - 1, 3);

        rotations.RemoveAt(i / 3);
    }

    /// <summary>
    /// Gets the cubic bezier of the segment at the index i
    /// </summary>
    /// <param name="index">index of the segment</param>
    public CubicBezier GetBezierOfSegment(int index) => new(points[index * 3], points[index * 3 + 1], points[index * 3 + 2], points[LoopPointsIndex(index * 3 + 3)], rotations[index], rotations[LoopRotationsIndex(index + 1)]);

    /// <summary>
    /// Toggles path open and closed
    /// </summary>
    public bool ToggleClosed()
    {
        if (isClosed)
        {
            points.RemoveRange(points.Count - 2, 2);
        }
        else
        {
            if (is2D)
            {
                points.Add((Vector2)(points[^1] * 2 - points[^2]));
                points.Add((Vector2)(points[0] * 2 - points[1]));
            }
            else
            {
                points.Add(points[^1] * 2 - points[^2]);
                points.Add(points[0] * 2 - points[1]);
            }
        }

        isClosed = !isClosed;
        return isClosed;
    }

    private int LoopPointsIndex(int i) => IndexHelper.LoopIndex(i, points.Count);

    private int LoopRotationsIndex(int i) => IndexHelper.LoopIndex(i, rotations.Count);

    /// <summary>
    /// Moves the point at index i to the position pos
    /// </summary>
    /// <param name="i">Index of the point</param>
    /// <param name="pos">Position to move the point to</param>
    public void MovePoint(int i, Vector3 pos)
    {
        Vector3 deltaMove = pos - points[i];
        if (Is2D)
        {
            deltaMove = (Vector2)deltaMove;
            pos = (Vector2)pos;
        }

        points[i] = pos;
        if (i % 3 == 0)
        {
            if (i + 1 < points.Count || isClosed)
                points[LoopPointsIndex(i + 1)] += deltaMove;
            if (i - 1 >= 0 || isClosed)
                points[LoopPointsIndex(i - 1)] += deltaMove;
        }
        else
        {
            bool nextPointIsAnchor = (i + 1) % 3 == 0;
            int correspondingControlIndex = nextPointIsAnchor ? i + 2 : i - 2;
            int anchorIndex = nextPointIsAnchor ? i + 1 : i - 1;
            if ((correspondingControlIndex >= 0 && correspondingControlIndex < points.Count) || isClosed)
            {
                float dist = (points[LoopPointsIndex(anchorIndex)] - pos).magnitude;
                Vector3 dir = (points[LoopPointsIndex(anchorIndex)] - pos).normalized;

                points[LoopPointsIndex(correspondingControlIndex)] = (Is2D ? (Vector2)(points[LoopPointsIndex(anchorIndex)]) : points[LoopPointsIndex(anchorIndex)]) + dir * dist;
            }
        }
    }

    /// <summary>
    /// Rotates the point at index i to the Quaternion rotation
    /// </summary>
    /// <param name="i">Index of the point</param>
    /// <param name="rotation">Quaternion to rotate to</param>
    public void RotatePoint(int i, Quaternion rotation)
    {
        rotations[i] = rotation;
    }

    /// <returns>Length of the path</returns>
    public float GetLength()
    {
        float length = 0;
        for (int i = 0; i < NumSegments; i++)
        {
            CubicBezier b = GetBezierOfSegment(i);
            length += b.Length;
        }

        return length;
    }

    /// <returns>Length of the path</returns>
    public float GetLength(Matrix4x4 transform)
    {
        float length = 0;
        for (int i = 0; i < NumSegments; i++)
        {
            CubicBezier b = GetBezierOfSegment(i).Transform(transform);
            length += b.Length;
        }

        return length;
    }

    /// <summary>
    /// Gets the length of the path with a custom resolution
    /// </summary>
    /// <param name="resolutions">resolutions for the beziers</param>
    /// <returns>length of the spline</returns>
    public float GetLength(params int[] resolutions)
    {
        if (resolutions.Length != NumSegments)
            throw new System.ArgumentException("resolutions must have the same length as the number of segments in the spline");

        float length = 0;
        for (int i = 0; i < NumSegments; i++)
        {
            CubicBezier b = GetBezierOfSegment(i);
            length += b.GetEstimatedLength(resolutions[i]);
        }
        return length;
    }

    /// <summary>
    /// Gets the length of the path with a custom resolution
    /// </summary>
    /// <param name="resolutions">resolutions for the beziers</param>
    /// <returns>length of the spline</returns>
    public float GetLength(Matrix4x4 transform, params int[] resolutions)
    {
        if (resolutions.Length != NumSegments)
            throw new System.ArgumentException("resolutions must have the same length as the number of segments in the spline");

        float length = 0;
        for (int i = 0; i < NumSegments; i++)
        {
            CubicBezier b = GetBezierOfSegment(i).Transform(transform);
            length += b.GetEstimatedLength(resolutions[i]);
        }
        return length;
    }

    /// <summary>
    /// Calculates a transformation for the path
    /// </summary>
    /// <param name="transformation">The tarnsformation matrix</param>
    /// <returns>Information about the transformed path</returns>
    public PathInfo TransformInfo(Matrix4x4 transformation)
    {
        Vector3[] points = new Vector3[NumPoints];
        Quaternion[] rotations = new Quaternion[NumRotations];
        //PathCheckpoint[] checkpoints = new PathCheckpoint[Checkpoints.Count];

        Quaternion matrixQud = transformation.rotation;

        for (int i = 0; i < points.Length; i++)
        {
            points[i] = transformation.MultiplyPoint(this[i]);
        }

        for (int i = 0; i < rotations.Length; i++)
        {
            rotations[i] = matrixQud * GetRotation(i);
        }

        return new PathInfo(points, rotations);

    }

    public override string ToString()
    {
        return $"points: {NumPoints}, segments: {NumSegments}, rotations: {NumRotations}";
    }
}

public readonly struct PathInfo
{
    public readonly Vector3[] points;
    public readonly Quaternion[] rotations;

    public PathInfo(Vector3[] points, Quaternion[] rotations)
    {
        this.points = points;
        this.rotations = rotations;
    }
}
