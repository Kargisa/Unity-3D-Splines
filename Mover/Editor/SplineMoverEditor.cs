using log4net.Util;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Splines;

[CustomEditor(typeof(SplineMover))]
public class SplineMoverEditor : Editor
{
    SplineMover _controller;
    SplineController _spline;
    Path _path;

    private void OnEnable()
    {
        try
        {
            _controller = (SplineMover)target;
            _spline = _controller.splineController;
            _path = _spline.path;

        }
        catch (System.Exception)
        {
            throw new System.Exception("CameraEditor 'initialization Error' some props were not correctly initialized");
        }
    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!NotNullProps())
            return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Path", EditorStyles.boldLabel);
        PaintSlider(ref _controller.progess, 0, _spline.path.NumSegments, "Progress");
        PathButton();

    }

    private void PaintSlider(ref float value, float min, float max, string label)
    {
        float prevValue = value;
        value = EditorGUILayout.Slider(label, value, min, max);
        if (prevValue != value)
            _controller.MoveTransformOnSpline(value);
    }

    private void PathButton()
    {
        if (GUILayout.Button("Toggle Path"))
        {
            _controller.showPath = !_controller.showPath;
            SceneView.RepaintAll();
        }
    }

    private bool NotNullProps()
    {
        if (_spline == null && _controller.splineController != null)
        {
            _spline = _controller.splineController;
            _path = _spline.path;
        }
        else if (_spline == null)
        {
            Debug.LogError("SplineController is null");
            return false;
        }

        return true;
    }

    private void OnSceneGUI()
    {
        if (_spline == null)
            return;

        Matrix4x4 oldMatrix = Handles.matrix;
        Handles.matrix = _spline.transform.localToWorldMatrix;

        _controller.MoveTransformOnSpline(_controller.progess);
        Draw();

        Handles.matrix = oldMatrix;
    }

    private void Draw()
    {
        if (!_controller.showPath)
            return;

        for (int i = 0; i < _path.NumSegments; i++)
        {
            Vector3[] points = _path.GetPointsInSegment(i);

            Handles.DrawBezier(points[0], points[3], points[1], points[2], _spline.custom.splineColor, null, 2);
        }
        DrawRotation();
    }
    private void DrawRotation()
    {
        float arrowsDistr = _spline.custom.arrowDistribution;
        Handles.color = _spline.custom.arrowColor;
        for (int j = 0; j < _path.NumSegments; j++)
        {
            for (int i = 0; i <= arrowsDistr; i++)
            {
                Quaternion rot = _spline.CalculateRotation(j + i / arrowsDistr);
                Vector3 pos = _spline.CalculatePosition(j + i / arrowsDistr);
                Handles.ArrowHandleCap(i, pos, rot, _spline.custom.arrowLength, EventType.Repaint);
            }
        }

    }

}
