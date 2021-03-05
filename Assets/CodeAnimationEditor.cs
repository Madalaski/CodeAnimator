using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(CodeAnimationHandler))]
public class CodeAnimationEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        CodeAnimationHandler handler = (CodeAnimationHandler)target;

        if (GUILayout.Button("Populate From File")) {
            handler.ReadFromFile();
        }
    }
}