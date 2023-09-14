using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    [HideInInspector]
    public List<PathCheckpoint> checkpoints;

    [SerializeField, HideInInspector]
    private List<float[]> distancesT;


    public Path(Vector3 center)
    {
        checkpoints = new List<PathCheckpoint>();
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

        distancesT = new List<float[]>() { new float[0] };

        Is2D = true;

        CreateCheckpoint();
    }

    public Vector3 this[int i]
    {
        get => points[i];
        set { points[i] = value; }
    }

    public int NumPoints { get => points.Count; }

    public int NumSegments { get => points.Count / 3; }

    public int NumRotations { get => rotations.Count; }

    public int NumDistancesT { get => distancesT.Count; }

    public bool IsNull { get => points == null; }

    public PathCheckpoint LastCheckPoint { get => checkpoints[^1]; }

    public bool Is2D
    {
        get { return is2D; }
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
        get { return isClosed; }
        set { isClosed = value; }
    }

    public Quaternion GetRotation(int i) => rotations[LoopRotationIndex(i)];

    public float[] GetDistancesT(int i) => distancesT[LoopDistancesTIndex(i)];

    public List<float[]> RecalculateDistancesT(float distance, int resolution)
    {
        for (int i = 0; i < NumSegments; i++)
        {
            Vector3[] segPoints = GetPointsInSegment(i);
            distancesT[i] = Bezier.GetEqualDistancesT(segPoints[0], segPoints[1], segPoints[2], segPoints[3], distance, resolution);
        }
        return distancesT;
    }

    /// <summary>
    /// Adds a new Segment at the point
    /// </summary>
    /// <param name="point">Position of the Segment</param>
    public void AddSegment(Vector3 point, Quaternion rotation)
    {
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
        distancesT.Add(new float[0]);
    }

    /// <summary>
    /// Adds a new Segement with an offset of half the distance of the last Segment
    /// </summary>
    public void AddSegment()
    {
        Vector3 newPoint = points[^1] - ((points[^1] + points[^4]) * 0.5f) + points[^1];
        AddSegment(newPoint, Quaternion.identity);
    }

    public void InsertSegment(int i, Vector3 pos)
    {
        int index = i * 3 + 2;
        points.Insert(index, pos);
        points.Insert(index, pos);
        points.Insert(index, pos);

        rotations.Insert(i, Quaternion.identity);
        distancesT.Insert(i, new float[0]);
    }

    public void DeletePoint(int i)
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

        rotations.RemoveAt(i/3);
    }

    public void LoadLastCheckpoint()
    {
        points = LastCheckPoint.points.ToList();
        rotations = LastCheckPoint.rotations.ToList();
        isClosed = LastCheckPoint.isClosed;
        is2D = LastCheckPoint.is2D;
    }

    public void CreateCheckpoint()
    {
        checkpoints.Add(new PathCheckpoint(points.ToList(), rotations.ToList(), isClosed, is2D));
    }

    public bool DeleteCheckpoint()
    {
        if (checkpoints.Count - 1 <= 0)
            return false;
        checkpoints.RemoveAt(checkpoints.Count - 1);
        return true;
    }

    public Vector3[] GetPointsInSegment(int index) => new Vector3[4] { points[index * 3], points[index * 3 + 1], points[index * 3 + 2], points[LoopIndex(index * 3 + 3)] };

    public void ToggleClosed()
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
    }

    private int LoopIndex(int i) => IndexHelper.LoopIndex(i, points.Count);

    private int LoopRotationIndex(int i) => IndexHelper.LoopIndex(i, rotations.Count);

    private int LoopDistancesTIndex(int i) => IndexHelper.LoopIndex(i, distancesT.Count);

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
                points[LoopIndex(i + 1)] += deltaMove;
            if (i - 1 >= 0 || isClosed)
                points[LoopIndex(i - 1)] += deltaMove;
        }
        else
        {
            bool nextPointIsAnchor = (i + 1) % 3 == 0;
            int correspondingControlIndex = nextPointIsAnchor ? i + 2 : i - 2;
            int anchorIndex = nextPointIsAnchor ? i + 1 : i - 1;
            if ((correspondingControlIndex >= 0 && correspondingControlIndex < points.Count) || isClosed)
            {
                float dist = (points[LoopIndex(anchorIndex)] - pos).magnitude;
                Vector3 dir = (points[LoopIndex(anchorIndex)] - pos).normalized;

                points[LoopIndex(correspondingControlIndex)] = (Is2D ? (Vector2)(points[LoopIndex(anchorIndex)]) : points[LoopIndex(anchorIndex)]) + dir * dist;
            }
        }
    }

    public void RotatePoint(int i, Quaternion rotation)
    {
        rotations[i] = rotation;
    }

    public override string ToString()
    {
        return $"points: {NumPoints}, segments: {NumSegments}, rotations {NumRotations}, checkpoints: {checkpoints.Count}";
    }
}
