using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraControllerBase : MonoBehaviour
{
    public static CameraControllerBase current;

    public Actor t_actor;

    Animator anim;
    CinemachineStateDrivenCamera cm_base;
    public CinemachineVirtualCamera cm_normalCam;
    public CinemachineVirtualCamera cm_adsCam;
    public CinemachineVirtualCamera cm_waveEnd;

    public CinemachineImpulseSource hitImpact;

    public CinemachineVirtualCamera[] cm_deathCam;
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

        cm_base.m_Follow = t_actor.transform;
        cm_base.m_LookAt = t_actor.target;

        t_actor.GetComponent<ActorMotor>().OnAimStateChanged += OnActorAimStateChanged;

        anim.SetBool("dead", false);

        cm_deathCam[0].m_Follow = actor.transform;
        cm_deathCam[0].m_LookAt = actor.target;

        cm_deathCam[1].m_Follow = actor.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
        cm_deathCam[1].m_LookAt = cm_deathCam[1].m_Follow;

        cm_waveEnd.m_Follow = actor.transform;
        cm_waveEnd.m_LookAt = actor.target;
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