using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponEntity : MonoBehaviour
{
    [HideInInspector] public Animator anim;

    public ParticleSystem[] ps_muzzle;
    public ParticleSystem ps_shells;

    [HideInInspector] public bool holstered = true;
    [HideInInspector] public bool isReloading;
    [HideInInspector] public bool isReloaded;
    [HideInInspector] public bool isChambered;
    [HideInInspector] public bool holdDown;

    public int clip;

    public Transform[] HIK;
    public Transform muzzle;
    public Transform bullet;

    [HideInInspector] public bool inp_fire;
    [HideInInspector] public bool inp_reload;

    //---helpers---
    public WeaponDATA data;
    public GameObject[] scope_swap;
    public float spread;

    //--- timings ---
    [HideInInspector] public float lastShot;
    [HideInInspector] public float shotDelay;

    //---Coroutines
    [HideInInspector] public Coroutine _reloadCoroutine;
    [HideInInspector] public Coroutine _chamberCoroutine;

    //---handler---
    [HideInInspector] public Actor handler_actor;
    [HideInInspector] public ActorEvents handler_events;
    [HideInInspector] public ActorLook handler_look;
    [HideInInspector] public ActorMotor handler_motor;
    [HideInInspector] public ActorWeapon handler;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        shotDelay = getFireRate();
    }

    public GameObject clipObject;
    private void Update()
    {
        if (Time.timeScale < 0.5f)
        {
            anim.updateMode = AnimatorUpdateMode.Normal;
        }
        else
        {
            anim.updateMode = AnimatorUpdateMode.AnimatePhysics;
        }
    }

    public virtual void TryShoot()
    { }

    public virtual void TryReload()
    { }

    public void OnAimStateChanged(bool state)
    {
        if (data.hasScope) anim.SetFloat("scope_fade", state ? 1f : 0f);
    }

    public void StopAnimations()
    {
        anim.Play("idle", 0, 0);
        if (data.hasScope) anim.SetFloat("scope_fade", 0);
        SpawnClip();
    }

    public void anim_Reload(bool isTactical)
    {
        if (data.reloadMode == ReloadMode.clip)
        {
            if (isTactical)
            {
                anim.Play("reload_tac", 0, 0);
            }
            else
            {
                anim.Play("reload_full", 0, 0);
            }
        }
        else
        {
            anim.Play("reload_start", 0, 0);
        }
    }

    public void anim_ReloadStart()
    {
        anim.Play("reload_start", 0, 0);
    }

    public void anim_InsertShell()
    {
        anim.Play("reload_insert", 0, 0);
    }

    public void anim_ReloadEnd()
    {
        anim.Play("reload_end", 0, 0);
    }

    public void anim_Chamber()
    {
        anim.Play("chamber", 0, 0);
    }

    public int getClipSize()
    {
        int value = data.clipSize;
        return value;
    }

    public float getFireRate()
    {
        float value = 60f / data.fireRate;
        return value;
    }

    public void AnimEvent_Chamber()
    {
        handler.PlayHandlingSFX(data.sfx_chamber);
    }

    public void AnimEvent_Remove()
    {
        handler.PlayHandlingSFX(data.sfx_remove);
    }

    public void AnimEvent_Insert()
    {
        handler.PlayHandlingSFX(data.sfx_insert);
    }

    public void AnimEvent_PlaySound(Object clip)
    {
        handler.PlayHandlingSFX((AudioClip)clip);
    }

    Vector3 clipPos;
    Vector3 clipRot;
    float savedTime;
    float savedDelta;
    public void SaveClipTransform()
    {
        clipPos = clipObject.transform.position;
        clipRot = clipObject.transform.eulerAngles;
        savedTime = Time.time;
    }

    public void DropClip()
    {
        if (clipObject != null)
        {
            var clip = Instantiate(clipObject, clipObject.transform.position, clipObject.transform.rotation, null);

            var rb = clip.GetComponent<Rigidbody>();

            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            savedDelta = Time.time - savedTime;
            rb.AddForce((rb.position - clipPos) / savedDelta, ForceMode.VelocityChange);
            rb.AddTorque((rb.rotation.eulerAngles - clipRot) / savedDelta, ForceMode.VelocityChange);

            //rb.AddTorque(Random.onUnitSphere * 10, ForceMode.VelocityChange);

            clip.GetComponent<Collider>().enabled = true;

            Destroy(clip, 10f);

            clipObject.SetActive(false);
        }
    }

    public void SpawnClip()
    {
        if (clipObject != null)
        {
            clipObject.SetActive(true);
        }
    }
}

