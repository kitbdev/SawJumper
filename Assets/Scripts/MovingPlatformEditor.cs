using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
        if (GUILayout.Button("Snap Points")) {
            if (movingPlatform.path.Count > 1) {
                Undo.RecordObject(movingPlatform, "Snap points");
                movingPlatform.path[0].Set(0,0,0);
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
        var wasphysBounces = physBounces;
        // EditorGUILayout
        physBounces = EditorGUILayout.IntField("Physics Sim Bounces", physBounces);
        physRad = EditorGUILayout.FloatField("Phys Radius", physRad);
        // if (physBounces != wasphysBounces) {
             
        // }
        if (physBounces > 0 && GUILayout.Button("Physics Sim")) {
            RecalcPhys();
        }
        // bool guiClosed = GUILayout.Toggle(movingPlatform.isOpen, "Open Loop");
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
        Vector3 gpos = movingPlatform.transform.position;
        Quaternion grot = movingPlatform.transform.rotation;
        if (Tools.pivotRotation == PivotRotation.Global) {
            grot = Quaternion.identity;
        }
        if (Tools.current == Tool.Move) {
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
        // if (physBounces>0 && !Application.isPlaying) {

        // }
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

    [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.NonSelected)]
    static void DrawGizmo(MovingPlatform mp, GizmoType gizmoType) {
        float rad = 0.3f;
        // if (gizmoType == GizmoType.Active) {
        Gizmos.color = Color.red;
        // } else if (gizmoType == GizmoType.Selected) {
        //     Gizmos.color = Color.black;
        // Debug.Log(mp.name + " " + gizmoType.ToString());
        if ((gizmoType & GizmoType.NonSelected) != 0) {
            Gizmos.color = Color.black;
            rad = 0.2f;
        }
        for (int i = 0; i < mp.path.Count; i++) {
            Vector3 pos = mp.path[i];
            pos = mp.transform.TransformPoint(pos);
            Gizmos.DrawSphere(pos, rad);
            if (i < mp.path.Count - 1) {
                Vector3 toPos = mp.path[i + 1];
                toPos = mp.transform.TransformPoint(toPos);
                Gizmos.DrawLine(pos, toPos);
                // Debug.Log("Line");
            }
        }
        if (!mp.isOpen && mp.path.Count > 1) {
            var from = mp.transform.TransformPoint(mp.path[0]);
            var to = mp.transform.TransformPoint(mp.path[mp.path.Count - 1]);
            Gizmos.DrawLine(from, to);
        }
        if (mp.spinEuler!=Vector3.zero) {
            Gizmos.DrawWireSphere(mp.transform.position, 1);
        }
    }
}