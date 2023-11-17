using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SplineMover))]
public class SplineMoverEditor : Editor
{
    SplineMover _mover;
    SplineController _spline;
    Path _path;

    private void OnEnable()
    {
        try
        {
            _mover = (SplineMover)target;
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
        PaintProgress(ref _mover.progess, _spline.path.NumSegments, "Progress");
        PathButton();
         
    }

    private void PaintProgress(ref float value, float max, string label)
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
            _mover.MoveTransformOnSpline(value);
    }

    private void PathButton()
    {
        if (GUILayout.Button("Toggle Path"))
        {
            _mover.showPath = !_mover.showPath;
            SceneView.RepaintAll();
        }
    }

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

        Matrix4x4 oldMatrix = Handles.matrix;
        Handles.matrix = _spline.transform.localToWorldMatrix;

        _mover.MoveTransformOnSpline(_mover.progess);
        Draw();

        Handles.matrix = oldMatrix;
    }

    private void Draw()
    {
        if (!_mover.showPath)
            return;

        for (int i = 0; i < _path.NumSegments; i++)
        {
            CubicBezier bezier = _path.GetBezierOfSegment(i);

            Handles.DrawBezier(bezier.p1, bezier.p2, bezier.p3, bezier.p4, _spline.custom.splineColor, null, 2);
        }
        DrawRotation();
    }
    private void DrawRotation()
    {
        Handles.color = _spline.custom.arrowColor;
        if (_spline.custom.arrowDistributionByDistance)
        {
            //TODO: arrwos by distance

            //for (int i = 0; i < _path.NumDistancesT; i++)
            //{
            //    float[] distancesT = _path.GetDistancesT(i);
            //    for (int j = 0; j < distancesT.Length; j++)
            //    {
            //        Quaternion rot = _spline.CalculateRotation(i + distancesT[j]);
            //        Vector3 pos = _spline.CalculatePosition(i + distancesT[j]);
            //        Handles.ArrowHandleCap(i, pos, rot, _spline.custom.arrowLength, EventType.Repaint);
            //    }
            //}
        }
        else
        {
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

}
