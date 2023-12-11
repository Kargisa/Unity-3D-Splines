using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SplineMover3D))]
public class SplineMoverEditor3D : Editor
{
    SplineMover3D _mover;
    SplineController _spline;
    Path _path;

    private void OnEnable()
    {
        try
        {
            _mover = (SplineMover3D)target;
            _spline = _mover.splineController;
            _path = _spline.path;

        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
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
        DrawProgress(_mover.Progress, _spline.path.NumSegments, "Progress");
        PathButton();

    }

    /// <summary>
    /// Draws the progress slider
    /// </summary>
    private void DrawProgress(float value, float max, string label)
    {
        float prevValue = value;
        if (_path.IsClosed)
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
    private bool NotNullProps()
    {
        if (_spline == null && _mover.splineController != null)
        {
            _spline = _mover.splineController;
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

        //Save the old matrix
        Matrix4x4 oldMatrix = Handles.matrix;
        //Set Handles.matrix to the localToWorldMatrix to enable drawing the spline in relation to the spline object
        Handles.matrix = _spline.transform.localToWorldMatrix;

        if (!Application.isPlaying)
            _mover.MoveOnSpline(_mover.Progress, MoveMode.Transform);

        Draw();

        //Reinstantiate the old matrix
        Handles.matrix = oldMatrix;
    }

    private void Draw()
    {
        if (!_mover.showPath)
            return;

        for (int i = 0; i < _path.NumSegments; i++)
        {
            CubicBezier bezier = _path.GetBezierOfSegment(i);

            Handles.DrawBezier(bezier.p1, bezier.p4, bezier.p2, bezier.p3, _spline.custom.splineColor, null, 2);
        }
        DrawRotation();
    }
    private void DrawRotation()
    {
        Handles.color = _spline.custom.arrowColor;

        float arrowsDistribution = _spline.custom.arrowDistribution;
        for (int j = 0; j < _path.NumSegments; j++)
        {
            for (int i = 0; i < arrowsDistribution; i++)
            {
                Quaternion rot = _spline.CalculateRotation(j + i / arrowsDistribution);
                Vector3 pos = _spline.CalculatePosition(j + i / arrowsDistribution);
                Handles.ArrowHandleCap(i, pos, rot, _spline.custom.arrowLength, EventType.Repaint);
            }
        }

    }

}
