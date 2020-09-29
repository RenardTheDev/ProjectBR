using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Actor : MonoBehaviour
{
    public static Actor PLAYERACTOR;
    ActorEvents actEvents;

    public string ActorName = "Actor";

    public bool isPlayer;
    public bool isAlive;

    public int team = 0;

    public float maxHealth = 100f;
    public float Health = 100f;

    public bool isInvulnerable;

    public float lastDamageTime;

    public Transform target;
    public Transform hips;

    public WeaponDATA fists;
    public WeaponDATA[] standardWeapon;

    public NavMeshAgent agent;

    [Range(0, 1)] public float Accuracy = 1;

    private void Awake()
    {
        actEvents = GetComponent<ActorEvents>();
        agent = GetComponent<NavMeshAgent>();

        Health = maxHealth;
        isAlive = true;

        if (isPlayer)
        {
            PLAYERACTOR = this;
            PlayerUI.current.AssignPlayer(this);
            InventoryUI.current.AssignPlayer(this);
        }
    }

    private void Start()
    {
        var actWeap = GetComponent<ActorWeapon>();
    }

    public void ApplyHealth(float amount)
    {
        if (Health < maxHealth)
        {
            float heal = (Health + amount > maxHealth) ? maxHealth - Health : amount;
            Health += heal;

            GlobalEvents.current.ActorHealed(this, heal);
            actEvents.ActorHealed(this, heal);
        }
    }

    public void MarkAsPlayer(bool state)
    {
        isPlayer = state;
        tag = state ? "Player" : "Untagged";
        if (state)
        {
            PLAYERACTOR = this;

            InventoryUI.current.AssignPlayer(this);
        }
    }

    public void ApplyDamage(Damage dmg)
    {
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            Debug.Log(name + "_ApplyDamage(" + dmg.amount.ToString("0.00") + ")");
        }

        if (isAlive)
        {
            lastDamageTime = Time.time;

            if (Health > dmg.amount)
            {
                if (!isInvulnerable) Health -= dmg.amount;

                GlobalEvents.current.ActorGetHit(this, dmg);
                actEvents.ActorGetHit(this, dmg);
            }
            else
            {
                if (!isInvulnerable) ActorKilled(dmg);
            }
        }
    }

    void ActorKilled(Damage dmg)
    {
        isAlive = false;
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            if (dmg.attacker != null) Debug.Log(dmg.attacker.ActorName + " killed " + ActorName);
        }

        dmg.amount = Health;
        Health = 0;
        GlobalEvents.current.ActorKilled(this, dmg);
        actEvents.ActorKilled(this, dmg);

        if (agent != null) agent.enabled = false;
    }

    private void OnValidate()
    {
        isPlayer = CompareTag("Player");
    }

    public void Revive()
    {
        Health = maxHealth;
        isAlive = true;

        GlobalEvents.current.ActorRevived(this);
        actEvents.ActorRevived(this);

        if (agent != null) agent.enabled = true;
    }
}

public class Damage
{
    public Actor attacker;
    public Transform bone;
    public float amount;
    public Vector3 point;
    public Vector3 direction;
    public WeaponDATA weapon;

    public Damage(float amount, Vector3 point, Vector3 direction, WeaponDATA weapon, Actor attacker = null, Transform bone=null)
    {
        this.attacker = attacker;
        this.amount = amount;
        this.point = point;
        this.direction = direction;
        this.weapon = weapon;
        this.bone = bone;
    }
}