using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager current;

    public Transform prefabSpawner;
    public GameObject characterPrefab;

    public List<CharacterPrefab> alive_character = new List<CharacterPrefab>();
    public List<CharacterPrefab> dead_character = new List<CharacterPrefab>();

    public int prebuildCount = 20;

    private void Awake()
    {
        current = this;
    }

    private void Start()
    {
        GenerateCharacters(prebuildCount);

        GlobalEvents.current.onActorKilled += OnActorKilled;
    }

    private void OnActorKilled(Actor actor, Damage damage)
    {
        var cp = alive_character.Find(x => x.actor == actor);
        if (cp == null) { Debug.LogError(name + ": Cant find CharacterPrefab for actor \'" + actor.name + "\'"); return; }

        StartCoroutine(_hideCharacter(cp));
    }

    public Actor SpawnCharacter(Vector3 position, Quaternion rotation, bool isPlayer)
    {
        if (dead_character.Count == 0) GenerateCharacters(1);

        var cp = dead_character[0];

        cp.gObj.SetActive(true);

        alive_character.Add(cp);
        dead_character.Remove(cp);

        cp.actor.Revive();

        if (isPlayer)
        {
            if (cp.gObj.GetComponent<CE_Regeneration>() == null)
            {
                var regen = cp.gObj.AddComponent<CE_Regeneration>();

                regen.maxRegenLevel = 1f;
                regen.waitToActivate = 5f;
                regen.regenSpeed = 5f;
                regen.regenType = regenType.percent;
            }

            //cp.look.aa_Power = 0.3f;
            if (cp.agent != null) Destroy(cp.agent);

            cp.clothing.LoadPlayerCustomization();

            PlayerUI.current.AssignPlayer(cp.actor);
            CameraControllerBase.current.SetActorTarget(cp.actor);

            cp.actor.MarkAsPlayer(true);

            cp.actor.ActorName = "Player";
        }
        else
        {
            if (cp.agent == null) cp.agent = cp.gObj.AddComponent<NavMeshAgent>();
            ConfigNavMeshAgent(cp.agent);
            cp.agent.Warp(position);

            cp.clothing.RandomizeClothes();

            cp.actor.MarkAsPlayer(false);

            cp.actor.ActorName = "TheDude#" + Random.Range(0, 1000).ToString("0000");
        }

        cp.gObj.transform.position = position;
        cp.gObj.transform.rotation = rotation;

        cp.look.heading = rotation.eulerAngles.y;

        return cp.actor;
    }

    void ConfigNavMeshAgent(NavMeshAgent agent)
    {
        agent.baseOffset = 0.08f;

        agent.speed = 0f;
        agent.angularSpeed = 0;
        agent.acceleration = 0;
        agent.stoppingDistance = 0.5f;

        agent.radius = 0.35f;
        agent.height = 1.8f;
    }

    IEnumerator _hideCharacter(CharacterPrefab character)
    {
        yield return new WaitForSecondsRealtime(10f);
        HideCharacter(character);
    }

    void HideCharacter(CharacterPrefab character)
    {
        var regen = character.gObj.GetComponent<CE_Regeneration>();
        if (regen != null)
        {
            Destroy(regen);
        }

        character.gObj.SetActive(false);

        if (alive_character.Contains(character) && !dead_character.Contains(character))
        {
            alive_character.Remove(character);
            dead_character.Add(character);
        }

        //---bonework---
        foreach (var item in character.ragdoll.bones)
        {
            item.trans.localRotation = Quaternion.identity;
        }
    }

    void GenerateCharacters(int count)
    {
        if (count <= 0) return;

        for (int i = 0; i < count; i++)
        {
            var pref = Instantiate(characterPrefab, prefabSpawner.position, prefabSpawner.rotation);
            dead_character.Add(new CharacterPrefab(pref));
        }

        for (int i = 0; i < count; i++)
        {
            HideCharacter(dead_character[i]);
        }
    }
}

[System.Serializable]
public class CharacterPrefab
{
    public GameObject gObj;

    public Actor actor;
    public ActorClothing clothing;
    public ActorLook look;
    public ActorMotor motor;
    public ActorRagdoll ragdoll;
    public ActorWeapon weapon;
    public NavMeshAgent agent;

    public CharacterPrefab(GameObject go)
    {
        gObj = go;

        actor = gObj.GetComponent<Actor>();
        clothing = gObj.GetComponent<ActorClothing>();
        look = gObj.GetComponent<ActorLook>();
        motor = gObj.GetComponent<ActorMotor>();
        ragdoll = gObj.GetComponent<ActorRagdoll>();
        weapon = gObj.GetComponent<ActorWeapon>();
    }
}