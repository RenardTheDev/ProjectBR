using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MaterialDoubles : MonoBehaviour
{
    public Dictionary<Material, int> doubles;

    Renderer rend;

    private void Awake()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (rend == null) rend = GetComponent<Renderer>();

        Gizmos.color = new Color(255, 0, 0, 0.25f);
        Gizmos.DrawCube(rend.bounds.center, rend.bounds.size);
        Gizmos.color = new Color(255, 0, 0, 1);
        Gizmos.DrawWireCube(rend.bounds.center, rend.bounds.size);
    }

    private void OnDrawGizmosSelected()
    {
        
    }

    List<Material> mats;
    public void FixDoubles()
    {
        if (rend == null) rend = GetComponent<Renderer>();
        mats = new List<Material>();
        mats.AddRange(rend.sharedMaterials);

        MeshFilter mf = GetComponent<MeshFilter>();

        Debug.Log("mats.Count=" + mats.Count + " | subMeshCount=" + mf.sharedMesh.subMeshCount);

        List<Combined> comb = new List<Combined>();

        for (int i = 0; i < mf.sharedMesh.subMeshCount; i++)
        {
            var sub = mf.sharedMesh.GetSubMesh(i);

            List<int> tris = new List<int>();
            for (int t = sub.indexStart; t < sub.indexCount; t++)
            {
                tris.Add(mf.sharedMesh.triangles[t] - sub.baseVertex);
            }

            List<Vector3> verts = new List<Vector3>();
            for (int t = sub.firstVertex; t < sub.vertexCount; t++) verts.Add(mf.sharedMesh.vertices[t]);

            List<Vector3> normals = new List<Vector3>();
            for (int t = sub.firstVertex; t < sub.vertexCount; t++) normals.Add(mf.sharedMesh.normals[t]);

            List<Vector2> uvs = new List<Vector2>();
            for (int t = sub.firstVertex; t < sub.vertexCount; t++) uvs.Add(mf.sharedMesh.uv[t]);

            if (comb.Count > 1 && i > 0)
            {
                var ex = comb[comb.Count - 1];

                if (ex.mat == mats[i])
                {
                    ex.AddComponents(tris, verts, normals, uvs);
                }
                else
                {
                    Combined newCombo = new Combined(mats[i], tris, verts, normals, uvs);
                    comb.Add(newCombo);
                }
            }
            else
            {
                Combined newCombo = new Combined(mats[i], tris, verts, normals, uvs);
                comb.Add(newCombo);
            }
        }

        Mesh newMesh = new Mesh();
        newMesh.name = gameObject.name;

        rend.sharedMaterials = new Material[comb.Count];
        CombineInstance[] combo = new CombineInstance[comb.Count];

        Material[] matsToAssign = new Material[comb.Count];
        for (int i = 0; i < comb.Count; i++)
        {
            matsToAssign[i] = comb[i].mat;
            combo[i] = comb[i].part;
        }
        rend.sharedMaterials = matsToAssign;
        newMesh.CombineMeshes(combo);
    }
}

public class Combined
{
    public Material mat;

    public List<int> indices;
    public List<Vector3> vertices;
    public List<Vector3> normals;
    public List<Vector2> uvs;

    public CombineInstance part;

    public Combined(Material mat, List<int> indices, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs)
    {
        this.mat = mat;
        this.indices = indices;
        this.vertices = vertices;
        this.normals = normals;
        this.uvs = uvs;

        part = new CombineInstance();
        part.mesh = new Mesh();

        part.mesh.vertices = vertices.ToArray();
        part.mesh.triangles = indices.ToArray();
        part.mesh.normals = normals.ToArray();
        part.mesh.uv = uvs.ToArray();
    }

    public void AddComponents(List<int> indices, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs)
    {
        for (int i = 0; i < indices.Count; i++)
        {
            indices[i] += this.vertices.Count;
        }

        this.indices.AddRange(indices);
        this.vertices.AddRange(vertices);
        this.normals.AddRange(normals);
        this.uvs.AddRange(uvs);

        part.mesh.vertices = vertices.ToArray();
        part.mesh.triangles = indices.ToArray();
        part.mesh.normals = normals.ToArray();
        part.mesh.uv = uvs.ToArray();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MaterialDoubles))]
public class MaterialDoublesEditor : Editor
{
    MaterialDoubles script;

    string info;

    public override void OnInspectorGUI()
    {
        if (script == null) script = (MaterialDoubles)target;
        base.OnInspectorGUI();

        if (GUILayout.Button("FixDoubles"))
        {
            script.FixDoubles();
        }

        if (script.doubles != null)
        {
            info = "";
            foreach (var item in script.doubles)
            {
                info += "\n" + item.Key.name + " x" + item.Value;
            }

            GUILayout.Label("Doubles: \n" + info);
        }

        EditorUtility.SetDirty(script);
    }
}
#endif