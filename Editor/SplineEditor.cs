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
    SplineController spline;
    Path path;

    int selectedSegment = -1;
    Vector3 mousePosOnPlane = Vector3.zero;

    private void OnEnable()
    {
        spline = (SplineController)target;
        if (spline.path.IsNull)
            spline.CreatePath();
        path = spline.path;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        ChangeTo2D();
        ToggleMoveButton();
        if (spline.isRotate) DrawRotations(ref spline.rotationsFoldOut);
        else DrawPath(ref spline.pathFoldOut);
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
        Info(ref spline.infoFoldOut);

    }
    // temporary implementation
    private void DrawUpdateBezierDistButton()
    {
        EditorGUILayout.LabelField("Temporare");
        GUI.enabled = spline.custom.arrowDistributionByDistance;
        if (GUILayout.Button("Recalculate Bezier Distances"))
        {
            path.RecalculateDistancesT(spline.custom.arrowDistance, spline.custom.arrowResolution);
        }
        GUI.enabled = true;
    }

    private void ChangeTo2D()
    {
        Undo.RecordObject(spline, "Dimenson Change");
        bool change = EditorGUILayout.Toggle("2D Spline", path.Is2D);
        if (change == path.Is2D)
            return;

        bool proceed = EditorUtility.DisplayDialog("Change Dimension?", $"Change spline from {(change ? "3D" : "2D")} to {(change ? "2D" : "3D")}", "Preceed", "Cancel");
        if (proceed)
            path.Is2D = change;
    }

    private void DrawIsClosed()
    {
        GUI.enabled = false;
        path.IsClosed = EditorGUILayout.Toggle("Is Closed", path.IsClosed);
        GUI.enabled = true;
    }

    private void AddSegmentButton()
    {
        if (GUILayout.Button("Add Segment"))
        {
            Undo.RecordObject(spline, "Button Add Segment");
            path.AddSegment();
            SceneView.RepaintAll();
        }
    }

    private void ToggleMoveButton()
    {
        if (GUILayout.Button($"{(spline.isRotate ? "Toggle to move" : "Toggle to rotate")}"))
        {
            spline.isRotate = !spline.isRotate;
            SceneView.RepaintAll();
        }
    }

    private void ClosePath()
    {
        if (GUILayout.Button("Toggle Close"))
        {
            Undo.RecordObject(spline, "Toggle Close");
            path.ToggleClosed();
            SceneView.RepaintAll();
        }
    }

    private void LoadCheckpoint()
    {
        if (GUILayout.Button("Load Checkpoint"))
        {
            Undo.RecordObject(spline, "Load Checkpoint");
            bool proceed = EditorUtility.DisplayDialog("LOAD CHECKPOINT", $"Load last checkpoint ({path.checkpoints.Count})?", "Preceed", "Cancel");
            if (proceed)
                path.LoadLastCheckpoint();

            SceneView.RepaintAll();
        }
    }

    private void CreateCheckpoint()
    {
        if (GUILayout.Button("Create Checkpoint"))
        {
            Undo.RecordObject(spline, "Create Checkpoint");
            path.CreateCheckpoint();
            EditorUtility.DisplayDialog("Checkpoint", $"Checkpoint {path.checkpoints.Count} created", "Ok", null);
        }
    }

    private void DeleteCheckpoint()
    {
        if (GUILayout.Button("Delete Checkpoint"))
        {
            if (path.checkpoints.Count - 1 <= 0)
                EditorUtility.DisplayDialog("No deletion", "Can not delete root checkpoint!", "Ok", null);
            else
            {
                Undo.RecordObject(spline, "Delete Checkpoint");
                bool proceed = EditorUtility.DisplayDialog("DELETE CHECKPOINT", $"Delete Checkpoint {path.checkpoints.Count}!", "Proceed", "Cancel");
                if (proceed)
                    path.DeleteCheckpoint();
            }
        }
    }

    private void DrawPath(ref bool foldOut)
    {
        foldOut = EditorGUILayout.Foldout(foldOut, "Path");
        if (!foldOut)
            return;
        EditorGUI.indentLevel++;
        for (int i = 0; i < path.NumPoints; i++)
        {
            path.MovePoint(i, EditorGUILayout.Vector3Field($"{(i % 3 == 0 ? $"Anchor {i / 3}" : $"Con {i}")}", path[i]));
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
        for (int i = 0; i < path.NumRotations; i++)
        {
            Vector3 rot = EditorGUILayout.Vector3Field($"Rot {i}", path.GetRotation(i).eulerAngles);
            path.RotatePoint(i, Quaternion.Euler(new Vector3((float)Math.Round(rot.x, 2), (float)Math.Round(rot.y, 2), (float)Math.Round(rot.z, 2))));
        }
        EditorGUI.indentLevel--;
        SceneView.RepaintAll();
    }

    private void DrawSplineCustomizer()
    {
        spline.customizerFoldOut = EditorGUILayout.Foldout(spline.customizerFoldOut, "Customizer");
        if (!spline.customizerFoldOut)
            return;

        EditorGUI.indentLevel++;
        spline.custom.splineColor = EditorGUILayout.ColorField("Spline Color", spline.custom.splineColor);
        spline.custom.selectedColor = EditorGUILayout.ColorField("Selected Color", spline.custom.selectedColor);
        spline.custom.connectionColor = EditorGUILayout.ColorField("Connection Color", spline.custom.connectionColor);

        spline.custom.anchorColor = EditorGUILayout.ColorField("Anchor Color", spline.custom.anchorColor);
        spline.custom.controlColor = EditorGUILayout.ColorField("Control Color", spline.custom.controlColor);

        spline.custom.arrowColor = EditorGUILayout.ColorField("Arrow Color", spline.custom.arrowColor);

        EditorGUILayout.Space();

        spline.custom.arrowDistributionByDistance = EditorGUILayout.Toggle("Arrow Distribution By Distance", spline.custom.arrowDistributionByDistance);
        bool isDist = spline.custom.arrowDistributionByDistance;

        spline.custom.arrowLength = EditorGUILayout.Slider("Arrow Length", spline.custom.arrowLength, 0, 1f);
        if (isDist)
        {
            spline.custom.arrowResolution = EditorGUILayout.IntSlider("Resolution", spline.custom.arrowResolution, 1, 10000);
            spline.custom.arrowDistance = EditorGUILayout.Slider("Distance", spline.custom.arrowDistance, 0f, 1f);
        }
        else
        {
            spline.custom.arrowDistribution = EditorGUILayout.IntSlider("Arrow Distribution", spline.custom.arrowDistribution, 1, 250);
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
        DrawCheckpoints(ref spline.checkpointsFoldOut);
        EditorGUI.indentLevel--;

    }

    private void DrawCheckpoints(ref bool checkpointsFoldOut)
    {
        checkpointsFoldOut = EditorGUILayout.Foldout(checkpointsFoldOut, "Checkpoints");
        if (!checkpointsFoldOut)
            return;


        EditorGUI.indentLevel++;
        for (int i = 0; i < path.checkpoints.Count; i++)
        {
            path.checkpoints[i].foldOut = EditorGUILayout.Foldout(path.checkpoints[i].foldOut, $"Checkpoint {i + 1}");
            if (!path.checkpoints[i].foldOut)
                continue;
            GUI.enabled = false;
            EditorGUILayout.Toggle("Is Closed", path.checkpoints[i].isClosed);
            EditorGUILayout.Toggle("Is 2D", path.checkpoints[i].is2D);
            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Points", EditorStyles.boldLabel);

            GUI.enabled = false;
            for (int j = 0; j < path.checkpoints[i].points.Count; j++)
            {
                EditorGUILayout.Vector3Field($"{(j % 3 == 0 ? $"Anchor {j / 3}" : $"Con {j}")}", path.checkpoints[i].points[j]);
            }
            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rotations", EditorStyles.boldLabel);

            GUI.enabled = false;
            for (int j = 0; j < path.checkpoints[i].rotations.Count; j++)
            {
                EditorGUILayout.Vector3Field($"Rot {j}", path.checkpoints[i].rotations[j].eulerAngles);
            }
            GUI.enabled = true;
        }
        EditorGUI.indentLevel--;
    }

    private void OnSceneGUI()
    {
        Matrix4x4 originalMatrix = Handles.matrix;

        Handles.matrix = spline.transform.localToWorldMatrix;
        Input();
        Draw(spline.isRotate);
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

            for (int i = 0; i < path.NumSegments; i++)
            {
                Vector3[] loaclPoints = path.GetPointsInSegment(i);
                Vector3[] points = new Vector3[4];
                for (int j = 0; j < points.Length; j++)
                {
                    points[j] = spline.transform.TransformPoint(loaclPoints[j]);
                }

                Vector3 p1TOp2 = points[3] - points[0];
                Vector3 planeNormal = Vector3.Cross(p1TOp2, spline.transform.up).normalized;

                float dot = Vector3.Dot(r.direction, planeNormal);
                if (dot <= 0.15f && dot >= -0.15f)
                    continue;


                float distToIntersection = Vector3.Dot(points[0] - r.origin, planeNormal) / dot;
                if (distToIntersection <= 0)
                    continue;

                Vector3 pointOnPlane = r.origin + distToIntersection * r.direction;
                mousePosOnPlane = pointOnPlane;

                float dist = HandleUtility.DistancePointBezier(pointOnPlane, points[0], points[3], points[1], points[2]);

                if (dist >= nearestDist)
                    continue;

                nearestDist = dist;
                nearestSegment = i;
            }
            if (nearestSegment != selectedSegment)
            {
                selectedSegment = nearestSegment;
                HandleUtility.Repaint();
            }
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.control && selectedSegment != -1)
        {
            Undo.RecordObject(spline, "Insert Segment");
            path.InsertSegment(selectedSegment, mousePosOnPlane);
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift && SceneView.lastActiveSceneView.in2DMode)
        {
            Undo.RecordObject(spline, "Add Segment");
            path.AddSegment(mousPos, Quaternion.identity);
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
        {
            float distToAnchor = 0.05f;
            int closestAnchorIndex = -1;
            for (int i = 0; i < path.NumPoints; i++)
            {
                float dist = DistFromCircle(r, path[i]);
                if (dist < distToAnchor)
                {
                    distToAnchor = dist;
                    closestAnchorIndex = i;
                }
            }
            if (closestAnchorIndex != -1)
            {
                Undo.RecordObject(spline, "Remove Segment");
                path.DeletePoint(closestAnchorIndex);
            }
        }
    }

    private void Draw(bool isRotate)
    {
        for (int i = 0; i < path.NumSegments; i++)
        {
            Vector3[] points = path.GetPointsInSegment(i);
            if (!isRotate)
            {
                Handles.color = spline.custom.connectionColor;
                Handles.DrawLine(points[0], points[1]);
                Handles.DrawLine(points[2], points[3]);
            }
            Color color = selectedSegment == i ? spline.custom.selectedColor : spline.custom.splineColor;
            Handles.DrawBezier(points[0], points[3], points[1], points[2], color, null, 2);

            if (isRotate)
                DrawRotation();
            else
                DrawMove();

        }

    }

    private void DrawMove()
    {
        for (int i = 0; i < path.NumPoints; i++)
        {
            Handles.color = i % 3 == 0 ? spline.custom.anchorColor : spline.custom.controlColor;
            float size = i % 3 == 0 ? 0.1f : 0.075f;
            Vector3 newPos = Handles.FreeMoveHandle(path[i], size, Vector3.zero, Handles.CylinderHandleCap);

            if (newPos == path[i])
                continue;

            Undo.RecordObject(spline, "Move Point");
            path.MovePoint(i, newPos);
        }
    }

    private void DrawRotation()
    {
        for (int i = 0; i < path.NumRotations; i++)
        {
            float size = 0.1f;
            Handles.color = spline.custom.anchorColor;
            Handles.FreeMoveHandle(path[i * 3], size, Vector3.zero, Handles.CylinderHandleCap);
            Quaternion newQuat = Handles.RotationHandle(path.GetRotation(i), path[i * 3]);

            Undo.RecordObject(spline, "Rotate Point");
            path.RotatePoint(i, newQuat);
        }

        float arrowsDistribution = spline.custom.arrowDistribution;
        Handles.color = spline.custom.arrowColor;

        if (spline.custom.arrowDistributionByDistance)
        {
            for (int i = 0; i < path.NumDistancesT; i++)
            {
                float[] distancesT = path.GetDistancesT(i);
                for (int j = 0; j < distancesT.Length; j++)
                {
                    Quaternion rot = spline.CalculateRotation(i + distancesT[j]);
                    Vector3 pos = spline.CalculatePosition(i + distancesT[j]);
                    Handles.ArrowHandleCap(i, pos, rot, spline.custom.arrowLength, EventType.Repaint);
                }
            }
        }
        else
        {
            for (int j = 0; j < path.NumSegments; j++)
            {
                for (int i = 0; i < arrowsDistribution; i++)
                {
                    Quaternion rot = spline.CalculateRotation(j + i / arrowsDistribution);
                    Vector3 pos = spline.CalculatePosition(j + i / arrowsDistribution);
                    Handles.ArrowHandleCap(i, pos, rot, spline.custom.arrowLength, EventType.Repaint);
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
