using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ChildCounter : MonoBehaviour
{
    public string ActualName;

    public void Rename()
    {
        gameObject.name = ActualName + " [" + transform.childCount + "]";
    }

    public void ResetName()
    {
        gameObject.name = ActualName;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ChildCounter))]
public class ChildCounterEditor : Editor
{
    ChildCounter script;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        script = (ChildCounter)target;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Rename"))
        {
            script.Rename();
        }
        GUILayout.Space(8);
        if (GUILayout.Button("Reset"))
        {
            script.ResetName();
        }
        GUILayout.Space(8);
        if (GUILayout.Button("Delete children"))
        {
            List<GameObject> children = new List<GameObject>();

            for (int i = 0; i < script.transform.childCount; i++)
            {
                children.Add(script.transform.GetChild(i).gameObject);
            }

            foreach (var item in children)
            {
                DestroyImmediate(item, false);
            }
        }
        GUILayout.EndHorizontal();
    }
}

#endif