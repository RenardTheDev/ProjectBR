using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEffectBase : MonoBehaviour
{
    public Actor actor;
    public bool Disposable = false; // auto remove effect after time
    public float disposeTime = 0f;  // dispose after this amount of time

    public virtual void ApplyEffect(Actor actor)
    {
        this.actor = actor;
    }

    public virtual void OnEffectDisposed()
    {

    }
}
