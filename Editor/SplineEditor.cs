using log4net.Filter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        RecalculateArrowBuffer();
        Undo.undoRedoPerformed += RecalculateArrowBuffer;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= RecalculateArrowBuffer;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        DrawOpenDocsButton();

        EditorGUILayout.Space();

        DrawToggleTo2D();
        DrawToggleMoveButton();
        if (_spline.isRotate) DrawRotations(ref _spline.rotationsFoldOut);
        else DrawPath(ref _spline.pathFoldOut);

        EditorGUILayout.Space();

        DrawAddSegmentButton();
        DrawClosePathToggle();

        EditorGUILayout.Space();

        DrawCreateCheckpointButton();
        DrawLoadCheckpointButton();
        DrawDeleteCheckpointButton();

        EditorGUILayout.Space();

        DrawSplineCustomizer();

        EditorGUILayout.Space();

        Info(ref _spline.infoFoldOut);

    }

    private void DrawOpenDocsButton()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Docs", EditorStyles.miniButtonRight, GUILayout.Width(80)))
            Application.OpenURL("http://poi-desk.at");
        EditorGUILayout.EndHorizontal();
    }

    private void DrawToggleTo2D()
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

    private void DrawAddSegmentButton()
    {
        if (GUILayout.Button("Add Segment"))
        {
            Undo.RecordObject(_spline, "Button Add Segment");
            _path.AddSegment();
            SceneView.RepaintAll();
        }
    }

    private void DrawToggleMoveButton()
    {
        if (GUILayout.Button($"{(_spline.isRotate ? "Toggle to move" : "Toggle to rotate")}"))
        {
            _spline.isRotate = !_spline.isRotate;
            SceneView.RepaintAll();
        }
    }

    private void DrawClosePathToggle()
    {
        if (GUILayout.Button("Toggle Close"))
        {
            Undo.RecordObject(_spline, "Toggle Close");
            _path.ToggleClosed();
            SceneView.RepaintAll();
        }
    }

    private void DrawLoadCheckpointButton()
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

    private void DrawCreateCheckpointButton()
    {
        if (GUILayout.Button("Create Checkpoint"))
        {
            Undo.RecordObject(_spline, "Create Checkpoint");
            _path.CreateCheckpoint();
            EditorUtility.DisplayDialog("Checkpoint", $"Checkpoint {_path.checkpoints.Count} created", "Ok", null);
        }
    }

    private void DrawDeleteCheckpointButton()
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
            Vector3 oldPos = _path[i];
            Vector3 pos = EditorGUILayout.Vector3Field($"{(i % 3 == 0 ? $"Anchor {i / 3}" : $"Con {i}")}", _path[i]);
            if (pos != oldPos)
                _path.MovePoint(i, pos);
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
            Vector3 oldRot = _path.GetRotation(i).eulerAngles;
            Vector3 rot = EditorGUILayout.Vector3Field($"Rot {i}", _path.GetRotation(i).eulerAngles);
            if (rot != oldRot)
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

        SplineCustomizer custom = _spline.custom;

        EditorGUI.indentLevel++;

        custom.splineColor = EditorGUILayout.ColorField("Spline Color", custom.splineColor);
        custom.selectedColor = EditorGUILayout.ColorField("Selected Color", custom.selectedColor);
        custom.connectionColor = EditorGUILayout.ColorField("Connection Color", custom.connectionColor);
        custom.anchorColor = EditorGUILayout.ColorField("Anchor Color", custom.anchorColor);
        custom.controlColor = EditorGUILayout.ColorField("Control Color", custom.controlColor);
        custom.arrowColor = EditorGUILayout.ColorField("Arrow Color", custom.arrowColor);

        EditorGUILayout.Space();

        custom.alwaysShowArrows = EditorGUILayout.Toggle("Always Show Arrows", custom.alwaysShowArrows);

        custom.useArrowDistanceDistribution = EditorGUILayout.Toggle("Arrow Distance Distribution", custom.useArrowDistanceDistribution);

        EditorGUILayout.Space();

        custom.arrowLength = EditorGUILayout.Slider("Arrow Length", custom.arrowLength, 0, 1f);

        if (custom.useArrowDistanceDistribution)
        {
            float oldDistance = custom.arrowDistance;
            custom.arrowDistance = EditorGUILayout.Slider("Arrow Distance", custom.arrowDistance, 0.1f, 1);

            if (oldDistance != custom.arrowDistance)
                RecalculateArrowBuffer();
        }
        else
            custom.arrowDistribution = EditorGUILayout.IntSlider("Arrow Distribution", custom.arrowDistribution, 1, 250);

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
        //Save the old matrix
        Matrix4x4 originalMatrix = Handles.matrix;

        //Set Handles.matrix to the localToWorldMatrix to enable to draw the spline in relation to the spline object
        Handles.matrix = _spline.transform.localToWorldMatrix;

        Input(); //Input befor Draw
        Paint(_spline.isRotate);
        
        // INFO: Debug
        // ------------------------------------------
        CubicBezier b = _path.GetBezierOfSegment(0);
        Handles.CircleHandleCap(0, b.GetPoint(0.25f), Quaternion.identity, 0.01f, EventType.Repaint);
        Handles.CircleHandleCap(1, b.GetPoint(0.75f), Quaternion.identity, 0.01f, EventType.Repaint);
        // ------------------------------------------

        //Reinstantiate the old matrix
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
                CubicBezier localBezier = _path.GetBezierOfSegment(i);
                
                // beziere in world space (no rotations)
                CubicBezier bezier = new();
                for (int j = 0; j < 4; j++)
                {
                    bezier[j] = _spline.transform.TransformPoint(localBezier[j]);
                }

                Vector3 p1TOp2 = bezier.p4 - bezier.p1;
                Vector3 planeNormal = Vector3.Cross(p1TOp2, _spline.transform.up).normalized;

                float dot = Vector3.Dot(r.direction, planeNormal);
                if (dot <= 0.01f && dot >= -0.01f)
                    continue;

                float distToIntersection = Vector3.Dot(bezier.p1 - r.origin, planeNormal) / dot;
                if (distToIntersection <= 0)
                    continue;

                Vector3 pointOnPlane = r.origin + distToIntersection * r.direction;
                _mousePosOnPlane = pointOnPlane;

                float dist = HandleUtility.DistancePointBezier(pointOnPlane, bezier.p1, bezier.p4, bezier.p2, bezier.p3);

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

        // INFO: Inserts a segment at the mouse position on the spline
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.control && _selectedSegment != -1 && _path.Is2D)
        {
            Undo.RecordObject(_spline, "Insert Segment");
            _path.InsertSegment(_selectedSegment, _spline.transform.InverseTransformPoint(_mousePosOnPlane));
            RecalculateArrowBuffer();
        }

        // TODO: remove or make better for 3D
        // INFO: Adds a segment at the mouse position (only for 2D view)
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift && SceneView.lastActiveSceneView.in2DMode)
        {
            Undo.RecordObject(_spline, "Add Segment");
            _path.AddSegment(mousPos, Quaternion.identity);
        }

        // INFO: removes a segment at the mouse position
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1 && guiEvent.control)
        {
            float distToAnchor = 0.05f;
            int closestAnchorIndex = -1;
            for (int i = 0; i < _path.NumPoints; i++)
            {
                if (i % 3 != 0)
                    continue;
                
                float dist = DistFromPoint(r, _spline.transform.TransformPoint(_path[i]));
                if (dist < distToAnchor)
                {
                    distToAnchor = dist;
                    closestAnchorIndex = i;
                }
            }
            if (closestAnchorIndex != -1)
            {
                Undo.RecordObject(_spline, "Remove Segment");
                _path.DeleteSegment(closestAnchorIndex);
                RecalculateArrowBuffer();
            }
        }
    }

    private void Paint(bool isRotate)
    {
        for (int i = 0; i < _path.NumSegments; i++)
        {
            CubicBezier bezier = _path.GetBezierOfSegment(i);
            if (!isRotate)
            {
                Handles.color = _spline.custom.connectionColor;
                Handles.DrawLine(bezier.p1, bezier.p2);
                Handles.DrawLine(bezier.p3, bezier.p4);
            }
            Color color = _selectedSegment == i ? _spline.custom.selectedColor : _spline.custom.splineColor;
            Handles.DrawBezier(bezier.p1, bezier.p4, bezier.p2, bezier.p3, color, null, 2);

        }
        if (isRotate)
            PaintRotation();
        else
            PaintMove();

    }

    private void PaintMove()
    {
        for (int i = 0; i < _path.NumPoints; i++)
        {
            Handles.color = i % 3 == 0 ? _spline.custom.anchorColor : _spline.custom.controlColor;
            float size = i % 3 == 0 ? 0.1f : 0.075f;
            Vector3 newPos = Handles.FreeMoveHandle(_path[i], size, Vector3.zero, Handles.CylinderHandleCap);

            if (newPos == _path[i])
                continue;

            RecalculateArrowBuffer();
            Undo.RecordObject(_spline, "Move Point Scene");
            _path.MovePoint(i, newPos);

        }

        if (_spline.custom.alwaysShowArrows)
            PaintRotationArrows();
    }

    private void PaintRotation()
    {
        for (int i = 0; i < _path.NumRotations; i++)
        {
            float size = 0.1f;
            Handles.color = _spline.custom.anchorColor;
            Handles.FreeMoveHandle(_path[i * 3], size, Vector3.zero, Handles.CylinderHandleCap);
            Quaternion newQuat = Handles.RotationHandle(_path.GetRotation(i), _path[i * 3]);

            if (newQuat == _path.GetRotation(i))
                continue;
            Undo.RecordObject(_spline, "Rotate Point Scene");
            _path.RotatePoint(i, newQuat);
        }

        PaintRotationArrows();

    }

    private void PaintRotationArrows()
    {
        Handles.color = _spline.custom.arrowColor;

        if (_spline.custom.useArrowDistanceDistribution)
        {
            for (int i = 0; i < _spline.bufferedArrowDistribution.Count; i++)
            {
                float[] p = _spline.bufferedArrowDistribution[i];
                for (int j = 0; j < p.Length; j++)
                {
                    Quaternion rot = _spline.CalculateRotation(i + p[j]);
                    Vector3 pos = _spline.CalculatePosition(i + p[j]);
                    Handles.ArrowHandleCap(i, pos, rot, _spline.custom.arrowLength, EventType.Repaint);
                }
            }
            return;
        }
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

    private void RecalculateArrowBuffer()
    {
        _spline.bufferedArrowDistribution = new();
        for (int i = 0; i < _path.NumSegments; i++)
        {
            _spline.bufferedArrowDistribution.Add(_path.GetBezierOfSegment(i).EqualDistancePoints(_spline.custom.arrowDistance));
        }
    }

    private float DistFromPoint(Ray r, Vector3 cPos)
    {
        float lambd = -((((r.origin.x - cPos.x) * r.direction.x) + ((r.origin.y - cPos.y) * r.direction.y) + ((r.origin.z - cPos.z) * r.direction.z)) / (Mathf.Pow(r.direction.x, 2) + Mathf.Pow(r.direction.y, 2) + Mathf.Pow(r.direction.z, 2)));
        float dist = Mathf.Sqrt(Mathf.Pow(r.origin.x + lambd * r.direction.x - cPos.x, 2) + Mathf.Pow(r.origin.y + lambd * r.direction.y - cPos.y, 2) + Mathf.Pow(r.origin.z + lambd * r.direction.z - cPos.z, 2));
        return dist;
    }

}
