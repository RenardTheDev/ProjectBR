using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.ProBuilder;
using UnityEngine.XR;

public class WeaponEntity : MonoBehaviour
{
    Animator anim;

    Rigidbody rig;
    public Collider[] coll;

    public bool isDropped = false;
    public bool holstered = true;
    public Transform[] HIK;
    public Transform muzzle;
    public Transform bullet;

    public Transform labelPivot;

    AudioSource sfx;

    public List<WeaponModule> Modules = new List<WeaponModule>();

    //---ammo---
    public int clip;
    public bool chambered;
    public bool reloaded;

    //---handling---
    public bool haveSpace;
    public bool inp_trigger;
    public bool inp_reload;
    float lastShot = -1;

    public bool reloading;
    public bool holdDown;
    public float spread;

    //---helpers---
    public WeaponDATA data;
    public GameObject[] scope_swap;

    //---handler---
    Actor handler_actor;
    ActorEvents handler_events;
    ActorLook handler_look;
    ActorMotor handler_motor;
    ActorWeapon handler;

    //---Scope---
    public bool aiming;
    public float relo_fade;

    //---Coroutines
    Coroutine _reloadCoroutine;
    Coroutine _chamberCoroutine;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rig = GetComponent<Rigidbody>();
        sfx = GetComponentInChildren<AudioSource>();
    }

    private void Start()
    {

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

    private void FixedUpdate()
    {
        spread = Mathf.Lerp(spread, 0f, Time.fixedDeltaTime * 1f);

        if (handler_actor != null && !holstered && handler_motor.dirLock_fade < 0.5f)
        {
            if (inp_trigger && handler_actor.isAlive && data.type != WeaponType.Melee && _chamberCoroutine == null && _reloadCoroutine == null)
            {
                if (reloaded && clip > 0)
                {
                    if (Time.time > lastShot + getFireRate())
                    {
                        if (chambered)
                        {
                            MakeShot();
                        }
                        else
                        {
                            if (_chamberCoroutine == null)
                            {
                                _chamberCoroutine = StartCoroutine(_chamber(false));
                            }
                        }
                    }
                }
                else
                {
                    if (/*handler.GetCurrentAmmo() > 0 &&*/ _reloadCoroutine == null && Time.time > lastShot + getFireRate())
                    {
                        _reloadCoroutine = StartCoroutine(_reload());
                    }
                }
            }

            if (inp_reload)
            {
                TryToReload();
            }
        }
    }

    Vector3 gunVector;
    Vector3 shotPoint;
    Vector3 resDirection;
    RaycastHit vHit;

    public void TryToReload()
    {
        if (clip < getClipSize() /*&& handler.GetCurrentAmmo() > 0*/ && _chamberCoroutine == null && _reloadCoroutine == null)
        {
            _reloadCoroutine = StartCoroutine(_reload());
        }
    }

    void MakeShot()
    {
        lastShot = Time.time;

        sfx.pitch = Random.Range(0.9f, 1.1f);
        sfx.PlayOneShot(data.sfx_shot);

        //!--Make muzzleflash---

        anim.Play("shot", 0, 0);

        gunVector = bullet.forward;

        if (handler_actor.isPlayer)
        {
            shotPoint = bullet.position;

            Ray ray = Camera.main.ViewportPointToRay(Vector2.one * 0.5f);
            if (Physics.Raycast(ray, out vHit, 100))
            {
                if (vHit.distance > 1.5f)
                {
                    gunVector = (vHit.point - vHit.normal * 0.1f - bullet.position).normalized;
                }
            }

            for (int i = 0; i < data.pellets; i++)
            {
                resDirection = gunVector * 10f + Random.onUnitSphere * (spread + data.pelletSpreading);
                ProjectileManager.current.SpawnProjectile(shotPoint,
                resDirection.normalized, handler_actor, data
                );
            }
        }
        else
        {
            gunVector = (handler_look.lookAt - bullet.position).normalized;

            for (int i = 0; i < data.pellets; i++)
            {
                resDirection = gunVector * 100f + Random.onUnitSphere * (spread + data.pelletSpreading + 10 * (1f - handler_actor.Accuracy));
                ProjectileManager.current.SpawnProjectile(bullet.position,
                resDirection.normalized, handler_actor, data
                );
            }
        }

        spread += data.spreading;

        clip--;

        if (data.firingMode == FiringMode.single)
        {
            chambered = false; 
            if (_chamberCoroutine == null && clip > 0) _chamberCoroutine = StartCoroutine(_chamber(true));
        }

        if (clip == 0)
        {
            reloaded = false;
            chambered = false;
        }

        //OnWeaponShot?.Invoke(data);
        handler_events.WeaponShot(data);
    }

    IEnumerator _chamber(bool afterShot)
    {
        if (afterShot) yield return new WaitForSeconds(data.animShot.length);

        reloading = true;
        anim_Chamber();

        yield return new WaitForSeconds(data.animChamber.length);

        chambered = true;
        reloading = false;
        holdDown = false;

        handler_events.WeaponChambered();

        _chamberCoroutine = null;
    }

    IEnumerator _reload()
    {
        if (_chamberCoroutine != null) StopCoroutine(_chamberCoroutine); _chamberCoroutine = null;
        
        handler_events.WeaponReloadStart();

        reloading = true;
        holdDown = true;
        reloaded = false;

        anim_Reload(chambered);

        if (data.reloadMode == ReloadMode.clip)
        {
            if (chambered)
            {
                yield return new WaitForSeconds(data.animRel_tactical.length * 0.8f);
                holdDown = false;
                yield return new WaitForSeconds(data.animRel_tactical.length * 0.2f);
            }
            else
            {
                yield return new WaitForSeconds(data.animRel_full.length * 0.8f);
                holdDown = false;
                yield return new WaitForSeconds(data.animRel_full.length * 0.2f);
                chambered = true;
            }

            if (isInfiniteAmmo())
            {
                clip = getClipSize();
            }
            else
            {
                int diff = getClipSize() - clip;
                /*if (handler.GetCurrentAmmo() > diff)
                {
                    //handler.ammo[data.ammoType] -= diff;

                    clip = getClipSize();
                }
                else
                {
                    //clip += handler.ammo[data.ammoType];
                    //handler.ammo[data.ammoType] = 0;
                }*/
                clip = getClipSize();
            }

            reloaded = true;
            reloading = false;

            handler_events.WeaponReloadEnd();

            if (!chambered)
            {
                _chamberCoroutine = StartCoroutine(_chamber(false));
            }
        }
        else
        {
            yield return new WaitForSeconds(data.animRel_Start.length);

            while (clip < getClipSize())
            {
                if (inp_trigger && clip > 0) break;

                /*if (handler.GetCurrentAmmo() > 0)
                {
                    //handler.ammo[data.ammoType]--;
                    clip++;
                    handler_events.WeaponShellInsert();
                }
                else
                {
                    break;
                }*/
                clip++;
                handler_events.WeaponShellInsert();

                anim_InsertShell();
                yield return new WaitForSeconds(data.animRel_Insert.length);
            }

            anim_ReloadEnd();
            yield return new WaitForSeconds(data.animRel_End.length);

            reloaded = true;
            reloading = false;
            holdDown = false;

            handler_events.WeaponReloadEnd();

            if (!chambered)
            {
                _chamberCoroutine = StartCoroutine(_chamber(false));
            }
        }

        _reloadCoroutine = null;
    }

    private void LateUpdate()
    {
        
    }

    public void Drop()
    {
        transform.GetChild(0).localScale = new Vector3(1, 1, 1);

        StopAnimations();

        rig.interpolation = RigidbodyInterpolation.Interpolate;
        rig.isKinematic = false;

        rig.AddTorque(Random.onUnitSphere * 10, ForceMode.VelocityChange);

        foreach (Collider c in coll) { c.enabled = true; }

        isDropped = true;

        despawn = StartCoroutine(DespawnPickup());

        UnloadWeapon();
        Holster(true);

        aiming = false;

        handler_actor = null;
        handler_events = null;
        handler_look = null;
        handler_motor = null;
        handler = null;
    }

    public void Holster(bool state)
    {
        holstered = state;
        if (holstered)
        {
            //--- weapon holstered ---
            if (_reloadCoroutine != null) StopCoroutine(_reloadCoroutine); _reloadCoroutine = null;
            if (_chamberCoroutine != null) StopCoroutine(_chamberCoroutine); _chamberCoroutine = null;

            reloading = false;
            holdDown = false;
        }
        else
        {
            //--- weapon armed ---
            reloading = false;
            holdDown = false;
        }
    }

    Coroutine despawn;
    IEnumerator DespawnPickup()
    {
        yield return new WaitForSeconds(30f);
        Destroy(gameObject, 0f);
    }

    public void PickUp(Actor picker)
    {
        anim.enabled = true;
        rig.interpolation = RigidbodyInterpolation.None;
        rig.isKinematic = true;
        foreach (Collider c in coll) { c.enabled = false; }

        isDropped = false;

        if (despawn != null) StopCoroutine(despawn);

        handler_actor = picker;
        handler_events = handler_actor.GetComponent<ActorEvents>();
        handler_look = handler_actor.GetComponent<ActorLook>();
        handler_motor = handler_actor.GetComponent<ActorMotor>();
        handler = handler_actor.GetComponent<ActorWeapon>();

        transform.parent = handler.WeaponHolder;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(0, 0, 0);

        if (!reloaded /*&& handler.GetCurrentAmmo() > 0*/)
        {
            if (!reloading && _reloadCoroutine == null) _reloadCoroutine = StartCoroutine(_reload());
        }

        if (reloaded && !chambered)
        {
            if (_chamberCoroutine == null) _chamberCoroutine = StartCoroutine(_chamber(false));
        }
    }

    public void OnAimStateChanged(bool state)
    {
        aiming = state;
        if (data.hasScope) anim.SetFloat("scope_fade", state ? 1f : 0f);
    }

    public void StopAnimations()
    {
        anim.Play("idle", 0, 0);
        if (data.hasScope) anim.SetFloat("scope_fade", 0);
        SpawnClip();
    }

    public void UnloadWeapon()
    {
        clip = 0;
        reloaded = false;
        chambered = false;
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

    public bool isInfiniteAmmo()
    {
        if (Modules.Count > 0)
        {
            for (int i = 0; i < Modules.Count; i++)
            {
                if (Modules[i].GetType() == typeof(module_EndlessAmmo))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public int getClipSize()
    {
        int value = data.clipSize;

        if (Modules.Count > 0)
        {
            for (int i = 0; i < Modules.Count; i++)
            {
                if (Modules[i].GetType() == typeof(module_ClipSizeMod))
                {
                    value += ((module_ClipSizeMod)Modules[i]).value;
                }
            }
        }

        return value;
    }

    public float getFireRate()
    {
        float value = 60f / data.fireRate;

        if (Modules.Count > 0)
        {
            for (int i = 0; i < Modules.Count; i++)
            {
                if (Modules[i].GetType() == typeof(module_FireRateMod))
                {
                    value /= ((module_FireRateMod)Modules[i]).value;
                }
            }
        }

        return value;
    }

    public void AnimEvent_Chamber()
    {
        sfx.PlayOneShot(data.sfx_chamber);
    }

    public void AnimEvent_Remove()
    {
        sfx.PlayOneShot(data.sfx_remove);
    }

    public void AnimEvent_Insert()
    {
        sfx.PlayOneShot(data.sfx_insert);
    }

    public void AnimEvent_PlaySound(Object clip)
    {
        sfx.PlayOneShot((AudioClip)clip);
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

