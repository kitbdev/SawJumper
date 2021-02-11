using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Moving Platform Editor for both moving platforms and sawblades
/// makes it much easier to create and edit paths in the editor
/// shows the path in 3D in the scene view
/// shows important statistics about the path, 
/// such as the distance, speed, percent completion, and duration for each point on the path
/// adds moveable path points to the scene view
/// handles rotation, global orientation, and move tool in scene
/// adds buttons, values, and toggles to the inspector for path control
/// Add and Remove point buttons, snap points to grid Button
/// Physic Bounce Simulation of Path
/// full Undo system support
/// </summary>
[SelectionBase]
[CanEditMultipleObjects]
[CustomEditor(typeof(MovingPlatform))]
public class MovingPlatformEditor : Editor {
    MovingPlatform movingPlatform;
    bool moveRel = true;
    bool rotRel = false;
    Vector3 lastPos = Vector3.zero;
    Quaternion lastRot = Quaternion.identity;
    int physBounces = 2;
    float physRad = 0.5f;
    bool showDistances = true;

    void OnEnable() {
        movingPlatform = (MovingPlatform) target;
        if (movingPlatform.path.Count == 0) {
            movingPlatform.path = new List<Vector3>() { Vector3.zero };
        }
        lastPos = movingPlatform.transform.position;
        lastRot = movingPlatform.transform.rotation;
    }
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (targets.Length > 1) {
            GUILayout.Label("Select only one for more options");
            return;
        }
        EditorGUI.BeginChangeCheck();

        /// Add and Remove Points
        if (GUILayout.Button("Add Point")) {
            Undo.RecordObject(movingPlatform, "Add point");
            movingPlatform.path.Add(movingPlatform.path[movingPlatform.path.Count - 1] + Vector3.forward * 2);
        }
        if (GUILayout.Button("Add Point up")) {
            Undo.RecordObject(movingPlatform, "Add point up");
            movingPlatform.path.Add(movingPlatform.path[movingPlatform.path.Count - 1] + Vector3.up * 2);
        }
        if (GUILayout.Button("Remove Point")) {
            if (movingPlatform.path.Count > 1) {
                Undo.RecordObject(movingPlatform, "Remove point");
                movingPlatform.path.RemoveAt(movingPlatform.path.Count - 1);
            }
        }
        // if (GUILayout.Button("Recenter Points")) {
        //     if (movingPlatform.path.Count > 1) {
        //         Undo.RecordObject(movingPlatform, "Recenter points");
        //         var dpos = movingPlatform.transform.position;
        //         for (int i = 0; i < movingPlatform.path.Count; i++) {
        //             movingPlatform.path[i] -= dpos;
        //         }
        //     }
        // }

        /// Snapping to nearest full grid unit (1m)
        if (GUILayout.Button("Snap Points")) {
            if (movingPlatform.path.Count > 1) {
                Undo.RecordObject(movingPlatform, "Snap points");
                movingPlatform.path[0].Set(0, 0, 0);
                for (int i = 0; i < movingPlatform.path.Count; i++) {
                    var pos = movingPlatform.path[i];
                    if (!moveRel) {
                        pos = movingPlatform.transform.TransformPoint(pos);
                    }
                    pos = new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), Mathf.Round(pos.z));
                    if (!moveRel) {
                        pos = movingPlatform.transform.InverseTransformPoint(pos);
                    }
                    movingPlatform.path[i] = pos;
                }
            }
        }
        
        /// Physics Bounce Simulation settings
        var wasphysBounces = physBounces;
        physBounces = EditorGUILayout.IntField("Physics Sim Bounces", physBounces);
        physRad = EditorGUILayout.FloatField("Phys Radius", physRad);
        // if (physBounces != wasphysBounces) {
        // }
        if (physBounces > 0 && GUILayout.Button("Physics Sim")) {
            RecalcPhys();
        }

        /// Display settings
        // bool guiClosed = GUILayout.Toggle(movingPlatform.isOpen, "Open Loop");
        showDistances = GUILayout.Toggle(showDistances, "Show Distances");

        /// Relative to anchor point toggles for movement and rotation
        var wasmoverel = moveRel;
        moveRel = GUILayout.Toggle(moveRel, "Relative Movement");
        if (moveRel && !wasmoverel) {
            lastPos = movingPlatform.transform.position;
            // lastRot = movingPlatform.transform.rotation;
            // movingPlatform.path[0] = movingPlatform.transform.position;
        }
        var wasrotrel = rotRel;
        rotRel = GUILayout.Toggle(rotRel, "Relative rotment");
        if (rotRel && !wasrotrel) {
            // lastPos = movingPlatform.transform.position;
            lastRot = movingPlatform.transform.rotation;
            // movingPlatform.path[0] = movingPlatform.transform.position;
        }
        if (EditorGUI.EndChangeCheck()) {
            SceneView.RepaintAll();
        }
    }
    void OnSceneGUI() {
        if (Application.isPlaying || movingPlatform.path.Count == 0) {
            return;
        }
        movingPlatform = (MovingPlatform) target;
        // if (movingPlatform.path.Count > 1) {
        //     Handles.color = Color.black;
        //     Handles.DrawDottedLines(movingPlatform.path, 10);
        // }

        /// Change based on current Tool selection
        Vector3 gpos = movingPlatform.transform.position;
        Quaternion grot = movingPlatform.transform.rotation;
        if (Tools.pivotRotation == PivotRotation.Global) {
            grot = Quaternion.identity;
        }
        if (Tools.current == Tool.Move) {
            /// position handles for all path points
            for (int i = 1; i < movingPlatform.path.Count; i++) {
                Vector3 pos = movingPlatform.path[i];
                pos = movingPlatform.transform.TransformPoint(pos);
                // var npos = Handles.FreeMoveHandle(pos, grot,0.2f,Vector3.zero,Handles.SphereHandleCap);
                var npos = Handles.PositionHandle(pos, grot);
                if (npos != pos) {
                    Undo.RecordObject(movingPlatform, "Move point");
                    movingPlatform.path[i] = movingPlatform.transform.InverseTransformPoint(npos);
                }
            }
        }

        /// Show useful stats
        /// including for each point:
        /// the distance it needs to travel from the last point
        /// the percent along the path this point is
        /// the inverse of the percent (useful for coordinating with other moving platforms)
        /// the time it takes for the moving platform to get to this point
        /// and for the initial point only:
        /// the total distance and duration the path takes to loop
        /// the speed of the moving platform
        if (showDistances) {
            float totalDist = 0;
            for (int i = 1; i < movingPlatform.path.Count; i++) {
                Vector3 pos = movingPlatform.path[i];
                Vector3 prevpos = movingPlatform.path[i - 1];
                float dist = Vector3.Distance(prevpos, pos);
                totalDist += dist;
            }
            if (!movingPlatform.isOpen && movingPlatform.path.Count > 2) {
                totalDist += Vector3.Distance(movingPlatform.path[0], movingPlatform.path[movingPlatform.path.Count - 1]);
            }
            float runDist = 0;
            for (int i = 1; i < movingPlatform.path.Count; i++) {
                Vector3 pos = movingPlatform.path[i];
                Vector3 prevpos = movingPlatform.path[i - 1];
                float dist = Vector3.Distance(prevpos, pos);
                runDist += dist;
                float perc = runDist / totalDist;
                float time = movingPlatform.duration * perc;
                string label = $"{dist:F3}m\n{perc:F3}%-{1-perc:F3}%\n{time:F3}s";
                // string label = dist + "m \n" + (perc) + "% \n" + time + " s";
                Vector3 labelPos = movingPlatform.transform.TransformPoint(pos);
                // labelPos += Vector3.up * 0.5f;
                Handles.Label(labelPos, label);
            }
            Vector3 labelPos0 = movingPlatform.transform.TransformPoint(movingPlatform.path[0]);
            // labelPos0 += Vector3.up * 0.5f;
            Handles.Label(labelPos0, $"0m/{totalDist}m\n{totalDist/movingPlatform.duration:F3}m/s\n0s/{movingPlatform.duration}s");
        }
        // show phys bounce preview?
        // if (physBounces>0 && !Application.isPlaying) {
        // }

        /// logic for non relative movement and rotation
        if (!moveRel && !Application.isPlaying) {
            var newPos = movingPlatform.transform.position;
            if (newPos != lastPos) {
                // move unrelative
                var dpos = newPos - lastPos;
                for (int i = 0; i < movingPlatform.path.Count; i++) {
                    movingPlatform.path[i] -= dpos;
                }
                lastPos = newPos;
            }
        }
        if (!rotRel && !Application.isPlaying) {
            var newRot = movingPlatform.transform.rotation;
            if (newRot != lastRot) {
                // turn unrelative
                var drot = Quaternion.Inverse(newRot) * lastRot;
                for (int i = 1; i < movingPlatform.path.Count; i++) {
                    var p = movingPlatform.path[i];
                    p = drot * p;
                    movingPlatform.path[i] = p;
                }
                lastRot = newRot;
            }
        }
    }

    /// <summary>
    /// Performs a simple bouncing simulation
    /// follows the current forward direction
    /// once an environment wall is collided with, bounce off at the correct reflection angle
    /// limited by number of bounces set in inspector
    /// replaces the current path with generated points
    /// stops if no environment wall collided with or the point matches another one exactly
    /// </summary>
    void RecalcPhys() {
        Undo.RecordObject(movingPlatform, "Recalc Phys");
        movingPlatform.path.Clear();
        movingPlatform.path.Add(Vector3.zero);
        RaycastHit hit;
        int i = 1;
        Vector3 rayPos = movingPlatform.transform.position;
        Vector3 rayDir = movingPlatform.transform.forward;
        while (i < physBounces + 1) {
            if (Physics.Raycast(rayPos, rayDir, out hit, 100, LayerMask.NameToLayer("enviro"))) {
                var npos = hit.point + physRad * hit.normal;
                if (movingPlatform.path.IndexOf(npos) > 0) {
                    break;
                }
                movingPlatform.path.Add(movingPlatform.transform.InverseTransformPoint(npos));
                rayPos = npos;
                rayDir = Vector3.Reflect(rayDir, hit.normal);
                i++;
            } else {
                break;
            }
        }
    }

    /// <summary>
    /// Draw gizmos in sceneview to show path, even when not currently editing
    /// </summary>
    /// <param name="mp"></param>
    /// <param name="gizmoType"></param>
    [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.NonSelected)]
    static void DrawGizmo(MovingPlatform mp, GizmoType gizmoType) {
        float rad = 0.3f;
        // if (gizmoType == GizmoType.Active) {
        Gizmos.color = Color.red;
        // } else if (gizmoType == GizmoType.Selected) {
        //     Gizmos.color = Color.black;
        // Debug.Log(mp.name + " " + gizmoType.ToString());
        if ((gizmoType & GizmoType.NonSelected) != 0) {
            Gizmos.color = Color.gray;
            rad = 0.2f;
        }
        for (int i = 0; i < mp.path.Count; i++) {
            // point position
            Vector3 pos = mp.path[i];
            pos = mp.transform.TransformPoint(pos);
            Gizmos.DrawSphere(pos, rad);
            if (i < mp.path.Count - 1) {
                // line to next point
                Vector3 toPos = mp.path[i + 1];
                toPos = mp.transform.TransformPoint(toPos);
                Gizmos.DrawLine(pos, toPos);
                // Debug.Log("Line");
            }
        }
        // todo show start point as well?
        // path open or closed loop
        if (!mp.isOpen && mp.path.Count > 1) {
            var from = mp.transform.TransformPoint(mp.path[0]);
            var to = mp.transform.TransformPoint(mp.path[mp.path.Count - 1]);
            Gizmos.DrawLine(from, to);
        }
        // rotation help
        if (mp.spinEuler != Vector3.zero) {
            Gizmos.DrawWireSphere(mp.transform.position, 1);
        }
    }
}