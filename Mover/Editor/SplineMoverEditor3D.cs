using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SplineMover3D))]
public class SplineMoverEditor3D : Editor
{
    SplineMover3D _mover;

    private void OnEnable()
    {
        _mover = (SplineMover3D)target;
    }

    public override void OnInspectorGUI()
    {
        
        base.OnInspectorGUI();
        serializedObject.ApplyModifiedProperties();

        if (NullProps())
            return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Path", EditorStyles.boldLabel);
        
        DrawProgress(_mover.Progress, _mover.splineController.Path.NumSegments, "Progress");
        PathButton();
    }

    /// <summary>
    /// Draws the progress slider
    /// </summary>
    private void DrawProgress(float value, float max, string label)
    {
        float prevValue = value;
        if (_mover.splineController.Path.IsClosed)
        {
            value = IndexHelper.LoopIndex(value, max);
            prevValue = value;
            value = EditorGUILayout.FloatField(label, value);
        }
        else
        {
            value = EditorGUILayout.Slider(label, value, 0, max);
        }

        if (prevValue != value)
            _mover.MoveOnSpline(value, MoveMode.Transform);
    }

    /// <summary>
    /// Draws the button that toggles the path
    /// </summary>
    private void PathButton()
    {
        if (GUILayout.Button("Toggle Path"))
        {
            _mover.showPath = !_mover.showPath;
            SceneView.RepaintAll();
        }
    }

    /// <summary>
    /// Checks if the splineMover has all the required props set
    /// </summary>
    private bool NullProps()
    {
        if (_mover.splineController == null)
        {
            Debug.LogWarning("no spline controller attached");
            return true;
        }

        return false;
    }

    private void OnSceneGUI()
    {
        if (_mover.splineController == null)
            return;

        if (!Application.isPlaying)
            _mover.MoveOnSpline(_mover.Progress, MoveMode.Transform);

        Paint();
    }

    private void Paint()
    {
        if (!_mover.showPath)
            return;

        for (int i = 0; i < _mover.splineController.Path.NumSegments; i++)
        {
            CubicBezier bezier = _mover.splineController.Path.GetBezierOfSegment(i).Transform(_mover.splineController.transform.localToWorldMatrix);

            Handles.DrawBezier(bezier.p1, bezier.p4, bezier.p2, bezier.p3, _mover.splineController.custom.splineColor, null, 2);
        }
        PaintArrows();
    }

    private void PaintArrows()
    {
        if (_mover.splineController.bufferedArrowDistribution.Count == 0)
            RecalculateArrowBuffer();

        Handles.color = _mover.splineController.custom.arrowColor;

        if (_mover.splineController.custom.useArrowDistanceDistribution)
        {
            for (int i = 0; i < _mover.splineController.bufferedArrowDistribution.Count; i++)
            {
                float[] p = _mover.splineController.bufferedArrowDistribution[i];
                for (int j = 0; j < p.Length; j++)
                {
                    Quaternion rot = _mover.splineController.CalculateRotationWorld(i + p[j]);
                    Vector3 pos = _mover.splineController.CalculatePositionWorld(i + p[j]);
                    Handles.ArrowHandleCap(i, pos, rot, _mover.splineController.custom.arrowLength, EventType.Repaint);
                }
            }
            return;
        }
        float arrowsDistribution = _mover.splineController.custom.arrowDistribution;
        for (int j = 0; j < _mover.splineController.Path.NumSegments; j++)
        {
            for (int i = 0; i < arrowsDistribution; i++)
            {
                Quaternion rot = _mover.splineController.CalculateRotationWorld(j + i / arrowsDistribution);
                Vector3 pos = _mover.splineController.CalculatePositionWorld(j + i / arrowsDistribution);
                Handles.ArrowHandleCap(i, pos, rot, _mover.splineController.custom.arrowLength, EventType.Repaint);
            }
        }

    }

    private void RecalculateArrowBuffer()
    {
        SplineController spline = _mover.splineController;
        if (!spline.custom.alwaysShowArrows || !spline.isRotate)
            return;

        spline.bufferedArrowDistribution.Clear();
        for (int i = 0; i < spline.Path.NumSegments; i++)
        {
            spline.bufferedArrowDistribution.Add(spline.Path.GetBezierOfSegment(i).EqualDistancePoints(spline.custom.arrowDistance));
        }
    }

}
