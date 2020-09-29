using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GMFreeroam : MonoBehaviour
{
    public static GMFreeroam current;

    public Transform PlayerSpawner;

    private void Awake()
    {
        current = this;
    }

    private void Start()
    {
        GlobalEvents.current.onActorKilled += OnActorKilled;
        GlobalEvents.current.onActorGetHit += OnActorGetHit;

        StartCoroutine(GameStart());
    }

    IEnumerator GameStart()
    {
        yield return new WaitForSeconds(5f);
    }

    private void OnActorGetHit(Actor victim, Damage arg2)
    {

    }

    private void OnActorKilled(Actor victim, Damage arg2)
    {

    }
}