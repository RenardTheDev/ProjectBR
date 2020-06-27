using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorEvents : MonoBehaviour
{
    //--- WEAPON EVENTS ---
    public event Action<WeaponDATA> onWeaponShot;
    public void WeaponShot(WeaponDATA data) { onWeaponShot?.Invoke(data); }

    public event Action onWeaponChambered;
    public void WeaponChambered() { onWeaponChambered?.Invoke(); }

    public event Action onWeaponShellInsert;
    public void WeaponShellInsert() { onWeaponShellInsert?.Invoke(); }

    public event Action onWeaponReloadStart;
    public void WeaponReloadStart() { onWeaponReloadStart?.Invoke(); }

    public event Action onWeaponReloadEnd;
    public void WeaponReloadEnd() { onWeaponReloadEnd?.Invoke(); }

    //--- DAMAGE EVENTS ---
    public event Action<Actor, Damage> onActorGetHit;
    public void ActorGetHit(Actor actor, Damage damage) { onActorGetHit?.Invoke(actor, damage); }

    public event Action<Actor, Damage> onActorKilled;
    public void ActorKilled(Actor actor, Damage damage) { onActorKilled?.Invoke(actor, damage); }

    public event Action<Actor> onActorRevived;
    public void ActorRevived(Actor actor) { onActorRevived?.Invoke(actor); }

    public event Action<Actor, float> onActorHealed;
    public void ActorHealed(Actor actor, float amount) { onActorHealed?.Invoke(actor, amount); }
}