using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager current;

    public Dictionary<WeaponDATA, List<eqpWeapon>> activeEQP;
    public Dictionary<WeaponDATA, List<eqpWeapon>> inactiveEQP;

    public Dictionary<int, eqpWeapon> refList;

    [HideInInspector] public List<WeaponDATA> eqp_dataBase;

    private void Awake()
    {
        current = this;

        UpdateWeaponDatabase();

        activeEQP = new Dictionary<WeaponDATA, List<eqpWeapon>>();
        inactiveEQP = new Dictionary<WeaponDATA, List<eqpWeapon>>();
        refList = new Dictionary<int, eqpWeapon>();

        for (int i = 0; i < eqp_dataBase.Count; i++)
        {
            activeEQP.Add(eqp_dataBase[i], new List<eqpWeapon>());
            inactiveEQP.Add(eqp_dataBase[i], new List<eqpWeapon>());
        }

        /*foreach (var eqpList in inactiveEQP)
        {
            for (int i = 0; i < 20; i++)
            {
                generateEQP(eqpList.Key);
            }
        }*/
    }
    public void UpdateWeaponDatabase()
    {
        eqp_dataBase = new List<WeaponDATA>();

        string[] guids = AssetDatabase.FindAssets("t:WeaponDATA");
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            eqp_dataBase.Add(AssetDatabase.LoadAssetAtPath<WeaponDATA>(path));
        }
    }

    eqpWeapon spawned_eqp;
    public WeaponEntity SpawnEQP(WeaponDATA weapon, Transform parent)
    {
        if (inactiveEQP[weapon].Count > 0)
        {
            spawned_eqp = inactiveEQP[weapon][0];
        }
        else
        {
            spawned_eqp = generateEQP(weapon);
        }

        activeEQP[weapon].Add(spawned_eqp);
        inactiveEQP[weapon].Remove(spawned_eqp);

        spawned_eqp.Spawn(parent);

        return spawned_eqp.entity;
    }

    eqpWeapon generateEQP(WeaponDATA weapon)
    {
        var go = Instantiate(weapon.prefab, transform);

        eqpWeapon newPickup = new eqpWeapon(go);

        refList.Add(go.GetInstanceID(), newPickup);

        if (inactiveEQP.ContainsKey(weapon))
        {
            inactiveEQP[weapon].Add(newPickup);
        }
        else
        {
            inactiveEQP.Add(weapon, new List<eqpWeapon>(new[] { newPickup }));
        }

        go.SetActive(false);

        return newPickup;
    }

    public void HideEQP(GameObject go)
    {
        int instID = go.GetInstanceID();
        if (refList.ContainsKey(instID))
        {
            eqpWeapon eqp = refList[instID];
            if (activeEQP[eqp.data].Contains(eqp))
            {
                eqp.Hide(transform);

                activeEQP[eqp.data].Remove(eqp);
                inactiveEQP[eqp.data].Add(eqp);
            }
            else
            {
                Debug.LogError($"activeEQP doesn't contain \'{eqp.entity.name}\'");
            }
        }
        else
        {
            Debug.LogError($"Object \'{go.name}\' has no reference in the refList");
        }
    }
}

public class eqpWeapon
{
    public GameObject go;
    public Transform trans;
    public WeaponEntity entity;
    public WeaponDATA data;

    public eqpWeapon(GameObject go)
    {
        this.go = go;
        trans = go.transform;
        entity = go.GetComponent<WeaponEntity>();
        data = entity.data;
    }

    public void Spawn(Transform parent)
    {
        ResetTransform(parent);
        go.SetActive(true);
    }

    public void Hide(Transform parent)
    {
        entity.StopAnimations();

        ResetTransform(parent);
        go.SetActive(false);
    }

    void ResetTransform(Transform parent)
    {
        trans.parent = parent;
        trans.localPosition = Vector3.zero;
        trans.localRotation = Quaternion.identity;
    }
}