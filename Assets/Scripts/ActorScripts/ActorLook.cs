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
    ActorEquipment eqp;
    Animator animator;
    CharacterController character;

    CameraControllerBase camControl;

    [Header("Look parameters")]
    public Vector3 lookAt;
    public Vector3 lookAtVector;
    public Vector3 aimEuler;
    public float pitch;
    public float heading;
    public float yaw;

    private void Awake()
    {
        actor = GetComponent<Actor>();
        events = GetComponent<ActorEvents>();
        motor = GetComponent<ActorMotor>();
        weapon = GetComponent<ActorWeapon>();
        eqp = GetComponent<ActorEquipment>();

        animator = GetComponent<Animator>();
        character = GetComponent<CharacterController>();

        camControl = CameraControllerBase.current;
    }

    private void Start()
    {

    }

    private void Update()
    {

    }

    private void LateUpdate()
    {
        if (actor.isPlayer)
        {
            aimEuler = camControl.aimEuler;
            pitch = aimEuler.x;
            heading = aimEuler.y;
            yaw = aimEuler.z;
        }

        lookAtVector = Quaternion.Euler(aimEuler) * Vector3.forward;
        lookAt = actor.target.position + lookAtVector * 10f;

        if (Vector3.Dot(lookAtVector, transform.forward) > 0)
        {
            lookAtLerp = Vector3.SlerpUnclamped(lookAtLerp, lookAt, Time.deltaTime * 4f);
        }
        else
        {
            lookAtLerp = Vector3.SlerpUnclamped(lookAtLerp, actor.target.position + transform.forward * 10f, Time.deltaTime * 4f);
        }
    }

    Quaternion lookatQuat;
    public void LookAtPoint(Vector3 point)
    {
        //lookAt = point;

        lookatQuat = Quaternion.LookRotation((point - actor.target.position).normalized, Vector3.up);

        pitch = lookatQuat.eulerAngles.x;
        heading = lookatQuat.eulerAngles.y;
    }

    void ApplyLookData()
    {

    }

    Vector3 lookAtLerp;
    private void OnAnimatorIK(int layerIndex)
    {
        if (actor.isAlive)
        {
            animator.SetLookAtPosition(lookAtLerp);
            if (eqp.isArmed)
            {
                animator.SetLookAtWeight(1f, 0.25f, 1f, 1f, 1f);
            }
            else
            {
                animator.SetLookAtWeight(1f, 0.0f, 1f, 1f, 1f);
            }
        }
    }
}
