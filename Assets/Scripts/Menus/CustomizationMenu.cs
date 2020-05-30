using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CustomizationMenu : MonoBehaviour
{
    ClothesConfig playerClothes = new ClothesConfig();

    public ActorClothing actor;
    public Animator anim;

    public RectTransform viewPort;

    public GameObject items_Hats;
    public GameObject items_Hair;
    public GameObject items_Torso;
    public GameObject items_Legs;
    public GameObject items_Feet;

    public GameObject prefab_item;

    public Dictionary<Button, ClothingDATA> items;

    private void Start()
    {
        viewPort.parent.parent.gameObject.SetActive(true);
        UpdateClothesUI();
        viewPort.parent.parent.gameObject.SetActive(false);

        LoadCustomization();
    }

    public void OpenCustomization()
    {
        anim.SetBool("customization", true);
    }

    public void CloseCustomization()
    {
        anim.SetBool("customization", false);
    }

    public void UpdateClothesUI()
    {
        ClearItemsFromList();

        if (Application.isEditor) ClothesManager.current = FindObjectOfType<ClothesManager>();

        for (int i = -1; i < ClothesManager.current.cl_dict[ClothingType.hat].Count; i++)
        {
            if (i==-1)
            {
                var go = Instantiate(prefab_item, items_Hats.transform);
                go.GetComponentInChildren<Text>().text = "Remove";
                go.GetComponent<Button>().onClick.AddListener(CreateButtonAction(ClothingType.hat, -1));
            }
            else
            {
                var go = Instantiate(prefab_item, items_Hats.transform);
                go.GetComponentInChildren<Text>().text = ClothesManager.current.cl_dict[ClothingType.hat][i].ClothName;
                go.GetComponent<Button>().onClick.AddListener(CreateButtonAction(ClothingType.hat, i));
            }
        }

        for (int i = -1; i < ClothesManager.current.cl_dict[ClothingType.hair].Count; i++)
        {
            if (i == -1)
            {
                var go = Instantiate(prefab_item, items_Hair.transform);
                go.GetComponentInChildren<Text>().text = "Remove";
                go.GetComponent<Button>().onClick.AddListener(CreateButtonAction(ClothingType.hair, -1));
            }
            else
            {
                var go = Instantiate(prefab_item, items_Hair.transform);
                go.GetComponentInChildren<Text>().text = ClothesManager.current.cl_dict[ClothingType.hair][i].ClothName;
                go.GetComponent<Button>().onClick.AddListener(CreateButtonAction(ClothingType.hair, i));
            }
        }

        for (int i = 0; i < ClothesManager.current.cl_dict[ClothingType.torso].Count; i++)
        {
            var go = Instantiate(prefab_item, items_Torso.transform);
            go.GetComponentInChildren<Text>().text = ClothesManager.current.cl_dict[ClothingType.torso][i].ClothName;
            go.GetComponent<Button>().onClick.AddListener(CreateButtonAction(ClothingType.torso, i));
        }

        for (int i = 0; i < ClothesManager.current.cl_dict[ClothingType.legs].Count; i++)
        {
            var go = Instantiate(prefab_item, items_Legs.transform);
            go.GetComponentInChildren<Text>().text = ClothesManager.current.cl_dict[ClothingType.legs][i].ClothName;
            go.GetComponent<Button>().onClick.AddListener(CreateButtonAction(ClothingType.legs, i));
        }

        for (int i = 0; i < ClothesManager.current.cl_dict[ClothingType.feet].Count; i++)
        {
            var go = Instantiate(prefab_item, items_Feet.transform);
            go.GetComponentInChildren<Text>().text = ClothesManager.current.cl_dict[ClothingType.feet][i].ClothName;
            go.GetComponent<Button>().onClick.AddListener(CreateButtonAction(ClothingType.feet, i));
        }

        UpdateLayout();
    }

    public void UpdateLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(viewPort);
    }

    UnityAction CreateButtonAction(ClothingType type, int id)
    {
        return () => ApplyClothing(type, id);
    }

    public void ApplyClothing(ClothingType type, int id)
    {
        if (id == -1)
        {
            actor.RemoveClothes(type);
        }
        else
        {
            actor.ChangeClothes(type, id);
        }
        playerClothes.clothing[type] = id;

        SaveCustomization();
    }

    public void LoadCustomization()
    {
        if (File.Exists(Application.persistentDataPath + "/player.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.OpenRead(Application.persistentDataPath + "/player.dat");

            playerClothes = (ClothesConfig)bf.Deserialize(file);
            file.Close();

            foreach (var item in playerClothes.clothing)
            {
                if (item.Value >= 0)
                {
                    actor.ChangeClothes(item.Key, item.Value);
                }
                else
                {
                    actor.RemoveClothes(item.Key);
                }
            }
        }
    }

    public void SaveCustomization()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/player.dat");

        bf.Serialize(file, playerClothes);
        file.Close();
    }

    public void ClearItemsFromList()
    {
        Button[] btns = items_Hats.GetComponentsInChildren<Button>();
        for (int i = 0; i < btns.Length; i++)
        {
            btns[i].onClick.RemoveAllListeners();
            DestroyImmediate(btns[i].gameObject);
        }

        btns = items_Hair.GetComponentsInChildren<Button>();
        for (int i = 0; i < btns.Length; i++)
        {
            btns[i].onClick.RemoveAllListeners();
            DestroyImmediate(btns[i].gameObject);
        }

        btns = items_Torso.GetComponentsInChildren<Button>();
        for (int i = 0; i < btns.Length; i++)
        {
            btns[i].onClick.RemoveAllListeners();
            DestroyImmediate(btns[i].gameObject);
        }

        btns = items_Legs.GetComponentsInChildren<Button>();
        for (int i = 0; i < btns.Length; i++)
        {
            btns[i].onClick.RemoveAllListeners();
            DestroyImmediate(btns[i].gameObject);
        }

        btns = items_Feet.GetComponentsInChildren<Button>();
        for (int i = 0; i < btns.Length; i++)
        {
            btns[i].onClick.RemoveAllListeners();
            DestroyImmediate(btns[i].gameObject);
        }
    }
}

[System.Serializable]
public class ClothesConfig
{
    public Dictionary<ClothingType, int> clothing;

    public ClothesConfig()
    {
        clothing = new Dictionary<ClothingType, int>();
        for (int i = 0; i < 8; i++)
        {
            if ((ClothingType)i != ClothingType.arms) clothing.Add((ClothingType)i, 0);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CustomizationMenu))]
public class CustomizationMenuEditor : Editor
{
    CustomizationMenu script;
    bool showAssets;
    public override void OnInspectorGUI()
    {
        if (script == null) script = (CustomizationMenu)target;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Update clothes UI"))
        {
            script.UpdateClothesUI();
        }
        if (GUILayout.Button("Clear UI"))
        {
            script.ClearItemsFromList();
        }
        GUILayout.EndHorizontal();

        base.OnInspectorGUI();

        EditorUtility.SetDirty(target);
    }
}
#endif