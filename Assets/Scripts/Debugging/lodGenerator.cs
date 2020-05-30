using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class lodGenerator : MonoBehaviour
{
    Transform child;
    LOD lod;
    public float screenHeight = 0.1f;
    public void GenerateLODs()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            //child = transform.GetChild(i);
            lod = new LOD(screenHeight, new[] { renderers[i] });
            renderers[i].gameObject.AddComponent<LODGroup>().SetLODs(new[] { lod });
        }
    }

    public void ClearLODs()
    {
        LODGroup[] lods = GetComponentsInChildren<LODGroup>();

        for (int i = 0; i < lods.Length; i++)
        {
            DestroyImmediate(lods[i]);
        }
    }

    public void ClearModels()
    {
        MeshFilter[] mfs = GetComponentsInChildren<MeshFilter>();

        for (int i = 0; i < mfs.Length; i++)
        {
            if (mfs[i].mesh == null)
            {
                DestroyImmediate(mfs[i].gameObject);
            }
            else
            {
                if (mfs[i].mesh.vertexCount == 0) DestroyImmediate(mfs[i].gameObject);
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(lodGenerator))]
public class lodGeneratorEditor : Editor
{
    lodGenerator script;
    public override void OnInspectorGUI()
    {
        script = (lodGenerator)target;
        base.OnInspectorGUI();
        GUILayout.Space(8);

        if (GUILayout.Button("Generate"))
        {
            script.GenerateLODs();
        }

        if (GUILayout.Button("Clear"))
        {
            script.ClearLODs();
        }

        if (GUILayout.Button("Clear empty models"))
        {
            script.ClearModels();
        }
    }
}
#endif