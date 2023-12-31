using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody))]
public class SplineMover3D : MonoBehaviour
{
#if UNITY_EDITOR
    [HideInInspector] public bool showPath = true;
#endif

    private Rigidbody rb;

    public SplineController splineController;

    /// <summary>
    /// Snap out of the splines path when colliding with something
    /// </summary>
    [SerializeField, HideInInspector] private bool snapOutOnCollision = true;

    [SerializeField, HideInInspector] private float progress = 0f;


    float startTime = 0;
    float endTime = 0;
    bool stopWatch = true;

    public float Progress
    {
        get => progress;
        set
        {
            progress = value;
            MoveOnSpline(progress, MoveMode.Transform);
        }
    }

    private bool _move = false;
    private MoveMode _moveMode;
    private float _progressToMoveTo = 0;
    private float _progressPerSecond = 1f;

    private void Start()
    {
        startTime = Time.time;
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rb.useGravity = false;
    }

    private void FixedUpdate()
    {
        if (_move && _moveMode == MoveMode.Physics)
        {
            if (_progressToMoveTo > progress)
            {
                progress += Time.fixedDeltaTime / _progressPerSecond;
                if (progress >= _progressToMoveTo)
                {
                    _move = false;
                    progress = _progressToMoveTo;
                }
            }
            else
            {
                progress -= Time.fixedDeltaTime / _progressPerSecond;
                if (progress <= _progressToMoveTo)
                {
                    _move = false;
                    progress = _progressToMoveTo;
                }
            }
            MoveOnSpline(progress, MoveMode.Physics);
        }
    }

    private void Update()
    {

        // INFO: Test
        // ------------------------------------------
        if (progress == 1 && stopWatch)
        {
            endTime = Time.time;
            Debug.Log(endTime - startTime);
            stopWatch = false;
        }

        if (Input.GetKeyDown(KeyCode.P))
            MoveToProgress(2, MoveMode.Physics, 5);
        if (Input.GetKeyDown(KeyCode.S))
            MoveToSegment(2, MoveMode.Physics, 2);
        if (Input.GetKeyDown(KeyCode.R))
            MoveToSegment(0, MoveMode.Physics, 0);
        // ------------------------------------------

        if (_move && _moveMode == MoveMode.Transform)
        {
            if (_progressToMoveTo > progress)
            {
                progress += Time.deltaTime / _progressPerSecond;
                if (progress >= _progressToMoveTo)
                {
                    _move = false;
                    progress = _progressToMoveTo;
                }
            }
            else
            {
                progress -= Time.deltaTime / _progressPerSecond;
                if (progress <= _progressToMoveTo)
                {
                    _move = false;
                    progress = _progressToMoveTo;
                }
            }
            MoveOnSpline(progress, MoveMode.Transform);
        }
    }

    /// <summary>
    /// Moves the transform to the given value on the spline
    /// </summary>
    /// <param name="value">progress on the spline</param>
    public void MoveOnSpline(float value, MoveMode mode)
    {
        value = splineController.Path.IsClosed ? IndexHelper.LoopIndex(value, splineController.Path.NumSegments) : value;

        Vector3 pos = splineController.CalculatePositionWorld(value);
        Quaternion rot = splineController.CalculateRotationWorld(value);

        switch (mode)
        {
            case MoveMode.Physics:
                rb.MovePosition(pos);
                rb.MoveRotation(rot);
                break;
            case MoveMode.Transform:
                transform.SetPositionAndRotation(pos, rot);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
        progress = value;
    }

    /// <summary>
    /// Moves the transform to the next segment
    /// </summary>
    /// <param name="time">time it takes to move to the next segment</param>
    public void MoveToNextSegment(float time, MoveMode mode)
    {
        int nextSegment = Mathf.FloorToInt(Mathf.Clamp(progress + 1, 0, splineController.Path.NumSegments));
        if (nextSegment == progress)
            return;

        time = Mathf.Max(time, 0);

        _progressPerSecond = time / Mathf.Abs(progress - nextSegment);
        _move = true;
        _progressToMoveTo = nextSegment;
        _moveMode = mode;
    }

    /// <summary>
    /// Moves transform to segment
    /// </summary>
    /// <param name="segment">segment to move to</param>
    /// <param name="time">time it takes to move to the segment</param>
    public void MoveToSegment(int segment, MoveMode mode, float time)
    {
        segment = Mathf.Clamp(segment, 0, splineController.Path.NumSegments);
        if (segment == progress)
            return;

        time = Mathf.Max(time, 0);

        _progressPerSecond = time / Mathf.Abs(progress - segment);
        _move = true;
        _progressToMoveTo = segment;
        _moveMode = mode;
    }

    /// <summary>
    /// Moves the gameObject to the given progress t
    /// </summary>
    /// <param name="t">progress clamped between <c>[0 and NumSegments]</c></param>
    /// <param name="time">time it takes to move to t</param>
    public void MoveToProgress(float t, MoveMode mode, float time)
    {
        t = Mathf.Clamp(t, 0, splineController.Path.NumSegments);
        if (t == progress)
            return;

        time = Mathf.Max(time, 0);

        _progressPerSecond = time / Mathf.Abs(progress - t);
        _move = true;
        _progressToMoveTo = t;
        _moveMode = mode;
    }

    /// <summary>
    /// Moves the gameObject on the spline with the given velocity
    /// </summary>
    /// <param name="velocity">velocity in <c>units/s</c></param>
    public void MoveWithVelocity(float velocity, MoveMode mode)
    {
        float delta = mode switch
        {
            MoveMode.Transform => Time.deltaTime,
            MoveMode.Physics => Time.fixedDeltaTime,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };


        int currentSegment = Mathf.FloorToInt(progress) + (progress == splineController.Path.NumSegments ? -1 : 0);
        CubicBezier bezier = splineController.Path.GetBezierOfSegment(currentSegment);
        float segmentT = progress - currentSegment;
        float t = bezier.PointFromDistance(segmentT, velocity * delta) + currentSegment;
        MoveOnSpline(t, mode);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(-rb.velocity.magnitude);
    }
}
