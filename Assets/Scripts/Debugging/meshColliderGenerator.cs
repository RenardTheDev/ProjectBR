using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class meshColliderGenerator : MonoBehaviour
{
    public void Generate()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].gameObject.AddComponent<MeshCollider>().sharedMesh = renderers[i].GetComponent<MeshFilter>().mesh;
        }
    }

    public void Clear()
    {
        MeshCollider[] colls = GetComponentsInChildren<MeshCollider>();

        for (int i = 0; i < colls.Length; i++)
        {
            DestroyImmediate(colls[i]);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(meshColliderGenerator))]
public class meshColliderGeneratorEditor : Editor
{
    meshColliderGenerator script;
    public override void OnInspectorGUI()
    {
        script = (meshColliderGenerator)target;
        base.OnInspectorGUI();
        GUILayout.Space(8);

        if (GUILayout.Button("Generate"))
        {
            script.Generate();
        }

        if (GUILayout.Button("Clear"))
        {
            script.Clear();
        }
    }
}
#endif