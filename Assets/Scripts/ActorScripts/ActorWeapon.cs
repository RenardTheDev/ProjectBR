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

    Transform rh;

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

        rh = anim.GetBoneTransform(HumanBodyBones.RightHand);
    }

    private void Start()
    {
        eqp.OnSlotDraw += OnSlotDraw;
    }

    private void OnSlotDraw(WeaponSlot slot)
    {
        sprintPivot = anim.GetBoneTransform(slot.entity.data.sprintPivot);
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

    public void UpdateWeapon()
    {

    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (!actor.isAlive) return;

        dirLock_fade = motor.dirLock_fade;

        useIK = eqp.isArmed && !eqp.currSlot.isEmpty && eqp.currSlot.entity.data.type != WeaponType.Melee;

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