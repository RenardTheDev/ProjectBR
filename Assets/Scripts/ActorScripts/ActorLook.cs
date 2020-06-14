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

    CameraControllerBase camControl;

    [Header("Look parameters")]
    public Vector3 lookAt;
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
    }

    Quaternion lookatQuat;
    public void LookAtPoint(Vector3 point)
    {
        lookAt = point;

        lookatQuat = Quaternion.LookRotation((point - actor.target.position).normalized, Vector3.up);

        pitch = lookatQuat.eulerAngles.x;
        heading = lookatQuat.eulerAngles.y;
    }

    void ApplyLookData()
    {

    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (actor.isAlive)
        {
            if (weapon.isArmed)
            {
                animator.SetLookAtWeight(1f, 0.0f, 1f, 1f, 1f);
            }
            else
            {
                animator.SetLookAtWeight(1f, 0.0f, 1f, 1f, 1f);
            }
        }
    }
}
