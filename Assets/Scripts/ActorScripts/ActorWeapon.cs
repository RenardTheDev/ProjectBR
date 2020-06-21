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
    ActorInventory inv;

    public WeaponSlot[] slots;
    public WeaponSlot currSlot;

    public LayerMask pickupMask;

    public Transform WeaponHolder;

    public bool isArmed;

    public AudioSource sfx_Shooting;
    public AudioSource sfx_Handling;

    private void Awake()
    {
        actor = GetComponent<Actor>();
        events = GetComponent<ActorEvents>();
        look = GetComponent<ActorLook>();
        motor = GetComponent<ActorMotor>();
        anim = GetComponent<Animator>();
        inv = GetComponent<ActorInventory>();

        slots = new WeaponSlot[5];
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
                //--- fire arm attacks ---
                if (motor.dirLock_fade < 0.5f)
                {
                    currSlot.entity.inp_fire = inp_fire;
                    currSlot.entity.inp_reload = inp_reload;
                }
            }
            else
            {
                //--- melee attacks ---
            }
        }
    }

    public void ChangeSlot(int id)
    {
        if (id == -1)
        {
            if (isArmed)
            {
                HolsterCurrentWeapon();
            }
            else
            {
                DrawCurrentWeapon();
            }
        }
        else
        {
            if (id < slots.Length)
            {
                if (isArmed) HolsterCurrentWeapon();
                DrawWeapon(id);
            }
        }
    }

    void DrawCurrentWeapon()
    {

    }

    void DrawWeapon(int slot)
    {
        currSlot = slots[slot];
    }

    void HolsterCurrentWeapon()
    {

    }

    void AssignNewWeapon(int sID, WeaponEntity entity)
    {
        slots[sID].AssignWeapon(entity);
    }

    public void PlayShotSFX(AudioClip sfx)
    {
        sfx_Shooting.PlayOneShot(sfx);
    }

    public void PlayHandlingSFX(AudioClip sfx)
    {
        sfx_Handling.PlayOneShot(sfx);
    }

    public int GetCurrentAmmo()
    {
        return inv.inventory.GetQuantity(currSlot.caliber.invItem);
    }

    public void RemoveCurrentAmmo(int amount)
    {
        inv.inventory.RemoveItem(currSlot.caliber.invItem, amount);
    }

    bool useIK;
    public Transform bonePivot;
    public Transform sprintPivot;
    float dirLock_fade;
    AvatarIKGoal[] goal = new AvatarIKGoal[] { AvatarIKGoal.LeftHand, AvatarIKGoal.RightHand };

    private void OnAnimatorIK(int layerIndex)
    {
        if (!actor.isAlive) return;

        dirLock_fade = motor.dirLock_fade;
        /*if (isArmed)
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
        if (isArmed) useIK = useIK && weapon[slot].entity.data != null && weapon[slot].entity.data.type != WeaponType.Melee;

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
        }*/
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
    public bool isSelected;
    public WeaponEntity entity;

    //public int clip;

    //--- links ---
    public AmmoDATA caliber;

    public bool IsEmpty()
    {
        return entity == null;
    }

    public void AssignWeapon(WeaponEntity newEntity)
    {
        entity = newEntity;
        caliber = entity.data.ammoType;
    }

    public void ClearWeapon()
    {

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