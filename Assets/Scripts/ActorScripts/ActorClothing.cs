using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

public class ActorClothing : MonoBehaviour
{
    public Dictionary<ClothingType, SkinnedMeshRenderer> skinned;
    public Dictionary<string, Transform> bones;

    //public SkinnedMeshRenderer skinned_build;

    private void Awake()
    {
        GetSkeleton();
        GetSkinnedRenderers();
    }

    void GetSkinnedRenderers()
    {
        skinned = new Dictionary<ClothingType, SkinnedMeshRenderer>();
        for (int i = 0; i < 8; i++)
        {
            skinned.Add((ClothingType)i, transform.Find("geom_" + ((ClothingType)i).ToString()).GetComponent<SkinnedMeshRenderer>());
        }
    }

    public void GetSkeleton()
    {
        bones = new Dictionary<string, Transform>();
        GetBones(transform);
    }
    void GetBones(Transform trans)
    {
        for (int i = 0; i < trans.childCount; i++)
        {
            var tr = trans.GetChild(i);
            if (!bones.ContainsKey(tr.name)) bones.Add(tr.name, tr);

            GetBones(tr);
        }
    }

    List<Transform> bonesToRig = new List<Transform>();
    //public ClothingDATA clothing;
    public void ChangeClothes(ClothingType type, int id)
    {
        if (Application.isEditor)
        {
            //clothing = FindObjectOfType<ClothesManager>().GetClothes(type, id);
            ApplyClothes(FindObjectOfType<ClothesManager>().GetClothes(type, id));
        }
        else
        {
            //clothing = ClothesManager.current.GetClothes(type, id);
            ApplyClothes(ClothesManager.current.GetClothes(type, id));
        }
    }

    public void RemoveClothes(ClothingType type)
    {
        skinned[type].enabled = false;
    }

    void ApplyClothes(ClothingDATA clothing)
    {
        GetSkinnedRenderers();
        GetSkeleton();

        bonesToRig.Clear();
        for (int i = 0; i < clothing.originalSkin.bones.Length; i++)
        {
            var bone = clothing.originalSkin.bones[i];
            bool added = false;
            foreach (var item in bones)
            {
                if (item.Key.Contains(bone.name))
                {
                    bonesToRig.Add(item.Value);
                    added = true;
                    break;
                }
            }

            if (!added)
            {
                Debug.Log("Cant find \'" + bone.name + "\'");
                bonesToRig.Add(null);
            }
        }

        //Debug.Log("Trying to apply - " + clothing.name);

        skinned[clothing.type].sharedMesh = clothing.originalSkin.sharedMesh;
        skinned[clothing.type].bones = bonesToRig.ToArray();
        skinned[clothing.type].enabled = true;

        //UpdateCharacterBuild();

        if (clothing.type == ClothingType.torso) ApplyClothes(clothing.arms);
    }

    /*void UpdateCharacterBuild()
    {
        Mesh newBuild = new Mesh();
        newBuild.name = "CharacterBatch";

        //List<int> tris = new List<int>();
        //List<Vector3> verts = new List<Vector3>();
        //List<Vector3> normals = new List<Vector3>();
        //List<Vector2> UVs = new List<Vector2>();
        List<Matrix4x4> binds = new List<Matrix4x4>();
        List<BoneWeight> bw = new List<BoneWeight>();

        //int lastIndCount = 0;
        foreach (var s in skinned)
        {
            //for (int i = 0; i < s.Value.sharedMesh.triangles.Length; i++)
            //{
            //    tris.Add(s.Value.sharedMesh.triangles[i] + lastIndCount);
            //}
            //lastIndCount = s.Value.sharedMesh.triangles.Length;

            //verts.AddRange(s.Value.sharedMesh.vertices);
            //normals.AddRange(s.Value.sharedMesh.normals);
            //UVs.AddRange(s.Value.sharedMesh.uv);

            bw.AddRange(s.Value.sharedMesh.boneWeights);
        }

        for (int i = 0; i < bonesToRig.Count; i++)
        {
            binds.Add(bonesToRig[i].worldToLocalMatrix * transform.localToWorldMatrix);
        }

        //newBuild.triangles = tris.ToArray();
        //newBuild.vertices = verts.ToArray();
        //newBuild.normals = normals.ToArray();
        //newBuild.uv = UVs.ToArray();

        CombineInstance[] cis = new CombineInstance[skinned.Count];
        int c = 0;
        foreach (var s in skinned)
        {
            cis[c].mesh = s.Value.sharedMesh;
            c++;
            //s.Value.enabled = false;
        }
        newBuild.CombineMeshes(cis, true);

        newBuild.boneWeights = bw.ToArray();
        newBuild.bindposes = binds.ToArray();

        newBuild.RecalculateBounds();
        newBuild.RecalculateNormals();
        newBuild.RecalculateTangents();

        skinned_build.sharedMesh = newBuild;
        skinned_build.bones = bonesToRig.ToArray();
    }*/

    public void RandomizeClothes()
    {
        for (int i = 0; i < 8; i++)
        {
            if (Application.isEditor)
            {
                ClothingType t = (ClothingType)i;
                if (t != ClothingType.arms)
                {
                    int id = Random.Range(
                        (t == ClothingType.hair || t == ClothingType.hat) ? -1 : 0,
                        FindObjectOfType<ClothesManager>().cl_dict[t].Count);
                    if (id == -1)
                    {
                        RemoveClothes(t);
                    }
                    else
                    {
                        ChangeClothes(t, id);
                    }
                }
            }
            else
            {
                ClothingType t = (ClothingType)i;
                if (t != ClothingType.arms)
                {
                    /*int id = Random.Range(0, ClothesManager.current.cl_dict[t].Count);
                    ChangeClothes(t, id);*/

                    int id = Random.Range(
                        (t == ClothingType.hair || t == ClothingType.hat) ? -1 : 0,
                        ClothesManager.current.cl_dict[t].Count);
                    if (id == -1)
                    {
                        RemoveClothes(t);
                    }
                    else
                    {
                        ChangeClothes(t, id);
                    }
                }
            }
        }
    }

    public void LoadPlayerCustomization()
    {
        if (File.Exists(Application.persistentDataPath + "/player.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.OpenRead(Application.persistentDataPath + "/player.dat");

            ClothesConfig playerClothes = (ClothesConfig)bf.Deserialize(file);
            file.Close();

            foreach (var item in playerClothes.clothing)
            {
                if (item.Value >= 0)
                {
                    ChangeClothes(item.Key, item.Value);
                }
                else
                {
                    RemoveClothes(item.Key);
                }
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ActorClothing))]
public class ActorClothingEditor : Editor
{
    ActorClothing script;
    public override void OnInspectorGUI()
    {
        if (script == null) script = (ActorClothing)target;

        if (GUILayout.Button("Randomize clothes"))
        {
            script.RandomizeClothes();
        }

        base.OnInspectorGUI();

        EditorUtility.SetDirty(target);
    }
}
#endif