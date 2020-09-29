using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PickupManager : MonoBehaviour
{
    public static PickupManager current;
    //public List<pickup> pickups = new List<pickup>();

    public Dictionary<ItemObject, List<pickup>> activePickup;
    public Dictionary<ItemObject, List<pickup>> inactivePickup;

    [HideInInspector] public List<ItemObject> item_dataBase;

    private void Awake()
    {
        current = this;

        UpdateItemDatabase();

        activePickup = new Dictionary<ItemObject, List<pickup>>();
        inactivePickup = new Dictionary<ItemObject, List<pickup>>();

        for (int i = 0; i < item_dataBase.Count; i++)
        {
            activePickup.Add(item_dataBase[i], new List<pickup>());
            inactivePickup.Add(item_dataBase[i], new List<pickup>());
        }

        foreach (var pList in inactivePickup)
        {
            for (int i = 0; i < 20; i++)
            {
                generatePickUp(pList.Key);
            }
        }
    }

    public void UpdateItemDatabase()
    {
        item_dataBase = new List<ItemObject>();

        string[] guids = AssetDatabase.FindAssets("t:ItemObject");
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            item_dataBase.Add(AssetDatabase.LoadAssetAtPath<ItemObject>(path));
        }
    }

    private void Update()
    {
        foreach (var pList in activePickup)
        {
            for (int i = 0; i < pList.Value.Count; i++)
            {
                var p = pList.Value[i];

                p.activeTime += Time.deltaTime;
                if (p.activeTime >= 30f)
                {
                    HidePickup(pList.Key, p);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            var spot = Actor.PLAYERACTOR.target.position + Random.onUnitSphere * 4 + Vector3.up * 3f;
            SpawnPickup(spot, Actor.PLAYERACTOR.target.rotation, item_dataBase[Random.Range(0, item_dataBase.Count)], Random.Range(1, 100));
        }
    }

    public void HidePickup(pickup p)
    {
        HidePickup(p.item, p);
    }

    void HidePickup(ItemObject item, pickup p)
    {
        if (activePickup.ContainsKey(item))
        {
            activePickup[item].Remove(p);
            inactivePickup[item].Add(p);

            p.Hide();
        }
    }

    pickup spawned_pickup;
    public void SpawnPickup(Vector3 pos, Quaternion rot, ItemObject item, int amount)
    {
        if (inactivePickup[item].Count > 0)
        {
            spawned_pickup = inactivePickup[item][0];

            activePickup[item].Add(spawned_pickup);
            inactivePickup[item].Remove(spawned_pickup);
        }
        else
        {
            spawned_pickup = generatePickUp(item);
        }

        spawned_pickup.Spawn(pos, rot, amount);
    }

    public void SpawnWeaponPickup(Vector3 pos, Quaternion rot, WeaponDATA weapon)
    {
        if (inactivePickup[weapon.inv_object].Count > 0)
        {
            spawned_pickup = inactivePickup[weapon.inv_object][0];

            activePickup[weapon.inv_object].Add(spawned_pickup);
            inactivePickup[weapon.inv_object].Remove(spawned_pickup);
        }
        else
        {
            spawned_pickup = generatePickUp(weapon.inv_object);
        }

        spawned_pickup.Spawn(pos, rot, 1);
    }

    pickup generatePickUp(ItemObject item)
    {
        var go = Instantiate(item.prefab_w, transform);

        pickup newPickup = new pickup(go);

        if (inactivePickup.ContainsKey(item))
        {
            inactivePickup[item].Add(newPickup);
        }
        else
        {
            inactivePickup.Add(item, new List<pickup>(new[] { newPickup }));
        }

        go.SetActive(false);

        return newPickup;
    }
}

[System.Serializable]
public class pickup
{
    Transform trans;
    public GameObject go;
    public ItemObject item;
    public Item w_item;

    public float activeTime;

    public pickup(GameObject go)
    {
        this.go = go;
        trans = go.transform;
        w_item = go.GetComponent<Item>();
        w_item.pickup = this;
        item = w_item.item;

        activeTime = 0;
    }

    public void Spawn(Vector3 pos, Quaternion rot, int amount)
    {
        trans.position = pos;
        trans.rotation = rot;

        if (item is WeaponObject)
        {
            w_item.amount = 1;
        }
        else
        {
            w_item.amount = amount;
        }

        go.SetActive(true);
    }

    public void Hide()
    {
        go.SetActive(false);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PickupManager))]
public class PickupManagerEditor : Editor
{
    PickupManager script;

    public override void OnInspectorGUI()
    {
        if (script == null) script = (PickupManager)target;

        base.OnInspectorGUI();

        EditorGUILayout.Separator();

        if(script.item_dataBase!=null && script.item_dataBase.Count > 0)
        {
            for (int i = 0; i < script.item_dataBase.Count; i++)
            {
                var item = script.item_dataBase[i];
                GUILayout.BeginHorizontal();
                GUILayout.Label($"\'{item.Name}\'");
                GUILayout.Label($"{script.activePickup[item].Count} / {script.inactivePickup[item].Count}");
                GUILayout.EndHorizontal();
            }
        }
    }
}
#endif