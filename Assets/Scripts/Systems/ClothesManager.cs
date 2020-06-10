using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ClothesManager : MonoBehaviour
{
    public static ClothesManager current;
    public List<ClothingDATA> clothes;
    public Dictionary<ClothingType, List<ClothingDATA>> cl_dict;

    private void Awake()
    {
        if (!current)
        {
            DontDestroyOnLoad(gameObject);
            current = this;
        }
        else
        {
            Destroy(gameObject);
        }
        GetClothesAssets();
    }

    public ClothingDATA GetClothes(ClothingType type, int id)
    {
        if (cl_dict == null) GetClothesAssets();

        return cl_dict[type][id];
    }

    public void GetClothesAssets()
    {
        cl_dict = new Dictionary<ClothingType, List<ClothingDATA>>();

        for (int i = 0; i < clothes.Count; i++)
        {
            var c = clothes[i];

            if (cl_dict.ContainsKey(c.type))
            {
                cl_dict[c.type].Add(c);
            }
            else
            {
                cl_dict.Add(c.type, new List<ClothingDATA>());
                cl_dict[c.type].Add(c);
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ClothesManager))]
public class ClothesManagerEditor : Editor
{
    ClothesManager script;
    bool showAssets;
    public override void OnInspectorGUI()
    {
        if (script == null) script = (ClothesManager)target;

        if (GUILayout.Button("Update clothes base"))
        {
            script.GetClothesAssets();
        }


        if (script.cl_dict != null)
        {
            foreach (KeyValuePair<ClothingType, List<ClothingDATA>> item in script.cl_dict)
            {
                string content = "";
                for (int i = 0; i < item.Value.Count; i++)
                {
                    content += "\n\t> " + item.Value[i].ClothName;
                }
                GUILayout.Label("Clothes type \'" + item.Key.ToString() + "\':" + content);
            }
        }
        else
        {
            GUILayout.Label("No clothes found");
        }


        base.OnInspectorGUI();

        EditorUtility.SetDirty(target);
    }
}
#endif