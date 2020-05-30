using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CE_Regeneration : CharacterEffectBase
{
    public float maxRegenLevel = 0.75f; // max precent of MaxHealth to regen
    public float waitToActivate = 5f;   // time from last damage to start regen
    public float regenSpeed = 1f;       // health point / percent per second
    public regenType regenType = regenType.hp;    //regen type

    float lastDamageTime = 0f;

    CharacterController actorChar;

    private void Awake()
    {
        
    }

    private void Start()
    {
        ApplyEffect(GetComponent<Actor>());
        actorChar = actor.GetComponent<CharacterController>();
        StartCoroutine(Regenerate());
    }

    private void OnDestroy()
    {
        GlobalEvents.current.onActorGetHit -= OnActorTakeDamage;
    }

    public override void ApplyEffect(Actor actor)
    {
        base.ApplyEffect(actor);

        GlobalEvents.current.onActorGetHit += OnActorTakeDamage;

        if (Application.platform == RuntimePlatform.WindowsEditor)
            Debug.Log(name + ": applied \'Regeneration\'\n" +
                "maxRegenLevel = " + maxRegenLevel + "\n" +
                "waitToActivate = " + waitToActivate + "\n" +
                "regenSpeed = " + regenSpeed + "\n" +
                "regenType = " + regenType.ToString());
    }

    private void OnActorTakeDamage(Actor actor, Damage damage)
    {
        if (actor != this.actor) return;

        lastDamageTime = Time.time;
    }

    IEnumerator Regenerate()
    {
        if (actor.isAlive && Time.time > lastDamageTime + waitToActivate && actor.Health < maxRegenLevel * actor.maxHealth)
        {
            float heal = 0;
            switch (regenType)
            {
                case regenType.hp:
                    heal = (actor.Health + regenSpeed > maxRegenLevel * actor.maxHealth)
                        ? maxRegenLevel * actor.maxHealth - actor.Health : regenSpeed;
                    actor.ApplyHealth(heal);
                    break;

                case regenType.percent:
                    heal = (actor.Health + (regenSpeed * 0.01f * actor.maxHealth) > maxRegenLevel * actor.maxHealth)
                        ? maxRegenLevel * actor.maxHealth - actor.Health : regenSpeed * 0.01f * actor.maxHealth;
                    actor.ApplyHealth(heal);
                    break;
            }
            ParticlesManager.inst.Effect_heal(actor.target.position, actorChar.velocity);

            yield return new WaitForSeconds(1f);
        }
        else
        {
            yield return new WaitForFixedUpdate();
        }

        StartCoroutine(Regenerate());
    }

    public override void OnEffectDisposed()
    {
        base.OnEffectDisposed();

        if (Application.platform == RuntimePlatform.WindowsEditor)
            Debug.Log(name + ": disposed \'Regeneration\'");
    }
}

public enum regenType
{
    hp,
    percent
}