using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorWeapon : MonoBehaviour
{
    Actor actor;
    ActorEvents events;
    ActorMotor motor;
    ActorLook look;
    Animator anim;

    public Dictionary<AmmoDATA, int> ammo = new Dictionary<AmmoDATA, int>();
    public WeaponSlot[] weapon;
    public int slot = 0;    // 0 - nogun

    public LayerMask pickupMask;

    public Transform WeaponHolder;

    bool haveSpace = false;
    public LayerMask spaceCheckMask;
    [HideInInspector] public float relo_fade;

    Transform rightHand;

    public bool isArmed;

    private void Awake()
    {
        actor = GetComponent<Actor>();
        events = GetComponent<ActorEvents>();
        look = GetComponent<ActorLook>();
        motor = GetComponent<ActorMotor>();
        anim = GetComponent<Animator>();

        rightHand = anim.GetBoneTransform(HumanBodyBones.RightHand);
    }

    private void Start()
    {

    }

    public bool inp_fire;
    public bool inp_reload;
    private void Update()
    {
        if (actor.isPlayer)
        {
            inp_fire = Controls.fire.state != bindState.none;
            inp_reload = Controls.reload.state != bindState.none;

            if (isArmed)
            {
                currWEntity.inp_trigger = inp_fire;
                currWEntity.inp_reload = inp_reload;
            }

            var coll = Physics.OverlapSphere(transform.position, 2f, pickupMask);
            for (int i = 0; i < coll.Length; i++)
            {
                PickupAmmo p = coll[i].GetComponent<PickupAmmo>();
                if (p != null)
                {
                    PickUpAmmo(p.caliber, p.amount);
                    PickupManager.inst.HidePickup(p);
                }
            }
        }
    }
    
    private void FixedUpdate()
    {
        if (!actor.isAlive) return;

        Ray ray = new Ray(bonePivot.position, Quaternion.Euler(look.aimEuler) * Vector3.forward);
        if (Physics.Raycast(ray, 0.5f, spaceCheckMask))
        {
            haveSpace = false;
        }
        else
        {
            haveSpace = true;
        }

        if (isArmed) currWEntity.haveSpace = haveSpace;
    }

    private void LateUpdate()
    {
        if (!actor.isAlive) return;
        if (isArmed)
        {
            relo_fade = Mathf.MoveTowards(relo_fade, currWEntity.holdDown ? 1f : 0f, Time.deltaTime * 4f);
            currWEntity.relo_fade = relo_fade;
        }
        else
        {
            relo_fade = Mathf.MoveTowards(relo_fade, 0f, Time.deltaTime * 4f);
        }
    }

    public void GiveWeapon(WeaponDATA weapon)
    {
        GameObject go = Instantiate(weapon.prefab);
        go.transform.position = transform.position;
        WeaponEntity ent = go.GetComponent<WeaponEntity>();

        PickupWeapon(ent);

        Debug.Log(name + ".GiveWeapon(" + weapon.Name + ")");
    }

    public void GiveAmmo(AmmoDATA caliber, int amount)
    {
        if (ammo.ContainsKey(caliber))
        {
            ammo[caliber] += amount;
        }
        else
        {
            ammo.Add(caliber, amount);
        }
    }

    public void PickUpAmmo(AmmoDATA caliber, int amount)
    {
        if (ammo.ContainsKey(caliber))
        {
            ammo[caliber] += amount;
        }
        else
        {
            ammo.Add(caliber, amount);
        }

        if (amount > 0) OnAmmoPickedup?.Invoke(caliber, amount);
    }

    public void PickupWeapon(WeaponEntity entity)
    {
        if (!actor.isAlive) return;

        int reqSlot = (int)entity.data.type;
        WeaponSlot wSlot = weapon[reqSlot];

        if (entity.data.type != WeaponType.Melee) PickUpAmmo(entity.data.ammoType, 0);

        if (!wSlot.IsEmpty())
        {
            DropWeapon(reqSlot);
        }

        wSlot.entity = entity;
        ChangeSlot(reqSlot);
        entity.PickUp(actor);
    }

    public void ChangeSlot(int newSlot)
    {
        WeaponSlot oldWSlot = weapon[slot];
        WeaponSlot newWSlot = weapon[newSlot];

        if (newWSlot.IsEmpty()) return;

        if (newWSlot.slotType != WeaponType.Melee)
        {
            newWSlot.entity.transform.parent = WeaponHolder;
            newWSlot.entity.transform.localPosition = Vector3.zero;
            newWSlot.entity.transform.localRotation = Quaternion.identity;
        }

        bonePivot = anim.GetBoneTransform(newWSlot.entity.data.bonePivot);
        sprintPivot = anim.GetBoneTransform(newWSlot.entity.data.sprintPivot);

        if (currWEntity != null) currWEntity.Holster(true);

        currWEntity = newWSlot.entity;
        currWData = currWEntity.data;

        isArmed = currWEntity != null;

        if (currWData.type != WeaponType.Melee) currCaliber = newWSlot.entity.data.ammoType;

        anim.SetBool("usingGun", newWSlot.slotType != WeaponType.Melee);

        //--- change sprint animation ---
        anim.SetInteger("GripType", (int)newWSlot.entity.data.grip);

        //--- old slot manipulations ---
        if (!oldWSlot.IsEmpty())
        {
            oldWSlot.entity.reloaded = oldWSlot.entity.clip > 0;
            if (oldWSlot.slotType != WeaponType.Melee)
            {
                oldWSlot.entity.transform.parent = oldWSlot.Holster;
                oldWSlot.entity.transform.localPosition = oldWSlot.entity.data.hPosition;
                oldWSlot.entity.transform.localRotation = Quaternion.Euler(oldWSlot.entity.data.hRotation);
                oldWSlot.entity.StopAnimations();
            }
        }

        if (slot != newSlot) OnSlotChanged?.Invoke(slot, newSlot, weapon);

        slot = newSlot;

        if (!currWData.canCrouch) motor.CrouchingInput(false);

        if (currWEntity != null) currWEntity.Holster(false);
    }

    public void DropWeapon(int dropSlot, bool autoChange = true)
    {
        WeaponSlot dropWSlot = weapon[dropSlot];

        if (dropWSlot.IsEmpty()) return;
        if (dropWSlot.slotType == WeaponType.Melee) return;

        ammo[GetCaliber(dropSlot)] += GetEntity(dropSlot).clip;

        dropWSlot.entity.transform.parent = null;
        dropWSlot.entity.Drop();

        dropWSlot.entity = null;

        for (int i = 0; i < weapon.Length; i++)
        {
            if (!weapon[i].IsEmpty())
            {
                ChangeSlot(i); 
                break;
            }
        }
    }

    public void DropAllWeapons()
    {
        for (int i = 0; i < weapon.Length; i++)
        {
            WeaponSlot dropWSlot = weapon[i];
            if (!weapon[i].IsEmpty() && weapon[i].slotType != WeaponType.Melee)
            {

                dropWSlot.entity.transform.parent = null;
                dropWSlot.entity.Drop();

                dropWSlot.entity = null;
            }
        }

        ChangeSlot(2);

        if (!actor.isPlayer) DropAllAmmo();
    }

    public void DropAmmo(AmmoDATA caliber)
    {
        PickupManager.inst.SpawnAmmoPickUp(actor.target.position, caliber, ammo[caliber]);
        ammo[caliber] = 0;
    }

    List<AmmoDATA> caliberList;
    public void DropAllAmmo()
    {
        caliberList = new List<AmmoDATA>(ammo.Keys);
        for (int i = 0; i < ammo.Count; i++)
        {
            if (ammo[caliberList[i]] > 0) DropAmmo(caliberList[i]);
        }
    }

    bool useIK;
    public Transform bonePivot;
    public Transform sprintPivot;
    float dirLock_fade;
    AvatarIKGoal[] goal = new AvatarIKGoal[] { AvatarIKGoal.LeftHand, AvatarIKGoal.RightHand };

    public LookAtWeights aLaw;
    private void OnAnimatorIK(int layerIndex)
    {
        if (!actor.isAlive) return;

        dirLock_fade = motor.dirLock_fade;
        if (isArmed)
        {
            WeaponHolder.rotation = Quaternion.Lerp(
                Quaternion.Euler(Mathf.LerpAngle(look.aimEuler.x, 0f, relo_fade), look.aimEuler.y, 0),
                sprintPivot.rotation * Quaternion.Euler(currWData.sprintAngle),
                motor.dirLock_fade);

            WeaponHolder.position = Vector3.Lerp(
                bonePivot.position,
                sprintPivot.TransformPoint(currWData.sprintOffset),
                motor.dirLock_fade);

            anim.SetLookAtWeight(Mathf.Clamp01(1f - relo_fade - dirLock_fade), 0.05f, 1f, 1f, 1f);
        }
        else
        {
            anim.SetLookAtWeight(1f, 0.05f, 1f, 1f, 1f);
        }

        useIK = !(slot < 0 || slot >= weapon.Length);
        if (isArmed) useIK = useIK && weapon[slot].entity.data != null && weapon[slot].slotType != WeaponType.Melee;

        anim.SetLookAtPosition(WeaponHolder.position + Quaternion.Euler(look.aimEuler) * Vector3.forward * 10);


        if (useIK && isArmed)
        {
            for (int i = 0; i < 2; i++)
            {
                //if (i == 1 && motor.sprinting) continue;

                anim.SetIKPositionWeight(goal[i], Mathf.Lerp(1f, currWData.sprintHIK[i], dirLock_fade));
                anim.SetIKRotationWeight(goal[i], Mathf.Lerp(1f, currWData.sprintHIK[i], dirLock_fade));

                anim.SetIKPosition(goal[i], weapon[slot].entity.HIK[i].position);
                anim.SetIKRotation(goal[i], weapon[slot].entity.HIK[i].rotation);
            }
        }
        else
        {
            for (int i = 0; i < 2; i++)
            {
                anim.SetIKPositionWeight(goal[i], 0f);
                anim.SetIKRotationWeight(goal[i], 0f);
            }
        }
    }

    public void ReparentOnAnimDeath(Transform parent)
    {
        currWEntity.transform.parent = parent;
    }

    //--- links ---
    public WeaponDATA currWData;
    public WeaponEntity currWEntity;
    public AmmoDATA currCaliber;

    public WeaponDATA GetWData(int slotID)
    {
        if (slot >= 0)
            return weapon[slotID].entity.data;
        else
            return null;
    }
    public WeaponEntity GetEntity(int slotID)
    {
        return weapon[slotID].entity;
    }
    public AmmoDATA GetCaliber(int slotID)
    {
        if (weapon[slot].IsEmpty())
        {
            return null;
        }
        else
        {
            return GetWData(slotID).ammoType;
        }
    }

    public int GetCurrentAmmo()
    {
        if (weapon[slot].IsEmpty())
        {
            return 0;
        }
        else
        {
            return ammo[currCaliber];
        }
    }
    public int GetAmmo(int slotID)
    {
        if (weapon[slotID].IsEmpty())
        {
            return 0;
        }
        else
        {
            return ammo[GetCaliber(slotID)];
        }
    }

    public bool IsCurrEntityEmpty()
    {
        return weapon[slot].IsEmpty();
    }
    public bool IsEntityEmpty(int slotID)
    {
        return weapon[slotID].IsEmpty();
    }

    //---EVENTS---

    public delegate void SlotChangeHandler(int oldSlot, int newSlot, WeaponSlot[] slotInfo);
    public event SlotChangeHandler OnSlotChanged;

    public delegate void AmmoPickupHandler(AmmoDATA caliber, int amount);
    public event AmmoPickupHandler OnAmmoPickedup;
}

[System.Serializable]
public class WeaponSlot
{
    public WeaponType slotType;
    public WeaponEntity entity;

    public float t_lastShot;
    public float t_reloadStart;

    public Transform Holster;

    public bool IsEmpty()
    {
        return entity == null;
    }
}

[System.Serializable]
public class LookAtWeights
{
    [Range(0, 1)] public float weight = 1.0f;
    [Range(0, 1)] public float bodyWeight = 0.1f;
    [Range(0, 1)] public float headWeight = 1.0f;
    [Range(0, 1)] public float eyesWeight = 1.0f;
    [Range(0, 1)] public float clampWeight = 1.0f;
}