using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MovingPlatform))]
public class MovingPlatformEditor : Editor {
    MovingPlatform movingPlatform;
    bool moveRel = true;
    Vector3 lastPos = Vector3.zero;

    void OnEnable() {
        movingPlatform = (MovingPlatform) target;
        if (movingPlatform.path.Length == 0) {
            movingPlatform.path = new Vector3[] { movingPlatform.transform.position };
        }
        lastPos = movingPlatform.transform.position;
    }
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        EditorGUI.BeginChangeCheck();
        if (GUILayout.Button("Add Point")) {
            Undo.RecordObject(movingPlatform, "Add point");
            List<Vector3> vl = new List<Vector3>(movingPlatform.path);
            vl.Add(vl[vl.Count - 1] + Vector3.forward * 2);
            movingPlatform.path = vl.ToArray();
        }
        if (GUILayout.Button("Remove Point")) {
            if (movingPlatform.path.Length > 1) {
                Undo.RecordObject(movingPlatform, "Remove point");
                List<Vector3> vl = new List<Vector3>(movingPlatform.path);
                vl.RemoveAt(vl.Count - 1);
                movingPlatform.path = vl.ToArray();
            }
        }
        if (GUILayout.Button("Recenter Points")) {
            if (movingPlatform.path.Length > 1) {
                Undo.RecordObject(movingPlatform, "Recenter points");
                var dpos = movingPlatform.path[0] - movingPlatform.transform.position;
                for (int i = 0; i < movingPlatform.path.Length; i++) {
                    movingPlatform.path[i] -= dpos;
                }
            }
        }
        // bool guiClosed = GUILayout.Toggle(movingPlatform.isOpen, "Open Loop");
        var wasmoverel = moveRel;
        moveRel = GUILayout.Toggle(moveRel, "Relative Movement");
        if (moveRel && !wasmoverel) {
            lastPos = movingPlatform.transform.position;
            // movingPlatform.path[0] = movingPlatform.transform.position;
        }
        if (EditorGUI.EndChangeCheck()) {
            SceneView.RepaintAll();
        }
    }
    void OnSceneGUI() {
        if (movingPlatform.path.Length == 0) {
            return;
        }
        // if (movingPlatform.path.Length > 1) {
        //     Handles.color = Color.black;
        //     Handles.DrawDottedLines(movingPlatform.path, 10);
        // }
        // Vector3 gpos = movingPlatform.transform.position;
        Quaternion grot = movingPlatform.transform.rotation;
        for (int i = 1; i < movingPlatform.path.Length; i++) {
            Vector3 pos = movingPlatform.path[i];
            // var npos = Handles.FreeMoveHandle(pos, grot,0.2f,Vector3.zero,Handles.SphereHandleCap);
            var npos = Handles.PositionHandle(pos, grot);
            if (npos != pos) {
                Undo.RecordObject(movingPlatform, "Move point");
                movingPlatform.path[i] = npos;
            }
        }
        if (moveRel && !Application.isPlaying) {
            var newPos = movingPlatform.transform.position;
            if (newPos != lastPos) {
                // move relative
                var dpos = newPos - lastPos;
                for (int i = 0; i < movingPlatform.path.Length; i++) {
                    movingPlatform.path[i] += dpos;
                }
                lastPos = newPos;
            }
        }
    }

    [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.NonSelected)]
    static void DrawGizmo(MovingPlatform mp, GizmoType gizmoType) {
        float rad = 0.2f;
        // if (gizmoType == GizmoType.Active) {
        Gizmos.color = Color.red;
        // } else if (gizmoType == GizmoType.Selected) {
        //     Gizmos.color = Color.black;
        // } else {
        //     Gizmos.color = Color.gray;
        //     rad = 0.1f;
        // }
        for (int i = 0; i < mp.path.Length; i++) {
            Vector3 pos = mp.path[i];
            Gizmos.DrawSphere(pos, rad);
            if (i < mp.path.Length - 1) {
                Gizmos.DrawLine(pos, mp.path[i + 1]);
                // Debug.Log("Line");
            }
        }
        if (!mp.isOpen) {
            Gizmos.DrawLine(mp.path[0], mp.path[mp.path.Length - 1]);
        }
    }
}