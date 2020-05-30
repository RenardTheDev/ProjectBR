using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupManager : MonoBehaviour
{
    public static PickupManager inst;

    public GameObject _prefab;
    public List<pickup> pickups = new List<pickup>();

    private void Awake()
    {
        inst = this;

        for (int i = 0; i < 20; i++)
        {
            generatePickUp();
        }
    }

    private void Update()
    {
        for (int i = 0; i < pickups.Count; i++)
        {
            if (pickups[i].script.active)
            {
                pickups[i].activeTime += Time.deltaTime;
                if (pickups[i].activeTime >= 30f)
                {
                    pickups[i].Hide();
                }
            }
        }
    }

    public void SpawnAmmoPickUp(Vector3 position, AmmoDATA caliber, int amount)
    {
        for (int i = 0; i < pickups.Count; i++)
        {
            if (!pickups[i].script.active)
            {
                pickups[i].Activate(position, caliber, amount);
                return;
            }
        }

        //---if there is no picks available---

        generatePickUp(position, caliber, amount);
    }

    public void HidePickup(PickupAmmo pickup)
    {
        var sPick = pickups.Find(x => x.script == pickup);
        if (sPick != null) sPick.Hide();
    }

    void generatePickUp()
    {
        var go = Instantiate(_prefab);
        go.transform.parent = transform;

        pickup newPickup = new pickup(go);

        newPickup.Hide();

        pickups.Add(newPickup);
    }

    void generatePickUp(Vector3 position, AmmoDATA caliber, int amount)
    {
        var go = Instantiate(_prefab);
        go.transform.parent = transform;

        pickup newPickup = new pickup(go);

        newPickup.Activate(position, caliber, amount);

        pickups.Add(newPickup);
    }
}

[System.Serializable]
public class pickup
{
    public GameObject go;
    public PickupAmmo script;
    public TrailRenderer trail;
    public MeshRenderer rend;
    public Rigidbody rig;

    public float activeTime;

    public pickup(GameObject go)
    {
        this.go = go;
        script = go.GetComponent<PickupAmmo>();
        trail = go.GetComponent<TrailRenderer>();
        rend = go.GetComponent<MeshRenderer>();
        rig = go.GetComponent<Rigidbody>();
    }

    public void Activate(Vector3 position, AmmoDATA caliber, int amount)
    {
        go.transform.position = position;
        go.SetActive(true);

        script.active = true;
        script.caliber = caliber;
        script.amount = amount;
        trail.emitting = true;
        trail.startColor = caliber.color;
        trail.endColor = caliber.color;
        rend.material.color = caliber.color;
        //rend.material = caliber.boxMat;
        var rand = Random.onUnitSphere;
        rand.y = 0;
        rig.AddForce((rand + Vector3.up).normalized * 2f, ForceMode.VelocityChange);

        activeTime = 0;
    }

    public void Hide()
    {
        script.active = false;
        go.SetActive(false);
        trail.emitting = false;
    }
}