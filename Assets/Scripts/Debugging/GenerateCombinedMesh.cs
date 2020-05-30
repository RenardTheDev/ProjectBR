using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GenerateCombinedMesh : MonoBehaviour
{
    List<MeshFilter> mFilters = new List<MeshFilter>();

    public void CombineMeshes()
    {
        GetMeshes();

        GameObject newCombinedMesh = new GameObject("Combined " + name, new[] { typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider) });
        newCombinedMesh.transform.parent = transform.parent; 
        //newCombinedMesh.transform.localPosition = transform.localPosition;

        MeshFilter cMeshFilter = newCombinedMesh.GetComponent<MeshFilter>();
        //MeshRenderer cMeshRenderer = newCombinedMesh.GetComponent<MeshRenderer>();
        //MeshCollider cMeshCollider = newCombinedMesh.GetComponent<MeshCollider>();

        Mesh visibleMesh = new Mesh();
        visibleMesh.name = "VisibleCombinedMesh";
        /*Mesh colliderMesh = new Mesh();
        colliderMesh.name = "ColliderCombinedMesh";*/

        List<CombineInstance> visCombine = new List<CombineInstance>();
        //List<Material> materials = new List<Material>();
        //List<CombineInstance> collCombine = new List<CombineInstance>();

        for (int i = 0; i < mFilters.Count; i++)
        {
            CombineInstance nc = new CombineInstance();
            nc.mesh = mFilters[i].sharedMesh;
            nc.transform = mFilters[i].transform.localToWorldMatrix;

            visCombine.Add(nc);
            //materials.AddRange(mFilters[i].GetComponent<MeshRenderer>().sharedMaterials);

            /*if (mFilters[i].GetComponent<MeshCollider>() != null)
            {
                nc.mesh = mFilters[i].sharedMesh;
                collCombine.Add(nc);
            }*/
        }

        Debug.Log("visCombine.Count = " + visCombine.Count /*+ "\n" +
            "materials.Count = " + materials.Count*/);

        visibleMesh.CombineMeshes(visCombine.ToArray(), false, true);

        /*visibleMesh.RecalculateBounds();
        visibleMesh.RecalculateTangents();
        visibleMesh.RecalculateNormals();*/

        //visibleMesh.Optimize();
        //colliderMesh.CombineMeshes(collCombine.ToArray());

        cMeshFilter.sharedMesh = visibleMesh;
        //cMeshCollider.sharedMesh = colliderMesh;

        //cMeshRenderer.sharedMaterials = materials.ToArray();
    }

    void GetMeshes()
    {
        mFilters.Clear();
        mFilters.AddRange(GetComponentsInChildren<MeshFilter>());
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GenerateCombinedMesh))]
public class GenerateCombinedMeshEditor : Editor
{
    GenerateCombinedMesh script;
    public override void OnInspectorGUI()
    {
        if (script == null) script = (GenerateCombinedMesh)target;

        if (GUILayout.Button("Combine"))
        {
            script.CombineMeshes();
        }

        base.OnInspectorGUI();
    }
}
#endif