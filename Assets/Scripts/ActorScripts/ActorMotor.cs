using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorMotor : MonoBehaviour
{
    Actor actor;
    ActorEvents events;
    ActorLook look;
    Animator animator;
    ActorWeapon actWeapon;
    CharacterController character;

    public Transform weaponHolder;
    public float crouchSpeed = 1.5f;
    public float runSpeed = 4f;
    public float sprintSpeed = 6f;
    public float currentSpeed;

    public bool crouching;
    public bool sprinting;
    public bool aiming;

    public float dirLock_fade;

    Vector3 movement;

    private void Awake()
    {
        actor = GetComponent<Actor>();
        events= GetComponent<ActorEvents>();
        look = GetComponent<ActorLook>();
        animator = GetComponent<Animator>();
        actWeapon = GetComponent<ActorWeapon>();
        character = GetComponent<CharacterController>();

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
                TouchController.current.button_changeGun.OnStateChanged += OnPlayerControl_ChangeGun;
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

    private void Update()
    {
        if (actor.isPlayer)
        {
            inpDirection = new Vector3(Controls.move.x, 0, Controls.move.y);
            if (sprinting)
            {
                inpDirection = inpDirection.normalized;
                
            }
        }

        dirLock_fade = Mathf.Lerp(dirLock_fade, (sprinting || !actWeapon.isArmed) ? 1f : 0f, Time.deltaTime * 3);

        if (actWeapon.currWData.type == WeaponType.Melee)
        {
            if (sprinting && inpDirection.magnitude < 0.1f)
            {
                ChangeSprintState(false);
            }
        }
        else
        {
            if (sprinting && (Vector3.Angle(inpDirection, Vector3.forward) > 45f || inpDirection.magnitude < 0.1f))
            {
                ChangeSprintState(false);
            }
        }

        if (inpDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inpDirection.x, inpDirection.z) * Mathf.Rad2Deg + look.heading;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0, angle, 0);

            moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
        }
        else
        {
            moveDir = Vector3.zero;
        }

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

            if (actWeapon.currWEntity != null)
            {
                currentSpeed = Mathf.Clamp(currentSpeed, 0, actWeapon.weapon[actWeapon.slot].entity.data.speedCap);
            }
            if (aiming)
            {
                currentSpeed = Mathf.Clamp(currentSpeed, 0, crouchSpeed);
            }
        }

        movement = moveDir * currentSpeed * Time.deltaTime;
        movement.y = -9.8f * Time.deltaTime;

        if (character.enabled && animator.enabled)
        {
            if (animator.GetFloat("rootm") <= 0)
            {
                character.Move(movement);
            }

            animator.SetBool("crouching", crouching);
            animator.SetFloat("move_x", transform.InverseTransformVector(character.velocity).x, 0.1f, Time.deltaTime);
            animator.SetFloat("move_z", transform.InverseTransformVector(character.velocity).z, 0.1f, Time.deltaTime);
        }
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
            actWeapon.currWEntity.OnAimStateChanged(state && actor.isPlayer);
            OnAimStateChanged?.Invoke(state, actWeapon.currWData.hasScope);
        }
        else
        {
            if (!sprinting && !actWeapon.IsCurrEntityEmpty() && !actWeapon.currWEntity.reloading)
            {
                aiming = true;
                actWeapon.currWEntity.OnAimStateChanged(state && actor.isPlayer);
                OnAimStateChanged?.Invoke(state, actWeapon.currWData.hasScope);
            }
        }
    }

    private void OnPlayerControl_Sprint(bindState state)
    {
        if (state == bindState.down)
        {
            if (!sprinting)
            {
                if (Vector3.Angle(inpDirection, Vector3.forward) < 45f && !crouching && inpDirection.sqrMagnitude > 0.1f)
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

    private void OnPlayerControl_ChangeGun(bindState state)
    {
        if (state == bindState.down)
        {
            if (actWeapon.slot == 0)
            {
                if (!actWeapon.IsEntityEmpty(1))
                {
                    actWeapon.ChangeSlot(1);
                }
            }
            else
            {
                if (!actWeapon.IsEntityEmpty(0))
                {
                    actWeapon.ChangeSlot(0);
                }
            }

            ChangeAimState(false);
        }
    }

    private void OnPlayerControl_Crouch(bindState state)
    {
        if (crouching)
        {
            if (state == bindState.down) CrouchingInput(false);
        }
        else
        {
            if (actWeapon.isArmed)
            {
                if (actWeapon.weapon[actWeapon.slot].entity.data.canCrouch)
                {
                    if (state == bindState.down) CrouchingInput(true);
                }
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
