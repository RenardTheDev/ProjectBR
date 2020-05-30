using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalEvents : MonoBehaviour
{
    public static GlobalEvents current;

    private void Awake()
    {
        current = this;
    }

    public event Action<Actor,Damage> onActorGetHit;
    public void ActorGetHit(Actor actor, Damage damage)
    {
        if (onActorGetHit != null)
        {
            onActorGetHit(actor, damage);
        }
    }

    public event Action<Actor, Damage> onActorKilled;
    public void ActorKilled(Actor actor, Damage damage)
    {
        if (onActorKilled != null)
        {
            onActorKilled(actor, damage);
        }
    }

    public event Action<Actor> onActorRevived;
    public void ActorRevived(Actor actor)
    {
        if (onActorRevived != null)
        {
            onActorRevived(actor);
        }
    }

    public event Action<Actor,float> onActorHealed;
    public void ActorHealed(Actor actor, float amount)
    {
        if (onActorHealed != null)
        {
            onActorHealed(actor, amount);
        }
    }
}
