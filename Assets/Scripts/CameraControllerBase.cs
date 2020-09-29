using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraControllerBase : MonoBehaviour
{
    public static CameraControllerBase current;


    Animator anim;
    CinemachineStateDrivenCamera cm_base;
    public CinemachineVirtualCamera cm_normalCam;
    public CinemachineVirtualCamera cm_adsCam;
    public CinemachineVirtualCamera cm_waveEnd;

    public CinemachineImpulseSource hitImpact;

    public CinemachineVirtualCamera[] cm_deathCam;

    //---Recoil---
    [Header("Recoil")]
    public Vector3 recoil_raw;
    public Vector3 recoil;

    public Vector3 recoil_offset_raw;
    public Vector3 recoil_offset;

    public float recoilDecay;
    public float recoilAttack;

    //---Camera---
    Transform cm_trans_normal;
    Transform cm_trans_ads;
    Camera maincam;
    Transform camTrans;


    public float pitch;
    public float heading;
    public float yaw;
    public Vector3 aimEuler;

    //---camera settings---
    [Header("camera settings")]

    public Vector3 base_offset;
    public Vector3 side_offset;
    public Vector3 side_offset_aim;
    public Vector3 hips_offset;
    [Space]
    public float standardFov = 70f;
    public float zoomSpeed = 20f;

    Vector3 _base_offset;
    Vector3 _side_offset;
    float _fov;
    public float aim_blend;

    //---actor components---
    Actor t_actor;
    ActorEvents t_events;
    ActorMotor t_motor;
    ActorWeapon t_weapon;
    Transform t_trans;

    void Awake()
    {
        current = this;

        anim = GetComponent<Animator>();
        cm_base = GetComponent<CinemachineStateDrivenCamera>();

        if (t_actor != null) SetActorTarget(t_actor);
    }

    private void Start()
    {
        ChangeRenderDistance();

        GlobalEvents.current.onActorGetHit += _OnActorTakeDamage;
        GlobalEvents.current.onActorKilled += _OnActorKilled;
        GlobalEvents.current.onActorRevived += _OnActorRevived;

        cm_trans_normal = cm_normalCam.transform;
        cm_trans_ads = cm_adsCam.transform;

        maincam = Camera.main;
        camTrans = maincam.transform;
    }

    Vector2 lookInput;
    private void Update()
    {
        recoil_raw = Vector3.Lerp(recoil_raw, Vector3.zero, Time.deltaTime * recoilDecay);
        recoil = Vector3.Lerp(recoil, recoil_raw, Time.deltaTime * recoilAttack);

        recoil_offset_raw = Vector3.Lerp(recoil_offset_raw, Vector3.zero, Time.deltaTime * recoilDecay);
        recoil_offset = Vector3.Lerp(recoil_offset, recoil_offset_raw, Time.deltaTime * recoilAttack);

        //--------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------
        if (t_actor != null)
        {
            if (t_actor.isAlive)
            {
                lookInput = Vector3.zero;
                if (!t_motor.aiming)
                {
                    lookInput.x = (Controls.fire.delta.y + Controls.look.y) * Controls.sens;
                    lookInput.y = (Controls.fire.delta.x + Controls.look.x) * Controls.sens;
                }
                else
                {
                    lookInput.x = (Controls.fire.delta.y + Controls.look.y) * Controls.sens /* * (t_weapon.currWData.aimFOV / standardFov)*/;
                    lookInput.y = (Controls.fire.delta.x + Controls.look.x) * Controls.sens /* * (t_weapon.currWData.aimFOV / standardFov)*/;
                }
            }
            else
            {
                lookInput = Vector2.zero;
            }

            pitch -= lookInput.x;
            heading += lookInput.y;

            pitch = Mathf.Clamp(pitch, -60, 60);
            heading = Mathf.Repeat(heading, 360f);

            UpdateCamera();

            aim_blend = Mathf.MoveTowards(aim_blend, t_motor.aiming ? 1f : 0f, Time.deltaTime * zoomSpeed);

            /*cm_normalCam.m_Lens.FieldOfView = Mathf.MoveTowards(
                cm_normalCam.m_Lens.FieldOfView,
                t_motor.aiming && !t_weapon.currWData.hasScope ? t_weapon.currWData.aimFOV : standardFov,
                Time.deltaTime * zoomSpeed * standardFov);*/
            cm_normalCam.m_Lens.FieldOfView = standardFov;
        }
        //--------------------------------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------

        aimEuler = new Vector3(pitch + recoil.x, heading + recoil.y, yaw + recoil.z);
    }

    private void LateUpdate()
    {
        if (t_actor != null)
        {
            if (t_actor.isAlive)
            {

            }
            //UpdateCameraTransform();
            //UpdateCamera();
        }
    }

    public void UpdateCameraTransform()
    {
        cm_trans_normal.rotation = Quaternion.Euler(aimEuler);
        cm_trans_normal.position = t_trans.position + _base_offset + Quaternion.Euler(0, aimEuler.y, 0) * hips_offset + cm_trans_normal.rotation * (recoil_offset + _side_offset);

        /*if (!t_weapon.IsCurrEntityEmpty())
        {
            cm_trans_ads.rotation = t_weapon.WeaponHolder.rotation;
            cm_trans_ads.position = t_weapon.WeaponHolder.TransformPoint(t_weapon.currWData.adsOffset + recoil_offset);
        }*/
    }
    void UpdateCamera()
    {
        _base_offset = Vector3.MoveTowards(_base_offset, base_offset, Time.deltaTime * 2);
        _side_offset = Vector3.Lerp(side_offset, side_offset_aim, 1f - cm_normalCam.m_Lens.FieldOfView / standardFov);

        hips_offset = Vector3.Lerp(hips_offset, t_trans.InverseTransformPoint(t_actor.hips.position), Time.deltaTime * 8f);

        /*if (!t_weapon.IsCurrEntityEmpty())
        {
            cm_adsCam.m_Lens.FieldOfView = t_weapon.currWData.aimFOV;
        }*/
    }

    public void ChangeRenderDistance()
    {
        var cms = FindObjectsOfType<CinemachineVirtualCamera>();

        for (int i = 0; i < cms.Length; i++)
        {
            cms[i].m_Lens.FarClipPlane = SettingsManager.instance.settings.renderDistance;
        }
    }

    AnimatorStateInfo tempInfo;
    public void TogglePhotomodeCamera(bool state)
    {
        if (state)
        {
            tempInfo = anim.GetCurrentAnimatorStateInfo(0);
            anim.Play("Photomode");
        }
        else
        {
            anim.Play(tempInfo.fullPathHash);
        }
    }

    public void SetActorTarget(Actor actor)
    {
        t_actor = actor;
        t_trans = t_actor.transform;
        t_events = t_actor.GetComponent<ActorEvents>();
        t_motor = t_actor.GetComponent<ActorMotor>();
        t_weapon = t_actor.GetComponent<ActorWeapon>();

        cm_base.m_Follow = t_actor.transform;
        cm_base.m_LookAt = t_actor.target;

        t_motor.OnAimStateChanged += OnActorAimStateChanged;
        t_events.onWeaponShot += OnActorWeaponShot;

        anim.SetBool("dead", false);

        cm_deathCam[0].m_Follow = actor.transform;
        cm_deathCam[0].m_LookAt = actor.target;

        cm_deathCam[1].m_Follow = actor.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
        cm_deathCam[1].m_LookAt = cm_deathCam[1].m_Follow;

        cm_waveEnd.m_Follow = t_trans;
        cm_waveEnd.m_LookAt = actor.target;

        _base_offset = base_offset;
        _side_offset = side_offset;
        hips_offset = t_trans.InverseTransformPoint(t_actor.hips.position);
    }

    public void ClearActorTarget()
    {

    }

    private void OnActorWeaponShot(WeaponDATA data)
    {
        recoilDecay = data.recoilDecay;
        recoilAttack = data.recoilAttack;

        //this for shifting aim when spraying
        pitch += recoil_raw.x;
        heading += recoil_raw.y;

        Vector3 recoil = new Vector3(
            -data.recoilPower.x * (0.8f + Random.value * 0.2f),
            data.recoilPower.y * (Random.value * 2f - 1f),
            0
            );

        recoil_raw = new Vector3(recoil.x, recoil.y,
            -(recoil.y / data.recoilPower.y) * data.recoilPower.z);

        recoil_offset_raw = new Vector3(
            0,
            0,
            data.recoilOffsetPower.z
            );
    }

    private void OnActorAimStateChanged(bool state, bool hasOptics)
    {
        if (t_actor.isAlive)
        {
            anim.SetBool("scope", state && hasOptics);
        }
    }

    private void _OnActorRevived(Actor actor)
    {
        if (actor != t_actor) return;

        anim.SetBool("dead", false);
        anim.SetBool("scope", false);
    }

    private void _OnActorTakeDamage(Actor actor, Damage damage)
    {
        if (actor != t_actor) return;

        if (actor.isPlayer)
        {
            hitImpact.GenerateImpulseAt(damage.point, damage.direction * (damage.amount * 0.04f));
        }
    }

    private void _OnActorKilled(Actor actor, Damage damage)
    {
        if (actor != t_actor) return;

        anim.SetBool("dead", true);
    }

    public void ChangeToState(string stateName, float blendTime)
    {
        anim.CrossFade(stateName, blendTime);
    }
}

public enum UpdateMethod
{
    Normal,
    Late,
    Physics,
    None
}