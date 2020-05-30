using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SceneViewExtender : MonoBehaviour { }

[CustomEditor(typeof(SceneViewExtender))]
public class SceneViewExtenderEditor : Editor
{
    private void OnSceneGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Label("Test label in scene view");
    }
}