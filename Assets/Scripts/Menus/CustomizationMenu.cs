using Cinemachine;
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
    public static CustomizationMenu current;

    ClothesConfig playerClothes = new ClothesConfig();

    Canvas canvas;

    ActorClothing actor;
    Animator anim;

    public RectTransform viewPort;

    public Button[] category_btns;
    public Canvas[] items_canvas;
    public RectTransform[] items_packets;

    public GameObject prefab_item;

    public Dictionary<Button, ClothingDATA> items;

    CinemachineVirtualCamera zoomCamera;

    public int currentClothingType = 0;
    //0 - hat
    //1 - hair
    //2 - torso
    //3 - legs
    //4 - feet

    private void Awake()
    {
        current = this;
        canvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        viewPort.gameObject.SetActive(false);
        UpdateClothesUI();
        viewPort.gameObject.SetActive(true);

        if (anim == null)
        {
            anim = GameObject.Find("CM StateDriven").GetComponent<Animator>();
        }

        if (actor == null)
        {
            actor = GameObject.FindWithTag("Doll").GetComponent<ActorClothing>();
        }
        LoadCustomization();
    }

    /*private void Update()
    {
        if (GameManager.current.gameState != GameState.mainmenu) return;
    }*/

    public void ToggleZoom(bool state)
    {
        zoom = state;
        anim.SetBool("zoom", zoom);
    }

    bool zoom;
    public void ChangeZoom()
    {
        zoom = !zoom;
        anim.SetBool("zoom", zoom);
    }

    public void SelectTypeTab(int type)
    {
        currentClothingType = type;

        anim.SetInteger("cust_type", currentClothingType);

        for (int i = 0; i < items_canvas.Length; i++)
        {
            items_canvas[i].enabled = i == currentClothingType;
        }

        for (int i = 0; i < category_btns.Length; i++)
        {
            category_btns[i].interactable = i != currentClothingType;
        }
    }

    public void ToggleCustomization(bool state)
    {
        if (state)
        {
            SelectTypeTab(0);
            anim.SetBool("cust", true);
            ToggleZoom(false);
        }
        else
        {
            ToggleZoom(false);
            anim.SetBool("cust", false);
        }
    }

    public void UpdateClothesUI()
    {
        ClearItemsFromList();

        Debug.Log("cl_dict.Count = " + ClothesManager.current.cl_dict.Count);

        if (Application.isEditor) ClothesManager.current = FindObjectOfType<ClothesManager>();

        int count = ClothesManager.current.cl_dict[ClothingType.hat].Count;
        for (int i = -1; i < count; i++)
        {
            var go = Instantiate(prefab_item, items_packets[0].transform);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector3(0, -(i + 1) * 64);

            if (i == -1)
            {
                go.GetComponentInChildren<Text>().text = "Remove";
                go.GetComponent<Button>().onClick.AddListener(CreateButtonAction(ClothingType.hat, -1));
            }
            else
            {
                go.GetComponentInChildren<Text>().text = ClothesManager.current.cl_dict[ClothingType.hat][i].ClothName;
                go.GetComponent<Button>().onClick.AddListener(CreateButtonAction(ClothingType.hat, i));
            }
        }
        items_packets[0].sizeDelta = new Vector2(384, (count + 1) * 64 - 8);

        count = ClothesManager.current.cl_dict[ClothingType.hair].Count;
        for (int i = -1; i < ClothesManager.current.cl_dict[ClothingType.hair].Count; i++)
        {
            var go = Instantiate(prefab_item, items_packets[1].transform);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector3(0, -(i + 1) * 64);

            if (i == -1)
            {
                go.GetComponentInChildren<Text>().text = "Remove";
                go.GetComponent<Button>().onClick.AddListener(CreateButtonAction(ClothingType.hair, -1));
            }
            else
            {
                go.GetComponentInChildren<Text>().text = ClothesManager.current.cl_dict[ClothingType.hair][i].ClothName;
                go.GetComponent<Button>().onClick.AddListener(CreateButtonAction(ClothingType.hair, i));
            }
        }
        items_packets[1].sizeDelta = new Vector2(384, (count + 1) * 64 - 8);

        count = ClothesManager.current.cl_dict[ClothingType.torso].Count;
        for (int i = 0; i < ClothesManager.current.cl_dict[ClothingType.torso].Count; i++)
        {
            var go = Instantiate(prefab_item, items_packets[2].transform);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector3(0, -i * 64);

            go.GetComponentInChildren<Text>().text = ClothesManager.current.cl_dict[ClothingType.torso][i].ClothName;
            go.GetComponent<Button>().onClick.AddListener(CreateButtonAction(ClothingType.torso, i));
        }
        items_packets[2].sizeDelta = new Vector2(384, count * 64 - 8);

        count = ClothesManager.current.cl_dict[ClothingType.legs].Count;
        for (int i = 0; i < ClothesManager.current.cl_dict[ClothingType.legs].Count; i++)
        {
            var go = Instantiate(prefab_item, items_packets[3].transform);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector3(0, -i * 64);

            go.GetComponentInChildren<Text>().text = ClothesManager.current.cl_dict[ClothingType.legs][i].ClothName;
            go.GetComponent<Button>().onClick.AddListener(CreateButtonAction(ClothingType.legs, i));
        }
        items_packets[3].sizeDelta = new Vector2(384, count * 64 - 8);

        count = ClothesManager.current.cl_dict[ClothingType.feet].Count;
        for (int i = 0; i < ClothesManager.current.cl_dict[ClothingType.feet].Count; i++)
        {
            var go = Instantiate(prefab_item, items_packets[4].transform);
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector3(0, -i * 64);

            go.GetComponentInChildren<Text>().text = ClothesManager.current.cl_dict[ClothingType.feet][i].ClothName;
            go.GetComponent<Button>().onClick.AddListener(CreateButtonAction(ClothingType.feet, i));
        }
        items_packets[4].sizeDelta = new Vector2(384, count * 64 - 8);
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
        Button[] btns = items_packets[0].GetComponentsInChildren<Button>();
        for (int i = 0; i < btns.Length; i++)
        {
            btns[i].onClick.RemoveAllListeners();
            DestroyImmediate(btns[i].gameObject);
        }

        btns = items_packets[1].GetComponentsInChildren<Button>();
        for (int i = 0; i < btns.Length; i++)
        {
            btns[i].onClick.RemoveAllListeners();
            DestroyImmediate(btns[i].gameObject);
        }

        btns = items_packets[2].GetComponentsInChildren<Button>();
        for (int i = 0; i < btns.Length; i++)
        {
            btns[i].onClick.RemoveAllListeners();
            DestroyImmediate(btns[i].gameObject);
        }

        btns = items_packets[3].GetComponentsInChildren<Button>();
        for (int i = 0; i < btns.Length; i++)
        {
            btns[i].onClick.RemoveAllListeners();
            DestroyImmediate(btns[i].gameObject);
        }

        btns = items_packets[4].GetComponentsInChildren<Button>();
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