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

        if (clothing.type == ClothingType.torso) ApplyClothes(clothing.arms);
    }

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
                    int id = Random.Range(0, ClothesManager.current.cl_dict[t].Count);
                    ChangeClothes(t, id);
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