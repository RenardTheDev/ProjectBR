using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorRagdoll : MonoBehaviour
{
    Rigidbody[] setupBones;
    public List<Bone> bones;
    public LayerMask boneMask;

    Animator anim;
    CharacterController character;

    Actor actor;
    ActorWeapon actWeapon;

    bool isRagdoll;

    private void Awake()
    {
        setupBones = GetComponentsInChildren<Rigidbody>();

        bones = new List<Bone>();
        for (int i = 0; i < setupBones.Length; i++)
        {
            if (boneMask.value == (boneMask.value | (1 << setupBones[i].gameObject.layer)))
            {
                bones.Add(new Bone(setupBones[i]));
                setupBones[i].interpolation = RigidbodyInterpolation.Interpolate;
            }
        }

        anim = GetComponent<Animator>();
        character = GetComponent<CharacterController>();

        actor = GetComponent<Actor>();
        actWeapon = GetComponent<ActorWeapon>();
    }

    float rdImpulseUpdateInterval = 0.2f;

    private void Start()
    {
        GlobalEvents.current.onActorKilled += OnActorKilled;
        GlobalEvents.current.onActorRevived += OnActorRevived;

        StartCoroutine(ImpulseUpdate());
    }

    private void OnDestroy()
    {
        GlobalEvents.current.onActorKilled -= OnActorKilled;
        GlobalEvents.current.onActorRevived -= OnActorRevived;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (isRagdoll)
            {
                ToggleRagdoll(false);
                anim.Play("Stance_rifle");
            }
            else
            {
                ToggleRagdoll(true);
            }
        }
    }

    IEnumerator ImpulseUpdate()
    {
        if (!isRagdoll)
        {
            for (int i = 0; i < bones.Count; i++)
            {
                bones[i].SaveLastTransform();
            }

            yield return new WaitForSecondsRealtime(rdImpulseUpdateInterval);

            for (int i = 0; i < bones.Count; i++)
            {
                bones[i].CalculateMovement(rdImpulseUpdateInterval);
            }
        }
        else
        {
            yield return new WaitForSecondsRealtime(rdImpulseUpdateInterval);
        }

        StartCoroutine(ImpulseUpdate());
    }

    public void ToggleRagdoll(bool state)
    {
        bones[0].rig.drag = 0;

        if (state)
        {
            anim.enabled = false;
            character.enabled = false;

            foreach (Bone b in bones)
            {
                b.RD_Enable();
            }

            isRagdoll = true;
        }
        else
        {
            anim.enabled = true;
            character.enabled = true;

            foreach (Bone b in bones)
            {
                b.RD_Disable();
            }

            isRagdoll = false;
        }
    }

    void ChangeToRagdollWithPush(Vector3 force, Vector3 point, Transform bone)
    {
        ToggleRagdoll(true);

        for (int i = 0; i < bones.Count; i++)
        {
            if (bones[i].trans == bone)
            {
                bones[i].rig.AddForce(force, ForceMode.Impulse);
                if (i == 9)
                {
                    bones[0].rig.AddForce(Vector3.up * 25, ForceMode.Impulse);
                }
            }
        }
    }

    Coroutine delayedRagdoll;
    IEnumerator DelayedRagdoll(float delay)
    {
        yield return new WaitForSeconds(delay);
        ToggleRagdoll(true);
    }

    Coroutine weapDropCoroutine;
    IEnumerator DelayedWeaponDrop(float delay)
    {
        actWeapon.currWEntity.StopAnimations();
        actWeapon.ReparentOnAnimDeath(anim.GetBoneTransform(HumanBodyBones.RightHand));

        yield return new WaitForSeconds(delay);

        actWeapon.DropAllWeapons();
    }

    float animChance;
    bool useAnimDeath=false;
    float deathAnimBlend = 0.4f;
    [Range(0, 1)] public float totalAnimChance = 0.8f;
    private void OnActorKilled(Actor actor, Damage damage)
    {
        if (actor != this.actor) return;

        animChance = Random.value;
        useAnimDeath = animChance < totalAnimChance && Vector3.Dot(-damage.direction, transform.forward) > 0.8f;

        if (!useAnimDeath)
        {
            ChangeToRagdollWithPush(damage.direction * damage.weapon.impact, damage.point, damage.bone);

            weapDropCoroutine = StartCoroutine(DelayedWeaponDrop(Random.value * 1f));
        }
        else
        {
            if (damage.weapon.pellets > 1 && (transform.position - damage.attacker.transform.position).sqrMagnitude < 9)  // if pellets > 1 => shotgun
            {
                int dNum = Random.Range(3, 5);
                anim.CrossFadeInFixedTime("death_front_" + dNum, dNum == 4 ? 0.1f : deathAnimBlend);
            }
            else
            {
                int dNum = Random.Range(0, 4);
                anim.CrossFadeInFixedTime("death_front_" + dNum, deathAnimBlend);
            }

            weapDropCoroutine = StartCoroutine(DelayedWeaponDrop(0.1f + Random.value * 0.2f));
            delayedRagdoll = StartCoroutine(DelayedRagdoll(deathAnimBlend + Random.value * (1.75f - deathAnimBlend)));

            actWeapon.ReparentOnAnimDeath(anim.GetBoneTransform(HumanBodyBones.RightHand));
        }
    }

    private void OnActorRevived(Actor actor)
    {
        if (actor != this.actor) return;

        ToggleRagdoll(false);
        anim.Play("Stance_rifle");

        if (weapDropCoroutine != null) StopCoroutine(weapDropCoroutine);
        if (delayedRagdoll != null) StopCoroutine(delayedRagdoll);
    }
}

[System.Serializable]
public class Bone
{
    public Transform trans;
    public Rigidbody rig;
    public Collider coll;

    public HingeJoint hinge;

    public Vector3 lastPos;
    public Vector3 lastRot;

    public Vector3 velocity;
    public Vector3 angularVelocity;

    public float springForce;

    public Bone(Rigidbody setup)
    {
        trans = setup.transform;
        rig = setup;
        coll = setup.GetComponent<Collider>();
        hinge = setup.GetComponent<HingeJoint>();

        rig.isKinematic = true;
        coll.isTrigger = true;
    }

    public void SaveLastTransform()
    {
        lastPos = trans.position;
        lastRot = trans.eulerAngles;
    }

    public void CalculateMovement(float timeDelta)
    {
        velocity = (trans.position - lastPos) / timeDelta;
        angularVelocity = (trans.eulerAngles - lastRot) / timeDelta;
    }

    public void RD_Enable()
    {
        rig.isKinematic = false;
        coll.isTrigger = false;

        rig.WakeUp();
        rig.velocity = velocity;
        rig.angularVelocity = new Vector3(
            angularVelocity.x * Mathf.Deg2Rad,
            angularVelocity.y * Mathf.Deg2Rad,
            angularVelocity.z * Mathf.Deg2Rad
            );

        velocity = Vector3.zero;
        angularVelocity = Vector3.zero;
    }

    public void RD_Disable()
    {
        rig.isKinematic = true;
        coll.isTrigger = true;
    }
}