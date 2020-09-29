using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorMotor : MonoBehaviour
{
    Actor actor;
    ActorEvents events;
    //ActorLook look;
    Animator animator;
    ActorWeapon weap;
    ActorEquipment eqp;
    CharacterController character;
    CameraControllerBase camControl;

    public Transform weaponHolder;
    public Transform weaponPivot;
    public Transform rh;

    public float crouchSpeed = 1.5f;
    public float runSpeed = 4f;
    public float sprintSpeed = 6f;
    public float currentSpeed;

    public Vector3 airVelocity;
    public float airMomentum;
    public float maxFallSpeed = -50;

    public bool crouching;
    public bool sprinting;
    public bool aiming;
    public bool grounded;

    public float dirLock_fade;

    [Header("Look parameters")]
    public Vector3 lookAt;
    public Vector3 lookAtVector;
    public Vector3 aimEuler;
    public float pitch;
    public float heading;
    public float yaw;

    bool useLookAt;
    float lookLerp = 0;
    Quaternion lookatQuat;
    Vector3 lookAtLerp;

    Vector3 movement;

    private void Awake()
    {
        actor = GetComponent<Actor>();
        events= GetComponent<ActorEvents>();
        //look = GetComponent<ActorLook>();
        animator = GetComponent<Animator>();
        weap = GetComponent<ActorWeapon>();
        eqp = GetComponent<ActorEquipment>();
        character = GetComponent<CharacterController>();

        camControl = CameraControllerBase.current;

        CrouchingInput(false);
    }

    private void Start()
    {
        if (actor.isPlayer)
        {
            if (TouchController.current != null)
            {
                TouchController.current.button_crouch.OnStateChanged += OnPlayerControl_Crouch;
                TouchController.current.button_aim.OnStateChanged += OnPlayerControl_Aim;
                TouchController.current.button_sprint.OnStateChanged += OnPlayerControl_Sprint;
                TouchController.current.button_equipment.OnStateChanged += OnPlayerControl_Equipment;
            }
        }

        GlobalEvents.current.onActorKilled += OnActorKilled;
        GlobalEvents.current.onActorRevived += OnActorRevived;

        events.onWeaponReloadStart += OnActorReloadStart;
    }

    private void OnDestroy()
    {
        GlobalEvents.current.onActorKilled -= OnActorKilled;
        GlobalEvents.current.onActorRevived -= OnActorRevived;
    }

    public Vector3 inpDirection;
    public Vector3 moveDir;
    public float turnSmoothTime = 0.1f;
    public float turnSmoothVelocity;

    [Header("Animation curves")]
    public float rootm;
    public float weap_to_rh;

    Vector3 inpDirNormal;
    float targetAngle;
    float angle;

    private void Update()
    {
        if (actor.isPlayer)
        {
            inpDirection = new Vector3(Controls.move.x, 0, Controls.move.y);
            if (sprinting)
            {
                inpDirection = Vector3.ClampMagnitude(inpDirection, 1f);
            }
        }

        rootm = animator.GetFloat("rootm");
        weap_to_rh = animator.GetFloat("weap_to_rh");

        dirLock_fade = Mathf.Lerp(dirLock_fade, (sprinting || !eqp.isArmed) ? 1f : 0f, Time.deltaTime * 3);

        if (crouching)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, crouchSpeed, Time.deltaTime * 4);
        }
        else
        {
            if (sprinting)
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, sprintSpeed, Time.deltaTime * 4);
            }
            else
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, runSpeed, Time.deltaTime * 4);
            }
        }

        movement = moveDir * currentSpeed * Time.deltaTime;
        movement.y = airMomentum * Time.deltaTime;

        if (!grounded)
        {
            if (airMomentum > maxFallSpeed) airMomentum -= 9.8f * Time.deltaTime;
        }
        else
        {
            airMomentum = -9.8f;
        }

        if (character.enabled)
        {
            animator.applyRootMotion = rootm > 0.1f;
            if (rootm < 0.1f)
            {
                character.Move(movement * (1f - rootm * 10));
            }

            animator.SetBool("crouching", crouching);
            animator.SetBool("grounded", grounded);
            animator.SetBool("usingGun", eqp.isArmed);
            animator.SetFloat("move_x", transform.InverseTransformVector(character.velocity).x, 0.05f, Time.deltaTime);
            animator.SetFloat("move_z", transform.InverseTransformVector(character.velocity).z, 0.05f, Time.deltaTime);
            animator.SetFloat("air_y", airVelocity.y);

            if (eqp.isArmed)
            {
                float targetAngle = heading;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime + (9999f * rootm));

                if (sprinting)
                {
                    if (inpDirection.magnitude < 0.25f || Vector3.Dot(inpDirection.normalized, Vector3.forward) < 0.5f)
                    {
                        ChangeSprintState(false);
                    }
                    else
                    {
                        inpDirection = inpDirection.normalized;
                    }
                }

                if (inpDirection.magnitude >= 0.1f)
                {
                    moveDir = Quaternion.Euler(0, angle, 0) * inpDirection;
                }
                else
                {
                    moveDir = Vector3.zero;
                }

                //transform.rotation = Quaternion.Euler(0, heading, 0);
            }
            else
            {
                if (sprinting)
                {
                    if (inpDirection.magnitude < 0.25f)
                    {
                        ChangeSprintState(false);
                    }
                    else
                    {
                        inpDirection = inpDirection.normalized;
                    }
                }

                if (inpDirection.magnitude >= 0.1f)
                {
                    inpDirNormal = inpDirection.normalized;
                    targetAngle = Mathf.Atan2(inpDirNormal.x, inpDirNormal.z) * Mathf.Rad2Deg + heading;
                    angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime + (9999f * rootm));
                    //transform.rotation = Quaternion.Euler(0, angle, 0);

                    moveDir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                }
                else
                {
                    moveDir = Vector3.zero;
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (actor.isPlayer)
        {
            aimEuler = camControl.aimEuler;
            pitch = aimEuler.x;
            heading = aimEuler.y;
            yaw = aimEuler.z;

            CameraControllerBase.current.UpdateCameraTransform();
        }

        lookAtVector = Quaternion.Euler(aimEuler) * Vector3.forward;
        useLookAt = Vector3.Dot(lookAtVector, transform.forward) > 0;
        lookLerp = Mathf.Lerp(lookLerp, useLookAt ? 1 : 0, Time.deltaTime * 2);

        if (useLookAt) lookAt = actor.target.position + lookAtVector * 10f;

        lookAtLerp = Vector3.SlerpUnclamped(actor.target.position + transform.forward * 10f, lookAt, lookLerp);

        if (eqp.isArmed)
        {
            transform.rotation = Quaternion.Euler(0, heading, 0);
        }
        else
        {
            if (inpDirection.magnitude >= 0.1f)
            {
                transform.rotation = Quaternion.Euler(0, angle, 0);
            }
        }

        if (eqp.isArmed)
        {
            weaponHolder.rotation = Quaternion.Lerp(
                   Quaternion.Euler(Mathf.LerpAngle(aimEuler.x, 0f, eqp.currSlot.entity.relo_fade), aimEuler.y, 0),
                   rh.rotation * Quaternion.Euler(eqp.currSlot.entity.data.inHandRotation),
                   weap_to_rh);

            weaponHolder.position = Vector3.Lerp(
                weaponPivot.position,
                rh.TransformPoint(eqp.currSlot.entity.data.inHandOffset),
                weap_to_rh);
        }
    }
    public void LookAtPoint(Vector3 point)
    {
        //lookAt = point;

        lookatQuat = Quaternion.LookRotation((point - actor.target.position).normalized, Vector3.up);

        pitch = lookatQuat.eulerAngles.x;
        heading = lookatQuat.eulerAngles.y;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (actor.isAlive)
        {
            animator.SetLookAtWeight(1f, 0.0f, 1f, 1f, 1f);
            animator.SetLookAtPosition(lookAtLerp);
            /*if (eqp.isArmed)
            {
                animator.SetLookAtWeight(1f, 0.0f, 1f, 1f, 1f);
            }
            else
            {
                animator.SetLookAtWeight(1f, 0.0f, 1f, 1f, 1f);
            }*/
        }
    }

    private void FixedUpdate()
    {
        if (grounded)
        {
            if (!character.isGrounded)
            {
                OnLostGround();
            }
        }
        else
        {
            if (character.isGrounded)
            {
                OnGrounded();
            }
            else
            {
                airVelocity = character.velocity;
            }
        }

        grounded = character.isGrounded;
    }

    private void OnGUI()
    {
        if (actor.isPlayer)
        {
            GUILayout.Space(100);
            GUILayout.Label("grounded = " + grounded.ToString());
            GUILayout.Label("airVelocity = " + airVelocity.ToString());
        }
    }

    public float hardLanding_threshold = -10f;

    void OnGrounded()
    {
        Debug.Log("Grounded velocity = " + airVelocity.ToString());
        grounded = true;

        if (airVelocity.y < hardLanding_threshold)
        {
            if (inpDirection.magnitude > 0)
            {
                animator.CrossFade("fall_to_roll", 0.05f, 0, 0.1f, 0.3f);
            }
            else
            {
                animator.CrossFade("fall_hard", 0.05f, 0, 0.1f, 0.3f);
            }
        }
    }

    void OnLostGround()
    {
        Debug.Log("Lost ground");
        grounded = false;
        airMomentum = airMomentum * 0.5f;
    }

    public void CrouchingInput(bool state)
    {
        crouching = state;
        if (crouching) ChangeSprintState(false);
    }

    void OnActorKilled(Actor actor, Damage dmg)
    {
        if (actor != this.actor) return;

        character.enabled = false;
        CrouchingInput(false);
        ChangeAimState(false);
    }

    private void OnActorRevived(Actor actor)
    {
        if (actor != this.actor) return;

        character.enabled = true;
    }

    private void OnActorReloadStart()
    {
        ChangeAimState(false);
    }

    private void OnEnable()
    {
        if (actor.isPlayer)
        {
            if (TouchController.current != null)
            {
                TouchController.current.button_crouch.OnStateChanged += OnPlayerControl_Crouch;
                TouchController.current.button_aim.OnStateChanged += OnPlayerControl_Aim;
            }
        }
    }

    private void OnPlayerControl_Aim(bindState state)
    {
        if (state == bindState.down)
        {
            ChangeAimState(!aiming);
        }
    }

    public void ChangeAimState(bool state)
    {
        if (!state)
        {
            aiming = false;
            //actWeapon.currWEntity.OnAimStateChanged(state && actor.isPlayer);
            //OnAimStateChanged?.Invoke(state, actWeapon.currWData.hasScope);
        }
        else
        {
            if (!sprinting /*&& !actWeapon.IsCurrEntityEmpty() && !actWeapon.currWEntity.reloading*/)
            {
                aiming = true;
                //actWeapon.currWEntity.OnAimStateChanged(state && actor.isPlayer);
                //OnAimStateChanged?.Invoke(state, actWeapon.currWData.hasScope);
            }
        }
    }

    private void OnPlayerControl_Sprint(bindState state)
    {
        if (state == bindState.down)
        {
            if (!sprinting)
            {
                if (crouching || inpDirection.sqrMagnitude < 0.25f) return;

                if (eqp.isArmed)
                {
                    if (Vector3.Dot(inpDirection, Vector3.forward) > 0.5f)
                    {
                        if (aiming) ChangeAimState(false);
                        ChangeSprintState(true);
                    }
                }
                else
                {
                    if (aiming) ChangeAimState(false);
                    ChangeSprintState(true);
                }
            }
            else
            {
                ChangeSprintState(false);
            }
        }
    }

    public void ChangeSprintState(bool state)
    {
        sprinting = state;
        OnSprintStateChanged?.Invoke(state);
    }

    private void OnPlayerControl_Equipment(bindState state)
    {
        /*if (state == bindState.down)
        {
            ChangeAimState(false);
        }*/
    }

    private void OnPlayerControl_Crouch(bindState state)
    {
        if (crouching)
        {
            if (state == bindState.down) CrouchingInput(false);
        }
        else
        {
            if (eqp.isArmed)
            {
                /*if (actWeapon.weapon[actWeapon.slot].entity.data.canCrouch)
                {
                    if (state == bindState.down) CrouchingInput(true);
                }*/
            }
        }
    }

    private void OnDisable()
    {
        //Controls.crouch.OnStateChanged -= OnPlayerControl_Crouch;
    }

    public delegate void AimStateHandler(bool state, bool hasOptics);
    public event AimStateHandler OnAimStateChanged;
    public delegate void SprintStateHandler(bool state);
    public event SprintStateHandler OnSprintStateChanged;
}
