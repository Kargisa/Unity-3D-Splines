using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.HableCurve;

public class SplineMover : MonoBehaviour
{
#if UNITY_EDITOR
    [HideInInspector] public bool showPath = true;
#endif

    [SerializeField] private Transform transToMove;
    public SplineController splineController;
    
    [HideInInspector] public float progess = 0f;
    
    private float _progressToMoveTo = 0;
    private bool _moveTransform = false;
    private float _moveTime = 1f;

    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            MoveToNextSegment(2);

        MoveWithVelocity(0.5f);

        if (_moveTransform)
        {
            if (_progressToMoveTo > progess)
            {
                progess += Time.deltaTime / _moveTime;
                if (progess >= _progressToMoveTo)
                    _moveTransform = false;
            }
            else
            {
                progess -= Time.deltaTime / _moveTime;
                if (progess <= _progressToMoveTo)
                    _moveTransform = false;
            }
            MoveTransformOnSpline(_progressToMoveTo);
        }
    }

    /// <summary>
    /// Moves the transform to the given value on the spline
    /// </summary>
    /// <param name="value">progress on the spline</param>
    public void MoveTransformOnSpline(float value)
    {
        value = Mathf.Clamp(value, 0, splineController.path.NumSegments);

        Vector3 pos = splineController.CalculatePositionWorld(value);
        Quaternion rot = splineController.CalculateRotationWorld(value);

        transToMove.SetPositionAndRotation(pos, rot);
        progess = splineController.path.IsClosed ? IndexHelper.LoopIndex(value, splineController.path.NumSegments) : value;
    }

    /// <summary>
    /// Moves the transform to the next segment
    /// </summary>
    /// <param name="time">time it takes to move to the next segment</param>
    public void MoveToNextSegment(float time = 1f)
    {
        int nextSegment = (int)Mathf.Clamp(progess + 1, 0, splineController.path.NumSegments);
        if (nextSegment == progess)
            return;
        
        time = time < 0 ? 0 : time;

        _moveTime = time / Mathf.Abs(progess - nextSegment);
        _moveTransform = true;
        _progressToMoveTo = nextSegment;
    }

    /// <summary>
    /// Moves transform to segment
    /// </summary>
    /// <param name="segment">segment to move to</param>
    /// <param name="time">time it takes to move to the segment</param>
    public void MoveToSegment(int segment, float time = 1f)
    {
        segment = Mathf.Clamp(segment, 0, splineController.path.NumSegments);
        if (segment == progess)
            return;

        time = time < 0 ? 0 : time;

        _moveTime = time / Mathf.Abs(progess - segment);
        _moveTransform = true;
        _progressToMoveTo = segment;
    }

    /// <summary>
    /// Moves the transform to the given progress t
    /// </summary>
    /// <param name="t">progress clamped between <c>[0 and NumSegments]</c></param>
    /// <param name="time">time it takes to move to t</param>
    public void MoveToProgress(float t, float time = 1f)
    {
        t = Mathf.Clamp(t, 0, splineController.path.NumSegments);
        if (t == progess)
            return;

        time = time < 0 ? 0 : time;

        _moveTime = time / Mathf.Abs(progess - t);
        _moveTransform = true;
        _progressToMoveTo = t;
    }

    /// <summary>
    /// Moves the transform on the spline with a certain velocity
    /// </summary>
    /// <param name="velocity">velocity in <c>units/s</c></param>
    public void MoveWithVelocity(float velocity, MoveMode mode = MoveMode.Transform)
    {
        float delta = mode switch
        {
            MoveMode.Transform => Time.deltaTime,
            MoveMode.Physics => Time.fixedDeltaTime,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };

        int currentSegment = Mathf.FloorToInt(progess) + (progess == splineController.path.NumSegments ? -1 : 0);
        Vector3[] points = splineController.path.GetPointsInSegment(currentSegment);
        float segmentT = progess - currentSegment;

        float t = Bezier.GetTFromDistance(points[0], points[1], points[2], points[3], 1000, segmentT, velocity * delta) + currentSegment;
        MoveTransformOnSpline(t);
    }
}
