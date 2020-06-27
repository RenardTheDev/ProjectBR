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
    ActorEquipment eqp;
    ActorInventory inv;


    public AudioSource sfx_Shooting;
    public AudioSource sfx_Handling;

    private void Awake()
    {
        actor = GetComponent<Actor>();
        events = GetComponent<ActorEvents>();
        look = GetComponent<ActorLook>();
        motor = GetComponent<ActorMotor>();
        anim = GetComponent<Animator>();
        eqp = GetComponent<ActorEquipment>();
        inv = GetComponent<ActorInventory>();

        //currSlot = slots[0];
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

            if (eqp.isArmed)
            {
                //--- fire arm attacks ---
                if (motor.dirLock_fade < 0.5f)
                {
                    eqp.currSlot.entity.inp_fire = inp_fire;
                    eqp.currSlot.entity.inp_reload = inp_reload;
                }
            }
            else
            {
                //--- melee attacks ---
            }
        }
    }

    public void PlayShotSFX(AudioClip sfx)
    {
        sfx_Shooting.PlayOneShot(sfx);
    }

    public void PlayHandlingSFX(AudioClip sfx)
    {
        sfx_Handling.PlayOneShot(sfx);
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
        if (eqp.isArmed)
        {
            eqp.WeaponHolder.rotation = Quaternion.Lerp(
                Quaternion.Euler(Mathf.LerpAngle(look.aimEuler.x, 0f, eqp.currSlot.entity.relo_fade), look.aimEuler.y, 0),
                sprintPivot.rotation * Quaternion.Euler(eqp.currSlot.entity.data.sprintAngle),
                motor.dirLock_fade);

            eqp.WeaponHolder.position = Vector3.Lerp(
                bonePivot.position,
                sprintPivot.TransformPoint(eqp.currSlot.entity.data.sprintOffset),
                motor.dirLock_fade);

            //anim.SetLookAtWeight(Mathf.Clamp01(1f - relo_fade - dirLock_fade), 0.05f, 1f, 1f, 1f);
        }

        useIK = eqp.isArmed && !eqp.currSlot.IsEmpty() && eqp.currSlot.entity.data.type != WeaponType.Melee;

        if (useIK && eqp.isArmed)
        {
            for (int i = 0; i < 2; i++)
            {
                anim.SetIKPositionWeight(goal[i], Mathf.Lerp(1f, eqp.currSlot.entity.data.sprintHIK[i], dirLock_fade));
                anim.SetIKRotationWeight(goal[i], Mathf.Lerp(1f, eqp.currSlot.entity.data.sprintHIK[i], dirLock_fade));

                anim.SetIKPosition(goal[i], eqp.currSlot.entity.HIK[i].position);
                anim.SetIKRotation(goal[i], eqp.currSlot.entity.HIK[i].rotation);
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

    //---EVENTS---

    public delegate void SlotChangeHandler(int oldSlot, int newSlot, WeaponSlot[] slotInfo);
    public event SlotChangeHandler OnSlotChanged;

    public delegate void AmmoPickupHandler(AmmoDATA caliber, int amount);
    public event AmmoPickupHandler OnAmmoPickedup;
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