using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorEvents : MonoBehaviour
{
    //--- WEAPON EVENTS ---
    public event Action<WeaponDATA> onWeaponShot;
    public void WeaponShot(WeaponDATA data) { if (onWeaponShot != null) onWeaponShot(data); }

    public event Action onWeaponChambered;
    public void WeaponChambered() { if (onWeaponChambered != null) onWeaponChambered(); }

    public event Action onWeaponShellInsert;
    public void WeaponShellInsert() { if (onWeaponShellInsert != null) onWeaponShellInsert(); }

    public event Action onWeaponReloadStart;
    public void WeaponReloadStart() { if (onWeaponReloadStart != null) onWeaponReloadStart(); }

    public event Action onWeaponReloadEnd;
    public void WeaponReloadEnd() { if (onWeaponReloadEnd != null) onWeaponReloadEnd(); }

    //--- DAMAGE EVENTS ---
    public event Action<Actor, Damage> onActorGetHit;
    public void ActorGetHit(Actor actor, Damage damage) { if (onActorGetHit != null) onActorGetHit(actor, damage); }

    public event Action<Actor, Damage> onActorKilled;
    public void ActorKilled(Actor actor, Damage damage) { if (onActorKilled != null) onActorKilled(actor, damage); }

    public event Action<Actor> onActorRevived;
    public void ActorRevived(Actor actor) { if (onActorRevived != null) onActorRevived(actor); }

    public event Action<Actor, float> onActorHealed;
    public void ActorHealed(Actor actor, float amount) { if (onActorHealed != null) onActorHealed(actor, amount); }
}