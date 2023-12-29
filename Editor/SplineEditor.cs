using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SplineController))]
public class SplineEditor : Editor
{
    SplineController _spline;

    int _selectedSegment = -1;
    Vector3 _mousePosOnPlane = Vector3.zero;

    private void OnEnable()
    {
        _spline = (SplineController)target;
        if (_spline.Path.IsNull)
            _spline.CreatePath();

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
        bool change = EditorGUILayout.Toggle("2D Spline", _spline.Path.Is2D);
        if (change == _spline.Path.Is2D)
            return;

        bool proceed = EditorUtility.DisplayDialog("Change Dimension?", $"Change spline from {(change ? "3D" : "2D")} to {(change ? "2D" : "3D")}", "Preceed", "Cancel");
        if (proceed)
            _spline.Path.Is2D = change;
    }

    private void DrawIsClosed()
    {
        GUI.enabled = false;
        _spline.Path.IsClosed = EditorGUILayout.Toggle("Is Closed", _spline.Path.IsClosed);
        GUI.enabled = true;
    }

    private void DrawAddSegmentButton()
    {
        if (GUILayout.Button("Add Segment"))
        {
            Undo.RecordObject(_spline, "Button Add Segment");
            _spline.Path.AddSegment();
            SceneView.RepaintAll();
        }
    }

    private void DrawToggleMoveButton()
    {
        if (GUILayout.Button($"{(_spline.isRotate ? "Toggle to move" : "Toggle to rotate")}"))
        {
            _spline.isRotate = !_spline.isRotate;

            if (!_spline.custom.alwaysShowArrows)
                RecalculateArrowBuffer();
            SceneView.RepaintAll();
        }
    }

    private void DrawClosePathToggle()
    {
        if (GUILayout.Button("Toggle Close"))
        {
            Undo.RecordObject(_spline, "Toggle Close");
            _spline.Path.ToggleClosed();
            SceneView.RepaintAll();
        }
    }

    private void DrawLoadCheckpointButton()
    {
        if (GUILayout.Button("Load Checkpoint"))
        {
            Undo.RecordObject(_spline, "Load Checkpoint");
            bool proceed = EditorUtility.DisplayDialog("LOAD CHECKPOINT", $"Load last checkpoint ({_spline.Path.Checkpoints.Count})?", "Preceed", "Cancel");
            if (proceed)
                _spline.Path.LoadCheckpoint();

            SceneView.RepaintAll();
        }
    }

    private void DrawCreateCheckpointButton()
    {
        if (GUILayout.Button("Create Checkpoint"))
        {
            Undo.RecordObject(_spline, "Create Checkpoint");
            _spline.Path.CreateCheckpoint();
            EditorUtility.DisplayDialog("Checkpoint", $"Checkpoint {_spline.Path.Checkpoints.Count} created", "Ok", null);
        }
    }

    private void DrawDeleteCheckpointButton()
    {
        if (GUILayout.Button("Delete Checkpoint"))
        {
            if (_spline.Path.Checkpoints.Count - 1 <= 0)
                EditorUtility.DisplayDialog("No deletion", "Can not delete root checkpoint!", "Ok", null);
            else
            {
                Undo.RecordObject(_spline, "Delete Checkpoint");
                bool proceed = EditorUtility.DisplayDialog("DELETE CHECKPOINT", $"Delete Checkpoint {_spline.Path.Checkpoints.Count}!", "Proceed", "Cancel");
                if (proceed)
                    _spline.Path.DeleteCheckpoint();
            }
        }
    }

    private void DrawPath(ref bool foldOut)
    {
        foldOut = EditorGUILayout.Foldout(foldOut, "Path");
        if (!foldOut)
            return;
        EditorGUI.indentLevel++;
        for (int i = 0; i < _spline.Path.NumPoints; i++)
        {
            Vector3 oldPos = _spline.Path[i];
            Vector3 pos = EditorGUILayout.Vector3Field($"{(i % 3 == 0 ? $"Anchor {i / 3}" : $"Con {i}")}", _spline.Path[i]);
            if (pos != oldPos)
                _spline.Path.MovePoint(i, pos);
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
        for (int i = 0; i < _spline.Path.NumRotations; i++)
        {
            Vector3 oldRot = _spline.Path.GetRotation(i).eulerAngles;
            Vector3 rot = EditorGUILayout.Vector3Field($"Rot {i}", _spline.Path.GetRotation(i).eulerAngles);
            if (rot != oldRot)
                _spline.Path.RotatePoint(i, Quaternion.Euler(new Vector3((float)Math.Round(rot.x, 2), (float)Math.Round(rot.y, 2), (float)Math.Round(rot.z, 2))));
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
            custom.arrowDistance = EditorGUILayout.Slider("Arrow Distance", custom.arrowDistance, 0.05f, 1);

            if (oldDistance != custom.arrowDistance && _spline.custom.alwaysShowArrows)
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
        for (int i = 0; i < _spline.Path.Checkpoints.Count; i++)
        {
            _spline.Path.Checkpoints[i].foldOut = EditorGUILayout.Foldout(_spline.Path.Checkpoints[i].foldOut, $"Checkpoint {i + 1}");
            if (!_spline.Path.Checkpoints[i].foldOut)
                continue;

            GUI.enabled = false;
            EditorGUILayout.Toggle("Is Closed", _spline.Path.Checkpoints[i].isClosed);
            EditorGUILayout.Toggle("Is 2D", _spline.Path.Checkpoints[i].is2D);
            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Points", EditorStyles.boldLabel);

            GUI.enabled = false;
            for (int j = 0; j < _spline.Path.Checkpoints[i].points.Count; j++)
            {
                EditorGUILayout.Vector3Field($"{(j % 3 == 0 ? $"Anchor {j / 3}" : $"Con {j}")}", _spline.Path.Checkpoints[i].points[j]);
            }
            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rotations", EditorStyles.boldLabel);

            GUI.enabled = false;
            for (int j = 0; j < _spline.Path.Checkpoints[i].rotations.Count; j++)
            {
                EditorGUILayout.Vector3Field($"Rot {j}", _spline.Path.Checkpoints[i].rotations[j].eulerAngles);
            }
            GUI.enabled = true;
        }
        EditorGUI.indentLevel--;
    }

    private void OnSceneGUI()
    {

        Input(); //Input befor Paint
        Paint(_spline.isRotate);


        // INFO: Debug
        // <------------------------------------------>
        CubicBezier b = _spline.Path.GetBezierOfSegment(0).Transform(_spline.transform.localToWorldMatrix);
        Handles.CircleHandleCap(0, b.GetPoint(0.25f), Quaternion.identity, 0.01f, EventType.Repaint);
        Handles.CircleHandleCap(1, b.GetPoint(0.75f), Quaternion.identity, 0.01f, EventType.Repaint);
        // <------------------------------------------>

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

            for (int i = 0; i < _spline.Path.NumSegments; i++)
            {
                //CubicBezier localBezier = _spline.Path.GetBezierOfSegment(i);

                // beziere in world space (no rotations)
                CubicBezier bezier = _spline.Path.GetBezierOfSegment(i).Transform(_spline.transform.localToWorldMatrix);
                //for (int j = 0; j < 4; j++)
                //{
                //    bezier[j] = _spline.transform.TransformPoint(localBezier[j]);
                //}

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

        // inserts a segment at the mouse position on the spline (only in 2D)
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.control && _selectedSegment != -1 && _spline.Path.Is2D)
        {
            Undo.RecordObject(_spline, "Insert Segment");
            _spline.Path.InsertSegment(_selectedSegment, _spline.transform.InverseTransformPoint(_mousePosOnPlane));

            if (_spline.custom.alwaysShowArrows)
                RecalculateArrowBuffer();
        }

        // Adds a segment at the mouse position (only for 2D)
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift && SceneView.lastActiveSceneView.in2DMode && _spline.Path.Is2D)
        {
            Undo.RecordObject(_spline, "Add Segment");
            _spline.Path.AddSegment(mousPos, Quaternion.identity);
        }

        // removes a segment at the mouse position
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1 && guiEvent.control)
        {
            float distToAnchor = 0.05f;
            int closestAnchorIndex = -1;
            for (int i = 0; i < _spline.Path.NumPoints; i++)
            {
                if (i % 3 != 0)
                    continue;

                float dist = DistFromPoint(r, _spline.transform.TransformPoint(_spline.Path[i]));
                if (dist < distToAnchor)
                {
                    distToAnchor = dist;
                    closestAnchorIndex = i;
                }
            }
            if (closestAnchorIndex != -1)
            {
                Undo.RecordObject(_spline, "Remove Segment");
                _spline.Path.DeleteSegment(closestAnchorIndex);

                if (_spline.custom.alwaysShowArrows)
                    RecalculateArrowBuffer();
            }
        }
    }

    private void Paint(bool isRotate)
    {
        for (int i = 0; i < _spline.Path.NumSegments; i++)
        {
            CubicBezier bezier = _spline.Path.GetBezierOfSegment(i).Transform(_spline.transform.localToWorldMatrix);
            if (!isRotate)
            {
                Handles.color = _spline.custom.connectionColor;
                Handles.DrawLine(bezier.p1, bezier.p2);
                Handles.DrawLine(bezier.p3, bezier.p4);
            }
            Color color = _selectedSegment == i ? _spline.custom.selectedColor : _spline.custom.splineColor;
            Handles.DrawBezier(bezier.p1, bezier.p4, bezier.p2, bezier.p3, color, null, 2);

        }

        PathInfo info = _spline.Path.Transform(_spline.transform.localToWorldMatrix);

        if (isRotate)
            PaintRotation(info);
        else
            PaintMove(info);
        
    }

    private void PaintMove(PathInfo pathInfo)
    {
        for (int i = 0; i < pathInfo.points.Length; i++)
        {
            Handles.color = i % 3 == 0 ? _spline.custom.anchorColor : _spline.custom.controlColor;
            float size = i % 3 == 0 ? 0.1f : 0.075f;
            Vector3 newPos = Handles.FreeMoveHandle(pathInfo.points[i], size, Vector3.zero, Handles.CylinderHandleCap);

            if (newPos == pathInfo.points[i])
                continue;

            if (_spline.custom.alwaysShowArrows)
                RecalculateArrowBuffer();
            Undo.RecordObject(_spline, "Move Point Scene");
            _spline.Path.MovePoint(i, _spline.transform.InverseTransformPoint(newPos));

        }

        if (_spline.custom.alwaysShowArrows)
            PaintRotationArrows();
    }

    private void PaintRotation(PathInfo pathInfo)
    {
        for (int i = 0; i < pathInfo.rotations.Length; i++)
        {
            float size = 0.1f;
            Handles.color = _spline.custom.anchorColor;
            Handles.FreeMoveHandle(pathInfo.points[i * 3], size, Vector3.zero, Handles.CylinderHandleCap);

            Quaternion newQuat = Handles.RotationHandle(pathInfo.rotations[i], pathInfo.points[i * 3]);

            if (newQuat == pathInfo.rotations[i])
                continue;
            Undo.RecordObject(_spline, "Rotate Point Scene");
            _spline.Path.RotatePoint(i, Quaternion.Inverse(_spline.transform.rotation) * newQuat);
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
                    Quaternion rot = _spline.CalculateRotationWorld(i + p[j]);
                    Vector3 pos = _spline.CalculatePositionWorld(i + p[j]);

                    Handles.ArrowHandleCap(i, pos, rot, _spline.custom.arrowLength, EventType.Repaint);
                }
            }
            return;
        }
        float arrowsDistribution = _spline.custom.arrowDistribution;
        for (int j = 0; j < _spline.Path.NumSegments; j++)
        {
            for (int i = 0; i < arrowsDistribution; i++)
            {
                Quaternion rot = _spline.CalculateRotationWorld(j + i / arrowsDistribution);
                Vector3 pos = _spline.CalculatePositionWorld(j + i / arrowsDistribution);

                Handles.ArrowHandleCap(i, pos, rot, _spline.custom.arrowLength, EventType.Repaint);
            }
        }
    }

    private void RecalculateArrowBuffer()
    {
        _spline.bufferedArrowDistribution.Clear();
        for (int i = 0; i < _spline.Path.NumSegments; i++)
        {
            _spline.bufferedArrowDistribution.Add(_spline.Path.GetBezierOfSegment(i).EqualDistancePoints(_spline.custom.arrowDistance));
        }
    }

    private float DistFromPoint(Ray r, Vector3 cPos)
    {
        float lambd = -((((r.origin.x - cPos.x) * r.direction.x) + ((r.origin.y - cPos.y) * r.direction.y) + ((r.origin.z - cPos.z) * r.direction.z)) / (Mathf.Pow(r.direction.x, 2) + Mathf.Pow(r.direction.y, 2) + Mathf.Pow(r.direction.z, 2)));
        float dist = Mathf.Sqrt(Mathf.Pow(r.origin.x + lambd * r.direction.x - cPos.x, 2) + Mathf.Pow(r.origin.y + lambd * r.direction.y - cPos.y, 2) + Mathf.Pow(r.origin.z + lambd * r.direction.z - cPos.z, 2));
        return dist;
    }

}
