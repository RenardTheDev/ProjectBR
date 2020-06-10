using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.PlayerLoop;

public class ActorLook : MonoBehaviour
{
    Actor actor;
    ActorEvents events;
    ActorMotor motor;
    ActorWeapon weapon;
    Animator animator;
    CharacterController character;

    [Header("Look parameters")]
    public Vector3 lookAt;
    public Vector3 aimEuler;
    public Transform HeadBone;
    public float pitch;
    public float heading;
    public float yaw;

    [Header("Aim assist")]
    public Quaternion aimAssist;
    RaycastHit[] aaHit;
    public Transform aa_lastTarget;
    public Actor aa_Actor;
    public float aa_Radius = 0.2f;
    [Range(0, 1)] public float aa_Power = 0.2f;
    public LayerMask aa_mask;
    List<RaycastHit> aa_targets;

    //---Recoil---
    [Header("Recoil")]
    public Vector3 recoil_raw;
    public Vector3 recoil;

    public Vector3 recoil_offset_raw;
    public Vector3 recoil_offset;

    public float recoilDecay;
    public float recoilAttack;

    //---Camera---
    CinemachineVirtualCamera cm_cam_normal;
    CinemachineVirtualCamera cm_cam_ads;
    Transform cm_trans_normal;
    Transform cm_trans_ads;
    Camera maincam;
    Transform camTrans;

    private void Awake()
    {
        actor = GetComponent<Actor>();
        events = GetComponent<ActorEvents>();
        motor = GetComponent<ActorMotor>();
        weapon = GetComponent<ActorWeapon>();
        animator = GetComponent<Animator>();
        character = GetComponent<CharacterController>();

        _base_offset = base_offset;
        _side_offset = side_offset;
        hips_offset = transform.InverseTransformPoint(actor.hips.position);

        aa_targets = new List<RaycastHit>();
    }

    private void Start()
    {
        cm_cam_normal = CameraControllerBase.current.cm_normalCam;
        cm_cam_ads = CameraControllerBase.current.cm_adsCam;
        cm_trans_normal = cm_cam_normal.transform;
        cm_trans_ads = cm_cam_ads.transform;

        maincam = Camera.main;
        camTrans = maincam.transform;

        events.onWeaponShot += OnActorShot;
    }

    Vector2 lookInput;
    private void Update()
    {
        recoil_raw = Vector3.Lerp(recoil_raw, Vector3.zero, Time.deltaTime * recoilDecay);
        recoil = Vector3.Lerp(recoil, recoil_raw, Time.deltaTime * recoilAttack);

        recoil_offset_raw = Vector3.Lerp(recoil_offset_raw, Vector3.zero, Time.deltaTime * recoilDecay);
        recoil_offset = Vector3.Lerp(recoil_offset, recoil_offset_raw, Time.deltaTime * recoilAttack);

        if (actor.isPlayer)
        {
            lookInput = Vector3.zero;
            if (weapon.IsCurrEntityEmpty() || !motor.aiming)
            {
                lookInput.x = (Controls.fire.delta.y + Controls.look.y) * Controls.sens;
                lookInput.y = (Controls.fire.delta.x + Controls.look.x) * Controls.sens;
            }
            else
            {
                lookInput.x = (Controls.fire.delta.y + Controls.look.y) * Controls.sens * (weapon.currWData.aimFOV / standardFov);
                lookInput.y = (Controls.fire.delta.x + Controls.look.x) * Controls.sens * (weapon.currWData.aimFOV / standardFov);
            }

            pitch -= lookInput.x;
            heading += lookInput.y;

            aa_lastTarget = null;
            float range = 1000;

            RaycastHit hit;
            if (Physics.Raycast(maincam.ViewportPointToRay(Vector2.one * 0.5f), out hit, 1000f, aa_mask))
            {
                range = hit.distance;
            }

            aa_targets.Clear();
            aa_targets.AddRange(Physics.SphereCastAll(camTrans.position + camTrans.forward * 1f, aa_Radius, camTrans.forward, range, aa_mask));
            if (aa_targets.Count > 0)
            {
                for (int i = 0; i < aa_targets.Count; i++)
                {
                    var coll = aa_targets[i].collider;
                    aa_Actor = coll.GetComponentInParent<Actor>();
                    if (aa_Actor != null && aa_Actor != actor)
                    {
                        aa_lastTarget = aa_Actor.target;
                        aimAssist = Quaternion.LookRotation(
                            (aa_Actor.target.position - Vector3.up * 0.1f - camTrans.position).normalized,
                            Vector3.up);
                        break;
                    }
                }
            }

            if (aa_lastTarget != null)
            {
                Debug.DrawRay(camTrans.position, aimAssist * Vector3.forward, Color.magenta);

                pitch = Mathf.LerpAngle(pitch, aimAssist.eulerAngles.x, aa_Power * Time.deltaTime * 30);
                heading = Mathf.LerpAngle(heading, aimAssist.eulerAngles.y, aa_Power * Time.deltaTime * 30);
            }
            else
            {
                //aa_offset = Vector3.zero;
            }

            pitch = Mathf.Clamp(pitch, -60, 60);
            heading = Mathf.Repeat(heading, 360f);

            //UpdateCamera();

            aim_blend = Mathf.MoveTowards(aim_blend, motor.aiming ? 1f : 0f, Time.deltaTime * zoomSpeed);

            cm_cam_normal.m_Lens.FieldOfView = Mathf.MoveTowards(
                cm_cam_normal.m_Lens.FieldOfView,
                motor.aiming && !weapon.currWData.hasScope ? weapon.currWData.aimFOV : standardFov,
                Time.deltaTime * zoomSpeed * standardFov);
        }

        aimEuler = new Vector3(pitch + recoil.x, heading + recoil.y, yaw + recoil.z);

        if (character.enabled && animator.enabled)
        {
            /*if (animator.GetFloat("rootm") == 0)
            {
                if (motor.sprint_fade > 0.01f)
                {
                    transform.rotation = Quaternion.Lerp(
                        Quaternion.Euler(0, heading, 0),
                        Quaternion.Euler(0, heading, 0) * Quaternion.LookRotation(motor.inpDirection),
                        motor.sprint_fade);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0, heading, 0);
                }
            }*/
        }
    }

    private void LateUpdate()
    {
        if (actor.isPlayer)
        {
            UpdateCamera();
        }
        ApplyLookData();
    }

    Quaternion lookatQuat;
    public void LookAtPoint(Vector3 point)
    {
        lookAt = point;

        lookatQuat = Quaternion.LookRotation((point - actor.target.position).normalized, Vector3.up);

        pitch = lookatQuat.eulerAngles.x;
        heading = lookatQuat.eulerAngles.y;
    }

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
    void UpdateCamera()
    {
        _base_offset = Vector3.MoveTowards(_base_offset, base_offset, Time.deltaTime * 2);
        _side_offset = Vector3.Lerp(side_offset, side_offset_aim, 1f - cm_cam_normal.m_Lens.FieldOfView / standardFov);

        hips_offset = Vector3.Lerp(hips_offset, transform.InverseTransformPoint(actor.hips.position), Time.deltaTime * 8f);

        if (!weapon.IsCurrEntityEmpty())
        {
            cm_cam_ads.m_Lens.FieldOfView = weapon.currWData.aimFOV;
        }
    }

    void UpdateCameraTransform()
    {
        cm_trans_normal.rotation = Quaternion.Euler(aimEuler);
        cm_trans_normal.position = transform.position + _base_offset + Quaternion.Euler(0, aimEuler.y, 0) * hips_offset + cm_trans_normal.rotation * (recoil_offset + _side_offset);

        if (!weapon.IsCurrEntityEmpty())
        {
            cm_trans_ads.rotation = weapon.WeaponHolder.rotation;
            cm_trans_ads.position = weapon.WeaponHolder.TransformPoint(weapon.currWData.adsOffset + recoil_offset);
        }
    }

    void ApplyLookData()
    {
        if (actor.isAlive && weapon.isArmed)
        {
            /*weapon.WeaponHolder.rotation = Quaternion.Lerp(
                Quaternion.Euler(Mathf.LerpAngle(aimEuler.x, 0f, weapon.relo_fade), aimEuler.y, 0),
                weapon.sprintPivot.rotation * Quaternion.Euler(weapon.currWData.sprintAngle),
                motor.sprint_fade);

            weapon.WeaponHolder.position = Vector3.Lerp(
                weapon.bonePivot.position,
                weapon.sprintPivot.TransformPoint(weapon.currWData.sprintOffset),
                motor.sprint_fade);

            animator.Update(0);*/
        }

        if (actor.isPlayer) UpdateCameraTransform();
    }

    private void OnActorShot(WeaponDATA data)
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


}
