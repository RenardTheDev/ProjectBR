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
        CameraControllerBase.current.ChangeToState("Cinematic", 0);

        CharacterManager.current.SpawnCharacter(PlayerSpawner.position, PlayerSpawner.rotation, true);
        PlayerUI.current.ToggleUIElement(player_ui_element.all, false);

        yield return new WaitForSeconds(2f);

        PlayerUI.current.ShowBigCenterMSG("FREEROAM", Color.white, "", 3, 1);

        CameraControllerBase.current.ChangeToState("idle", 0);

        yield return new WaitForSeconds(5f);

        PlayerUI.current.ToggleUIElement(player_ui_element.all, true);
    }

    private void OnActorGetHit(Actor victim, Damage arg2)
    {

    }

    private void OnActorKilled(Actor victim, Damage arg2)
    {

    }
}