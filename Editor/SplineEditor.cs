using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(SplineController))]
public class SplineEditor : Editor
{
    SplineController _spline;
    Path _path;

    int _selectedSegment = -1;
    Vector3 _mousePosOnPlane = Vector3.zero;

    private void OnEnable()
    {
        _spline = (SplineController)target;
        if (_spline.path.IsNull)
            _spline.CreatePath();
        _path = _spline.path;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        ChangeTo2D();
        ToggleMoveButton();
        if (_spline.isRotate) DrawRotations(ref _spline.rotationsFoldOut);
        else DrawPath(ref _spline.pathFoldOut);
        EditorGUILayout.Space();
        AddSegmentButton();
        ClosePath();
        EditorGUILayout.Space();
        CreateCheckpoint();
        LoadCheckpoint();
        DeleteCheckpoint();
        EditorGUILayout.Space();
        DrawSplineCustomizer();
        DrawUpdateBezierDistButton();   // temporary implementation
        EditorGUILayout.Space();
        Info(ref _spline.infoFoldOut);

    }
    // temporary implementation
    private void DrawUpdateBezierDistButton()
    {
        EditorGUILayout.LabelField("Temporare");
        GUI.enabled = _spline.custom.arrowDistributionByDistance;
        if (GUILayout.Button("Recalculate Bezier Distances"))
        {
            _path.RecalculateDistancesT(_spline.custom.arrowDistance, _spline.custom.arrowResolution);
        }
        GUI.enabled = true;
    }

    private void ChangeTo2D()
    {
        Undo.RecordObject(_spline, "Dimenson Change");
        bool change = EditorGUILayout.Toggle("2D Spline", _path.Is2D);
        if (change == _path.Is2D)
            return;

        bool proceed = EditorUtility.DisplayDialog("Change Dimension?", $"Change spline from {(change ? "3D" : "2D")} to {(change ? "2D" : "3D")}", "Preceed", "Cancel");
        if (proceed)
            _path.Is2D = change;
    }

    private void DrawIsClosed()
    {
        GUI.enabled = false;
        _path.IsClosed = EditorGUILayout.Toggle("Is Closed", _path.IsClosed);
        GUI.enabled = true;
    }

    private void AddSegmentButton()
    {
        if (GUILayout.Button("Add Segment"))
        {
            Undo.RecordObject(_spline, "Button Add Segment");
            _path.AddSegment();
            SceneView.RepaintAll();
        }
    }

    private void ToggleMoveButton()
    {
        if (GUILayout.Button($"{(_spline.isRotate ? "Toggle to move" : "Toggle to rotate")}"))
        {
            _spline.isRotate = !_spline.isRotate;
            SceneView.RepaintAll();
        }
    }

    private void ClosePath()
    {
        if (GUILayout.Button("Toggle Close"))
        {
            Undo.RecordObject(_spline, "Toggle Close");
            _path.ToggleClosed();
            SceneView.RepaintAll();
        }
    }

    private void LoadCheckpoint()
    {
        if (GUILayout.Button("Load Checkpoint"))
        {
            Undo.RecordObject(_spline, "Load Checkpoint");
            bool proceed = EditorUtility.DisplayDialog("LOAD CHECKPOINT", $"Load last checkpoint ({_path.checkpoints.Count})?", "Preceed", "Cancel");
            if (proceed)
                _path.LoadLastCheckpoint();

            SceneView.RepaintAll();
        }
    }

    private void CreateCheckpoint()
    {
        if (GUILayout.Button("Create Checkpoint"))
        {
            Undo.RecordObject(_spline, "Create Checkpoint");
            _path.CreateCheckpoint();
            EditorUtility.DisplayDialog("Checkpoint", $"Checkpoint {_path.checkpoints.Count} created", "Ok", null);
        }
    }

    private void DeleteCheckpoint()
    {
        if (GUILayout.Button("Delete Checkpoint"))
        {
            if (_path.checkpoints.Count - 1 <= 0)
                EditorUtility.DisplayDialog("No deletion", "Can not delete root checkpoint!", "Ok", null);
            else
            {
                Undo.RecordObject(_spline, "Delete Checkpoint");
                bool proceed = EditorUtility.DisplayDialog("DELETE CHECKPOINT", $"Delete Checkpoint {_path.checkpoints.Count}!", "Proceed", "Cancel");
                if (proceed)
                    _path.DeleteCheckpoint();
            }
        }
    }

    private void DrawPath(ref bool foldOut)
    {
        foldOut = EditorGUILayout.Foldout(foldOut, "Path");
        if (!foldOut)
            return;
        EditorGUI.indentLevel++;
        for (int i = 0; i < _path.NumPoints; i++)
        {
            _path.MovePoint(i, EditorGUILayout.Vector3Field($"{(i % 3 == 0 ? $"Anchor {i / 3}" : $"Con {i}")}", _path[i]));
        }
        EditorGUI.indentLevel--;
        SceneView.RepaintAll();
    }

    private void DrawRotations(ref bool foldOut)
    {
        foldOut = EditorGUILayout.Foldout(foldOut, "Rotations");
        if (!foldOut)
            return;

        EditorGUI.indentLevel++;
        for (int i = 0; i < _path.NumRotations; i++)
        {
            Vector3 rot = EditorGUILayout.Vector3Field($"Rot {i}", _path.GetRotation(i).eulerAngles);
            _path.RotatePoint(i, Quaternion.Euler(new Vector3((float)Math.Round(rot.x, 2), (float)Math.Round(rot.y, 2), (float)Math.Round(rot.z, 2))));
        }
        EditorGUI.indentLevel--;
        SceneView.RepaintAll();
    }

    private void DrawSplineCustomizer()
    {
        _spline.customizerFoldOut = EditorGUILayout.Foldout(_spline.customizerFoldOut, "Customizer");
        if (!_spline.customizerFoldOut)
            return;

        EditorGUI.indentLevel++;
        _spline.custom.splineColor = EditorGUILayout.ColorField("Spline Color", _spline.custom.splineColor);
        _spline.custom.selectedColor = EditorGUILayout.ColorField("Selected Color", _spline.custom.selectedColor);
        _spline.custom.connectionColor = EditorGUILayout.ColorField("Connection Color", _spline.custom.connectionColor);

        _spline.custom.anchorColor = EditorGUILayout.ColorField("Anchor Color", _spline.custom.anchorColor);
        _spline.custom.controlColor = EditorGUILayout.ColorField("Control Color", _spline.custom.controlColor);

        _spline.custom.arrowColor = EditorGUILayout.ColorField("Arrow Color", _spline.custom.arrowColor);

        EditorGUILayout.Space();

        _spline.custom.arrowDistributionByDistance = EditorGUILayout.Toggle("Arrow Distribution By Distance", _spline.custom.arrowDistributionByDistance);
        bool isDist = _spline.custom.arrowDistributionByDistance;

        _spline.custom.arrowLength = EditorGUILayout.Slider("Arrow Length", _spline.custom.arrowLength, 0, 1f);
        if (isDist)
        {
            int clampedRes = _spline.custom.arrowResolution < 1 ? 1 : _spline.custom.arrowResolution;
            _spline.custom.arrowResolution = EditorGUILayout.IntField("Arrow Resolution", clampedRes);
            _spline.custom.arrowDistance = EditorGUILayout.Slider("Distance", _spline.custom.arrowDistance, 0f, 1f);
        }
        else
        {
            _spline.custom.arrowDistribution = EditorGUILayout.IntSlider("Arrow Distribution", _spline.custom.arrowDistribution, 1, 250);
        }
        EditorGUI.indentLevel--;

        SceneView.RepaintAll();
    }

    private void Info(ref bool infoFoldOut)
    {
        infoFoldOut = EditorGUILayout.Foldout(infoFoldOut, "Info");
        if (!infoFoldOut)
            return;

        EditorGUI.indentLevel++;
        DrawIsClosed();
        DrawCheckpoints(ref _spline.checkpointsFoldOut);
        EditorGUI.indentLevel--;

    }

    private void DrawCheckpoints(ref bool checkpointsFoldOut)
    {
        checkpointsFoldOut = EditorGUILayout.Foldout(checkpointsFoldOut, "Checkpoints");
        if (!checkpointsFoldOut)
            return;


        EditorGUI.indentLevel++;
        for (int i = 0; i < _path.checkpoints.Count; i++)
        {
            _path.checkpoints[i].foldOut = EditorGUILayout.Foldout(_path.checkpoints[i].foldOut, $"Checkpoint {i + 1}");
            if (!_path.checkpoints[i].foldOut)
                continue;
            GUI.enabled = false;
            EditorGUILayout.Toggle("Is Closed", _path.checkpoints[i].isClosed);
            EditorGUILayout.Toggle("Is 2D", _path.checkpoints[i].is2D);
            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Points", EditorStyles.boldLabel);

            GUI.enabled = false;
            for (int j = 0; j < _path.checkpoints[i].points.Count; j++)
            {
                EditorGUILayout.Vector3Field($"{(j % 3 == 0 ? $"Anchor {j / 3}" : $"Con {j}")}", _path.checkpoints[i].points[j]);
            }
            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rotations", EditorStyles.boldLabel);

            GUI.enabled = false;
            for (int j = 0; j < _path.checkpoints[i].rotations.Count; j++)
            {
                EditorGUILayout.Vector3Field($"Rot {j}", _path.checkpoints[i].rotations[j].eulerAngles);
            }
            GUI.enabled = true;
        }
        EditorGUI.indentLevel--;
    }

    private void OnSceneGUI()
    {
        Matrix4x4 originalMatrix = Handles.matrix;

        Handles.matrix = _spline.transform.localToWorldMatrix;
        Input();
        Draw(_spline.isRotate);
        Handles.matrix = originalMatrix;
    }

    private void Input()
    {
        Event guiEvent = Event.current;

        Ray r = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        Vector3 mousPos = r.origin;

        if (guiEvent.type == EventType.MouseMove)
        {
            float nearestDist = 0.05f;
            int nearestSegment = -1;

            for (int i = 0; i < _path.NumSegments; i++)
            {
                Vector3[] loaclPoints = _path.GetPointsInSegment(i);
                Vector3[] points = new Vector3[4];
                for (int j = 0; j < points.Length; j++)
                {
                    points[j] = _spline.transform.TransformPoint(loaclPoints[j]);
                }

                Vector3 p1TOp2 = points[3] - points[0];
                Vector3 planeNormal = Vector3.Cross(p1TOp2, _spline.transform.up).normalized;

                float dot = Vector3.Dot(r.direction, planeNormal);
                if (dot <= 0.15f && dot >= -0.15f)
                    continue;


                float distToIntersection = Vector3.Dot(points[0] - r.origin, planeNormal) / dot;
                if (distToIntersection <= 0)
                    continue;

                Vector3 pointOnPlane = r.origin + distToIntersection * r.direction;
                _mousePosOnPlane = pointOnPlane;

                float dist = HandleUtility.DistancePointBezier(pointOnPlane, points[0], points[3], points[1], points[2]);

                if (dist >= nearestDist)
                    continue;

                nearestDist = dist;
                nearestSegment = i;
            }
            if (nearestSegment != _selectedSegment)
            {
                _selectedSegment = nearestSegment;
                HandleUtility.Repaint();
            }
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.control && _selectedSegment != -1)
        {
            Undo.RecordObject(_spline, "Insert Segment");
            _path.InsertSegment(_selectedSegment, _mousePosOnPlane);
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift && SceneView.lastActiveSceneView.in2DMode)
        {
            Undo.RecordObject(_spline, "Add Segment");
            _path.AddSegment(mousPos, Quaternion.identity);
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
        {
            float distToAnchor = 0.05f;
            int closestAnchorIndex = -1;
            for (int i = 0; i < _path.NumPoints; i++)
            {
                float dist = DistFromCircle(r, _path[i]);
                if (dist < distToAnchor)
                {
                    distToAnchor = dist;
                    closestAnchorIndex = i;
                }
            }
            if (closestAnchorIndex != -1)
            {
                Undo.RecordObject(_spline, "Remove Segment");
                _path.DeletePoint(closestAnchorIndex);
            }
        }
    }

    private void Draw(bool isRotate)
    {
        for (int i = 0; i < _path.NumSegments; i++)
        {
            Vector3[] points = _path.GetPointsInSegment(i);
            if (!isRotate)
            {
                Handles.color = _spline.custom.connectionColor;
                Handles.DrawLine(points[0], points[1]);
                Handles.DrawLine(points[2], points[3]);
            }
            Color color = _selectedSegment == i ? _spline.custom.selectedColor : _spline.custom.splineColor;
            Handles.DrawBezier(points[0], points[3], points[1], points[2], color, null, 2);

            if (isRotate)
                DrawRotation();
            else
                DrawMove();

        }

    }

    private void DrawMove()
    {
        for (int i = 0; i < _path.NumPoints; i++)
        {
            Handles.color = i % 3 == 0 ? _spline.custom.anchorColor : _spline.custom.controlColor;
            float size = i % 3 == 0 ? 0.1f : 0.075f;
            Vector3 newPos = Handles.FreeMoveHandle(_path[i], size, Vector3.zero, Handles.CylinderHandleCap);

            if (newPos == _path[i])
                continue;

            Undo.RecordObject(_spline, "Move Point");
            _path.MovePoint(i, newPos);

        }
    }

    private void DrawRotation()
    {
        for (int i = 0; i < _path.NumRotations; i++)
        {
            float size = 0.1f;
            Handles.color = _spline.custom.anchorColor;
            Handles.FreeMoveHandle(_path[i * 3], size, Vector3.zero, Handles.CylinderHandleCap);
            Quaternion newQuat = Handles.RotationHandle(_path.GetRotation(i), _path[i * 3]);

            Undo.RecordObject(_spline, "Rotate Point");
            _path.RotatePoint(i, newQuat);
        }


        Handles.color = _spline.custom.arrowColor;

        if (_spline.custom.arrowDistributionByDistance)
        {
            for (int i = 0; i < _path.NumDistancesT; i++)
            {
                float[] distancesT = _path.GetDistancesT(i);
                for (int j = 0; j < distancesT.Length; j++)
                {
                    Quaternion rot = _spline.CalculateRotation(i + distancesT[j]);
                    Vector3 pos = _spline.CalculatePosition(i + distancesT[j]);
                    Handles.ArrowHandleCap(i, pos, rot, _spline.custom.arrowLength, EventType.Repaint);
                }
            }
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

    private float DistFromCircle(Ray r, Vector3 cPos)
    {
        float lambd = -((((r.origin.x - cPos.x) * r.direction.x) + ((r.origin.y - cPos.y) * r.direction.y) + ((r.origin.z - cPos.z) * r.direction.z)) / (Mathf.Pow(r.direction.x, 2) + Mathf.Pow(r.direction.y, 2) + Mathf.Pow(r.direction.z, 2)));
        float dist = Mathf.Sqrt(Mathf.Pow(r.origin.x + lambd * r.direction.x - cPos.x, 2) + Mathf.Pow(r.origin.y + lambd * r.direction.y - cPos.y, 2) + Mathf.Pow(r.origin.z + lambd * r.direction.z - cPos.z, 2));
        return dist;
    }

}
