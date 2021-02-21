using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LightSource))]
public class LightSourceEditor : Editor {
    private void OnEnable() {
        SceneView.duringSceneGui += OnSceneGUI;
    }
    private void OnDisable() {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (!EditorApplication.isPlaying) return;
        if (GUILayout.Button("Toggle active")) (target as LightSource).ToggleActive();
    }
    private void OnSceneGUI(SceneView obj) {
        var source = (target as LightSource);
        if (source.radius > 0)
            Handles.DrawWireDisc(source.transform.position, Vector3.back, source.radius);
    }
}
