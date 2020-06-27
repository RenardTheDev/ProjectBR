using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalEvents : MonoBehaviour
{
    public static GlobalEvents current;

    private void Awake()
    {
        if (current == null)
        {
            current = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public event Action<Actor,Damage> onActorGetHit;
    public void ActorGetHit(Actor actor, Damage damage)
    {
        onActorGetHit?.Invoke(actor, damage);
    }

    public event Action<Actor, Damage> onActorKilled;
    public void ActorKilled(Actor actor, Damage damage)
    {
        onActorKilled?.Invoke(actor, damage);
    }

    public event Action<Actor> onActorRevived;
    public void ActorRevived(Actor actor)
    {
        onActorRevived?.Invoke(actor);
    }

    public event Action<Actor,float> onActorHealed;
    public void ActorHealed(Actor actor, float amount)
    {
        onActorHealed?.Invoke(actor, amount);
    }
}
