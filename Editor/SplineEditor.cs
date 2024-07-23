using GluonGui;
using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SplineController))]
public class SplineEditor : Editor
{
    SplineController _spline;

    int _selectedSegment = -1;
    Vector3 _mousePosOnPlane = Vector3.zero;

    Vector3 _oldScale;

    private void OnEnable()
    {
        _spline = (SplineController)target;
        if (_spline.Path.IsNull)
            _spline.CreatePath();

        _oldScale = _spline.transform.lossyScale;

        if (_spline.custom.useArrowDistanceDistribution)
            _spline.RecalculateArrowBuffer();

        Undo.undoRedoPerformed += _spline.RecalculateArrowBuffer;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= _spline.RecalculateArrowBuffer;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        DrawOpenDocsButton();

        _spline.IsStatic = EditorGUILayout.Toggle("Static", _spline.IsStatic);
        if (_spline.IsStatic)
        {
            GUI.enabled = !_spline.baking;
            if (GUILayout.Button("Bake Length"))
                _spline.BakeLength();
            GUI.enabled = true;
        }


        EditorGUILayout.Space();

        DrawToggleTo2D();
        DrawToggleMoveButton();
        if (_spline.isRotate) DrawRotations(ref _spline.rotationsFoldOut);
        else DrawPath(ref _spline.pathFoldOut);

        EditorGUILayout.Space();

        DrawAddSegmentButton();
        DrawClosePathToggle();

        EditorGUILayout.Space();

        _spline.checkpointsFoldOut = EditorGUILayout.Foldout(_spline.checkpointsFoldOut, "Checkpoints");
        if (_spline.checkpointsFoldOut)
        {
            EditorGUI.indentLevel++;
            DrawCreateCheckpointButton();
            DrawLoadCheckpointButton();
            DrawDeleteCheckpointButton();

            EditorGUILayout.Space();

            Info(ref _spline.infoFoldOut);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        DrawSplineCustomizer();

        EditorGUILayout.Space();

        CheckForScaleChange();


    }

    /// <summary>
    /// Draws a button in the inspector wich opens the documentation
    /// </summary>
    /// <returns>if the butten was pressed</returns>
    private bool DrawOpenDocsButton()
    {

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        bool pressed = GUILayout.Button("Open Docs", EditorStyles.miniButtonRight, GUILayout.Width(80));
        if (pressed)
            Application.OpenURL("http://poi-desk.at");

        EditorGUILayout.EndHorizontal();

        return pressed;
    }

    /// <summary>
    /// Draws a toggle in the inspector wich toggles the path between 2D and 3D
    /// </summary>
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

    /// <summary>
    /// Draws a button in the inspector wich adds a new segment to the path
    /// </summary>
    /// <returns>if the butten was pressed</returns>
    private bool DrawAddSegmentButton()
    {
        bool pressed = GUILayout.Button("Add Segment");
        if (pressed)
        {
            Undo.RecordObject(_spline, "Button Add Segment");
            _spline.Path.AddSegment();

            if ((_spline.isRotate || _spline.custom.alwaysShowArrows) && _spline.custom.useArrowDistanceDistribution)
                _spline.RecalculateArrowBuffer();

            SceneView.RepaintAll();
        }

        return pressed;
    }

    /// <summary>
    /// Draws a button in the inspector wich toggles the paths edit mode (rotate or move)
    /// </summary>
    /// <returns>the mode of the spline</returns>
    private bool DrawToggleMoveButton()
    {
        bool mode = _spline.isRotate;
        if (GUILayout.Button($"{(_spline.isRotate ? "Toggle to move" : "Toggle to rotate")}"))
        {
            _spline.isRotate = !_spline.isRotate;
            mode = _spline.isRotate;

            if (_spline.isRotate || (_spline.custom.alwaysShowArrows && !_spline.isRotate) && _spline.custom.useArrowDistanceDistribution)
                _spline.RecalculateArrowBuffer();

            SceneView.RepaintAll();
        }

        return mode;
    }

    /// <summary>
    /// Draws a toggle in the inspector wich toggles the paths closed state
    /// </summary>
    /// <returns>the close state of the path</returns>
    private bool DrawClosePathToggle()
    {
        bool state = _spline.Path.IsClosed;
        if (GUILayout.Button("Toggle Close"))
        {
            Undo.RecordObject(_spline, "Toggle Close");
            state = _spline.Path.ToggleClosed();

            if ((_spline.isRotate || _spline.custom.alwaysShowArrows) && _spline.custom.useArrowDistanceDistribution && state)
                _spline.RecalculateArrowBuffer();

            SceneView.RepaintAll();
        }

        return state;
    }

    /// <summary>
    /// Draws a button in the inspector wich loads the last checkpoint created
    /// </summary>
    /// <returns>if the butten was pressed</returns>
    private bool DrawLoadCheckpointButton()
    {
        bool pressed = GUILayout.Button("Load Checkpoint");
        if (pressed)
        {
            Undo.RecordObject(_spline, "Load Checkpoint");
            bool proceed = EditorUtility.DisplayDialog("LOAD CHECKPOINT", $"Load last checkpoint ({_spline.Checkpoints.Count})?", "Preceed", "Cancel");
            if (proceed)
            {
                _spline.LoadCheckpoint();

                if ((_spline.custom.alwaysShowArrows || _spline.isRotate) && _spline.custom.useArrowDistanceDistribution)
                    _spline.RecalculateArrowBuffer();
            }

            SceneView.RepaintAll();
        }

        return pressed;
    }

    /// <summary>
    /// Draws a button in the inspector wich creates a checkpoint with the paths current state
    /// </summary>
    /// <returns>if the butten was pressed</returns>
    private bool DrawCreateCheckpointButton()
    {
        bool pressed = GUILayout.Button("Create Checkpoint");
        if (pressed)
        {
            Undo.RecordObject(_spline, "Create Checkpoint");
            _spline.CreateCheckpoint();
            EditorUtility.DisplayDialog("Checkpoint", $"Checkpoint {_spline.Checkpoints.Count} created", "Ok", null);
        }

        return pressed;
    }

    /// <summary>
    /// Draws a button in the inspector wich deletes the last checkpoint created
    /// </summary>
    /// <returns>if the butten was pressed</returns>
    private bool DrawDeleteCheckpointButton()
    {
        bool pressed = GUILayout.Button("Delete Checkpoint");
        if (pressed)
        {
            if (_spline.Checkpoints.Count - 1 <= 0)
                EditorUtility.DisplayDialog("No deletion", "Can not delete root checkpoint!", "Ok", null);
            else
            {
                Undo.RecordObject(_spline, "Delete Checkpoint");
                bool proceed = EditorUtility.DisplayDialog("DELETE CHECKPOINT", $"Delete Checkpoint {_spline.Checkpoints.Count}!", "Proceed", "Cancel");
                if (proceed)
                    _spline.DeleteCheckpoint();
            }
        }

        return pressed;
    }

    /// <summary>
    /// Draws all points of the path in the inspector (foldable)
    /// </summary>
    /// <param name="foldOut"></param>
    private void DrawPath(ref bool foldOut)
    {
        foldOut = EditorGUILayout.Foldout(foldOut, "Path");
        if (!foldOut)
            return;
        EditorGUI.indentLevel++;
        for (int i = 0; i < _spline.Path.NumPoints; i++)
        {
            Vector3 oldPos = _spline.Path[i];
            Vector3 pos = EditorGUILayout.Vector3Field($"{(i % 3 == 0 ? $"Anchor {i / 3 + 1}" : $"Con {i - i / 3}")}", _spline.Path[i]);
            if (pos != oldPos)
            {
                _spline.Path.MovePoint(i, pos);
                if (_spline.custom.alwaysShowArrows && _spline.custom.useArrowDistanceDistribution)
                    _spline.RecalculateArrowBuffer();
            }

            if (i % 3 == 1)
            {
                DrawInsertButton((i - 1) / 3);
            }

        }
        EditorGUI.indentLevel--;
        SceneView.RepaintAll();
    }

    /// <summary>
    /// Draws all rotations of the path in the inspector (flodable)
    /// </summary>
    /// <param name="foldOut"></param>
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

    /// <summary>
    /// Draws a button in the inspectir wich inserts a segemnt at the given index
    /// <br></br>
    /// Position and rotation will be the average of the adjacent segments
    /// </summary>
    /// <param name="index">segment index</param>
    /// <returns>if the butten was pressed</returns>
    private bool DrawInsertButton(int index)
    {
        GUIStyle style = new(GUI.skin.button)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.LowerRight,
        };
        style.normal.textColor = Color.white;
        GUIContent buttonContent = new(">", "Insert a segment here");

        EditorGUILayout.BeginHorizontal();

        bool pressed = GUILayout.Button(buttonContent, style, GUILayout.Width(20), GUILayout.Height(20));

        EditorGUILayout.EndHorizontal();

        if (pressed)
        {
            CubicBezier b = _spline.Path.GetBezierOfSegment(index);

            Undo.RecordObject(_spline, "Insert Segment Button");
            _spline.Path.InsertSegment(index, (b.p1 + b.p4) / 2);

            if ((_spline.custom.alwaysShowArrows || _spline.isRotate) && _spline.custom.useArrowDistanceDistribution)
                _spline.RecalculateArrowBuffer();

            SceneView.RepaintAll();
        }

        return pressed;
    }

    /// <summary>
    /// Draws all fields in the splines custom 
    /// </summary>
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

        bool oldShowArrows = custom.alwaysShowArrows;
        custom.alwaysShowArrows = EditorGUILayout.Toggle("Always Show Arrows", custom.alwaysShowArrows);

        bool oldDist = custom.useArrowDistanceDistribution;
        custom.useArrowDistanceDistribution = EditorGUILayout.Toggle("Arrow Distance Distribution", custom.useArrowDistanceDistribution);

        EditorGUILayout.Space();

        custom.arrowLength = EditorGUILayout.Slider("Arrow Length", custom.arrowLength, 0, 1f);

        if (custom.useArrowDistanceDistribution)
        {
            float oldDistance = custom.arrowDistance;
            custom.arrowDistance = EditorGUILayout.Slider("Arrow Distance", custom.arrowDistance, 0.05f, 1);

            if ((oldDistance != custom.arrowDistance && (_spline.custom.alwaysShowArrows || _spline.isRotate)) || (!oldShowArrows && custom.alwaysShowArrows) || (!oldDist && custom.useArrowDistanceDistribution && (_spline.isRotate || custom.alwaysShowArrows)))
                _spline.RecalculateArrowBuffer();
        }
        else
            custom.arrowDistribution = EditorGUILayout.IntSlider("Arrow Distribution", custom.arrowDistribution, 1, 250);

        EditorGUI.indentLevel--;

        SceneView.RepaintAll();
    }

    /// <summary>
    /// Draws non editable information about the path in the inspector (foldable)
    /// </summary>
    /// <param name="infoFoldOut"></param>
    private void Info(ref bool infoFoldOut)
    {
        infoFoldOut = EditorGUILayout.Foldout(infoFoldOut, "Info");
        if (!infoFoldOut)
            return;

        EditorGUI.indentLevel++;
        DrawIsClosed();
        DrawCheckpoints();
        EditorGUI.indentLevel--;

    }

    /// <summary>
    /// Darws a toggle in the inspector wich shows the close state of the path
    /// </summary>
    private void DrawIsClosed()
    {
        GUI.enabled = false;
        EditorGUILayout.Toggle("Is Closed", _spline.Path.IsClosed);
        GUI.enabled = true;
    }

    /// <summary>
    /// Draws all checkpoints non editable in the inspector (foldable)
    /// </summary>
    /// <param name="checkpointsFoldOut"></param>
    private void DrawCheckpoints()
    {
        for (int i = 0; i < _spline.Checkpoints.Count; i++)
        {
            _spline.Checkpoints[i].foldOut = EditorGUILayout.Foldout(_spline.Checkpoints[i].foldOut, $"Checkpoint {i + 1}");
            if (!_spline.Checkpoints[i].foldOut)
                continue;

            GUI.enabled = false;
            EditorGUILayout.Toggle("Is Closed", _spline.Checkpoints[i].isClosed);
            EditorGUILayout.Toggle("Is 2D", _spline.Checkpoints[i].is2D);
            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Points", EditorStyles.boldLabel);

            GUI.enabled = false;
            for (int j = 0; j < _spline.Checkpoints[i].points.Count; j++)
            {
                EditorGUILayout.Vector3Field($"{(j % 3 == 0 ? $"Anchor {j / 3}" : $"Con {j}")}", _spline.Checkpoints[i].points[j]);
            }
            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rotations", EditorStyles.boldLabel);

            GUI.enabled = false;
            for (int j = 0; j < _spline.Checkpoints[i].rotations.Count; j++)
            {
                EditorGUILayout.Vector3Field($"Rot {j}", _spline.Checkpoints[i].rotations[j].eulerAngles);
            }
            GUI.enabled = true;
        }
    }

    /// <summary>
    /// Recalculates the arrow buffer on the splines lossyScale changed
    /// </summary>
    private void CheckForScaleChange()
    {
        if (_oldScale != _spline.transform.lossyScale && _spline.custom.useArrowDistanceDistribution)
        {
            _oldScale = _spline.transform.lossyScale;
            _spline.RecalculateArrowBuffer();
        }
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

    /// <summary>
    /// Tracks all inputs for the scene editor
    /// </summary>
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
                CubicBezier bezier = _spline.Path.GetBezierOfSegment(i).Transform(_spline.transform.localToWorldMatrix);

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


        // Inserts a segment at the mouse position on the spline (only in 2D)
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.control && _selectedSegment != -1 && _spline.Path.Is2D)
        {
            Undo.RecordObject(_spline, "Insert Segment");
            _spline.Path.InsertSegment(_selectedSegment, _spline.transform.InverseTransformPoint(_mousePosOnPlane));

            if (_spline.custom.alwaysShowArrows && _spline.custom.useArrowDistanceDistribution)
                _spline.RecalculateArrowBuffer();
        }

        // Adds a segment at the mouse position (only for 2D)
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift && SceneView.lastActiveSceneView.in2DMode && _spline.Path.Is2D)
        {
            Undo.RecordObject(_spline, "Add Segment");
            _spline.Path.AddSegment(mousPos, Quaternion.identity);

            if ((_spline.isRotate || _spline.custom.alwaysShowArrows) && _spline.custom.useArrowDistanceDistribution)
                _spline.RecalculateArrowBuffer();
        }

        // Removes a segment at the mouse position
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

                if (_spline.custom.alwaysShowArrows && _spline.custom.useArrowDistanceDistribution)
                    _spline.RecalculateArrowBuffer();
            }
        }
    }

    /// <summary>
    /// Paints the spline on the scene
    /// </summary>
    /// <param name="isRotate">Is the spline in rotation mode</param>
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

    /// <summary>
    /// Paints the move mode in the editor
    /// </summary>
    /// <param name="pathInfo">Information about the path</param>
    private void PaintMove(PathInfo pathInfo)
    {
        for (int i = 0; i < pathInfo.points.Length; i++)
        {
            Handles.color = i % 3 == 0 ? _spline.custom.anchorColor : _spline.custom.controlColor;
            float size = i % 3 == 0 ? 0.1f : 0.075f;
            Vector3 newPos = Handles.FreeMoveHandle(pathInfo.points[i], size, Vector3.zero, Handles.CylinderHandleCap);

            if (newPos == pathInfo.points[i])
                continue;

            if (_spline.custom.alwaysShowArrows && _spline.custom.useArrowDistanceDistribution)
                _spline.RecalculateArrowBuffer();

            Undo.RecordObject(_spline, "Move Point Scene");
            _spline.Path.MovePoint(i, _spline.transform.InverseTransformPoint(newPos));

        }

        if (_spline.custom.alwaysShowArrows)
            PaintRotationArrows();
    }

    /// <summary>
    /// Paints the rotzation mode in the editor
    /// </summary>
    /// <param name="pathInfo">Information about the path</param>
    private void PaintRotation(PathInfo pathInfo)
    {
        Handles.color = _spline.custom.anchorColor;
        float size = 0.1f;
        for (int i = 0; i < pathInfo.rotations.Length; i++)
        {
            Handles.FreeMoveHandle(pathInfo.points[i * 3], size, Vector3.zero, Handles.CylinderHandleCap);

            Quaternion newQuat = Handles.RotationHandle(pathInfo.rotations[i], pathInfo.points[i * 3]);

            if (newQuat == pathInfo.rotations[i])
                continue;

            Undo.RecordObject(_spline, "Rotate Point Scene");
            _spline.Path.RotatePoint(i, Quaternion.Inverse(_spline.transform.rotation) * newQuat);
        }

        PaintRotationArrows();

    }

    /// <summary>
    /// Paints the rotation arrows on the spline
    /// </summary>
    private void PaintRotationArrows()
    {
        Camera cam = Camera.current;

        Handles.color = _spline.custom.arrowColor;
        if (_spline.custom.useArrowDistanceDistribution)
        {
            for (int i = 0; i < _spline.bufferedArrowDistribution.Count; i++)
            {
                float[] p = _spline.bufferedArrowDistribution[i];
                for (int j = 0; j < p.Length; j++)
                {
                    Quaternion rot = _spline.GetRotationWorld(i + p[j]);
                    Vector3 pos = _spline.GetPositionWorld(i + p[j]);
                    if ((cam.transform.position - pos).sqrMagnitude < 100)
                    {
                        Vector3 viewPos = cam.WorldToViewportPoint(pos);
                        if (viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1 || viewPos.z < 0)
                            continue;
                        Handles.ArrowHandleCap(i, pos, rot, _spline.custom.arrowLength, EventType.Repaint);
                    }
                }
            }
            return;
        }

        float arrowsDistribution = _spline.custom.arrowDistribution;
        for (int j = 0; j < _spline.Path.NumSegments; j++)
        {
            for (int i = 0; i < arrowsDistribution; i++)
            {
                Quaternion rot = _spline.GetRotationWorld(j + i / arrowsDistribution);
                Vector3 pos = _spline.GetPositionWorld(j + i / arrowsDistribution);
                if ((cam.transform.position - pos).sqrMagnitude < 100)
                {
                    Vector3 viewPos = cam.WorldToViewportPoint(pos);
                    if (viewPos.x < 0 || viewPos.x > 1 || viewPos.y < 0 || viewPos.y > 1 || viewPos.z < 0)
                        continue;
                    Handles.ArrowHandleCap(i, pos, rot, _spline.custom.arrowLength, EventType.Repaint);
                }
            }
        }
    }

    private float DistFromPoint(Ray r, Vector3 cPos)
    {
        float lambd = -((((r.origin.x - cPos.x) * r.direction.x) + ((r.origin.y - cPos.y) * r.direction.y) + ((r.origin.z - cPos.z) * r.direction.z)) / (Mathf.Pow(r.direction.x, 2) + Mathf.Pow(r.direction.y, 2) + Mathf.Pow(r.direction.z, 2)));
        float dist = Mathf.Sqrt(Mathf.Pow(r.origin.x + lambd * r.direction.x - cPos.x, 2) + Mathf.Pow(r.origin.y + lambd * r.direction.y - cPos.y, 2) + Mathf.Pow(r.origin.z + lambd * r.direction.z - cPos.z, 2));
        return dist;
    }

}
