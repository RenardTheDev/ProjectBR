using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MeshOptimizationBase : MonoBehaviour
{
    private void Awake()
    {
        
    }

    List<Renderer> rends;
    Dictionary<Material, int> doubles;
    bool haveDoubles;
    MaterialDoubles md;
    public void FindMaterialDoubles()
    {
        rends = new List<Renderer>(GetComponentsInChildren<Renderer>());

        for (int r = 0; r < rends.Count; r++)
        {
            haveDoubles = false;

            doubles = new Dictionary<Material, int>();

            for (int m = 0; m < rends[r].sharedMaterials.Length; m++)
            {
                if (doubles.ContainsKey(rends[r].sharedMaterials[m]))
                {
                    haveDoubles = true;
                    doubles[rends[r].sharedMaterials[m]]++;
                }
                else
                {
                    doubles.Add(rends[r].sharedMaterials[m], 1);
                }
            }

            if (haveDoubles)
            {
                md = rends[r].gameObject.AddComponent<MaterialDoubles>();
                md.doubles = new Dictionary<Material, int>();

                foreach (var item in doubles)
                {
                    if (item.Value > 1) md.doubles.Add(item.Key, item.Value);
                }
            }
        }
    }

    public void RemoveMDComponents()
    {
        var mds = GetComponentsInChildren<MaterialDoubles>();
        foreach (var item in mds)
        {
            DestroyImmediate(item);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MeshOptimizationBase))]
public class MeshOptimizationBaseEditor : Editor
{
    MeshOptimizationBase script;

    public override void OnInspectorGUI()
    {
        if (script == null) script = (MeshOptimizationBase)target;
        base.OnInspectorGUI();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("FindMaterialDoubles"))
        {
            script.FindMaterialDoubles();
        }
        if (GUILayout.Button("Delete md components"))
        {
            script.RemoveMDComponents();
        }
        GUILayout.EndHorizontal();

        EditorUtility.SetDirty(script);
    }
}
#endif