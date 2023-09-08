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

        if (_moveTransform)
        {
            if (_progressToMoveTo > progess)
            {
                progess += Time.deltaTime / _moveTime;
                if (progess >= _progressToMoveTo)
                {
                    _moveTransform = false;
                    progess = _progressToMoveTo;
                }
            }
            else
            {
                progess -= Time.deltaTime / _moveTime;
                if (progess <= _progressToMoveTo)
                {
                    _moveTransform = false;
                    progess = _progressToMoveTo;
                }
            }
            MoveTransformOnSpline(progess);
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
}
