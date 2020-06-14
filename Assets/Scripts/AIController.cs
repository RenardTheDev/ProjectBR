using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    ClothesConfig playerClothes = new ClothesConfig();
    public Actor playerActor;
    public List<Bot> bot = new List<Bot>();

    //public Transform[] spawner;
    public List<SpawnSpot> spawn;

    Vector3 spawnAreaPivot = Vector3.zero;
    public Vector3 spawnAreaSize = Vector3.one;
    public Vector3 spawnAreaSegments = new Vector3(2, 2, 2);
    public float closestSpawn = 10f;
    public float furthestSpawn = 40f;
    float standardFurthestSpawn;
    public WeaponDATA[] RandomizeGUNS;

    private void Awake()
    {
        camtrans = Camera.main.transform;
        standardFurthestSpawn = furthestSpawn;
        UpdateSpawnGridWithHeight();
    }

    private void Start()
    {
        var actors = FindObjectsOfType<Actor>();

        foreach (Actor actor in actors)
        {
            if (!actor.isPlayer)
            {
                bot.Add(new Bot(actor));

                if (RandomizeGUNS.Length > 0)
                {
                    if (actor.standardWeapon == null) actor.standardWeapon = new WeaponDATA[1];
                    actor.standardWeapon[0] = RandomizeGUNS[Random.Range(0, RandomizeGUNS.Length)];
                }

                RespawnBot(actor);
            }
        }

        playerActor = Actor.PLAYERACTOR;
        LoadCustomization();

        GlobalEvents.current.onActorKilled += OnActorKilled;
        GlobalEvents.current.onActorGetHit += OnActorGetHit;
        GlobalEvents.current.onActorRevived += OnActorRevived;
    }

    private void OnDestroy()
    {
        GlobalEvents.current.onActorKilled -= OnActorKilled;
        GlobalEvents.current.onActorGetHit -= OnActorGetHit;
        GlobalEvents.current.onActorRevived -= OnActorRevived;
    }

    private void OnActorKilled(Actor actor, Damage damage)
    {
        if (actor.isPlayer)
        {
            /*List<WeaponDATA> currentWeapons = new List<WeaponDATA>();
            var paw = actor.GetComponent<ActorWeapon>();
            for (int i = 0; i < paw.weapon.Length; i++)
            {
                if (!paw.weapon[i].IsEmpty() && paw.weapon[i].entity.data.type != WeaponType.Melee)
                {
                    currentWeapons.Add(paw.weapon[i].entity.data);
                }
            }

            currentWeapons.Sort(delegate (WeaponDATA x, WeaponDATA y)
            {
                if (x == null && y == null) return 0;
                else if (x == null) return -1;
                else if (y == null) return 1;
                else return y.type.CompareTo(x.type);
            });
            actor.standardWeapon = currentWeapons.ToArray();*/

            StartCoroutine(SlowTime(3f));
        }
        else
        {
            if (Random.value > 0.8f && damage.attacker == playerActor) StartCoroutine(SlowTime(2f));

            if (RandomizeGUNS.Length > 0)
            {
                if (actor.standardWeapon == null) actor.standardWeapon = new WeaponDATA[1];
                actor.standardWeapon[0] = RandomizeGUNS[Random.Range(0, RandomizeGUNS.Length)];
            }
        }
    }

    private void OnActorGetHit(Actor actor, Damage damage)
    {
        if (actor.isPlayer)
        {
            actionHeat += damage.amount * heatMult;
        }
    }

    private void OnActorRevived(Actor actor)
    {
        if (actor.isPlayer)
        {
            foreach (Bot b in bot)
            {
                if (!b.actor.isAlive) continue;

                RespawnBot(b.actor);
                b.agent.SetDestination(playerActor.target.position + Random.insideUnitSphere * 10f);
            }
        }
        else
        {
            RespawnBot(actor);
        }
    }

    Transform camtrans;

    void RespawnBot(Actor actor)
    {
        if(recalculatingSpots == null) recalculatingSpots = StartCoroutine(RecalculateSpawns());

        var goodSpawn = spawn.FindAll(x => x.good);
        if (goodSpawn.Count == 0)
        {
            Debug.Log("No spots found");
            furthestSpawn += 25f;
            RespawnBot(actor);
            return;
        }
        else
        {
            furthestSpawn = standardFurthestSpawn;
        }

        actor.agent.enabled = false;
        actor.transform.position = goodSpawn[Random.Range(0, goodSpawn.Count)].spot;
        actor.agent.enabled = true;

        actor.GetComponent<ActorClothing>().RandomizeClothes();

        actor.Health = actor.maxHealth;
    }

    public float recalcDelay = 0.25f;
    public Coroutine recalculatingSpots;
    IEnumerator RecalculateSpawns()
    {
        for (int i = 0; i < spawn.Count; i++)
        {
            UpdateSpawnSegmentGoodness(i);
        }
        yield return new WaitForSecondsRealtime(recalcDelay);
        recalculatingSpots = null;
    }

    public LayerMask searchMask;
    RaycastHit sHit;
    Vector3 sDir;
    public void UpdateSpawnGrid()
    {
        UpdateSpawnAreaPivot();

        spawnAreaSegments.x = Mathf.Abs(Mathf.Ceil(spawnAreaSegments.x));
        spawnAreaSegments.y = Mathf.Abs(Mathf.Ceil(spawnAreaSegments.y));
        spawnAreaSegments.z = Mathf.Abs(Mathf.Ceil(spawnAreaSegments.z));

        spawn = new List<SpawnSpot>();

        for (int x = 0; x < spawnAreaSegments.x; x++)
        {
            for (int y = 0; y < spawnAreaSegments.y; y++)
            {
                for (int z = 0; z < spawnAreaSegments.z; z++)
                {
                    spawn.Add(new SpawnSpot(
                        new Vector3(
                            spawnAreaPivot.x + (spawnAreaSize.x / spawnAreaSegments.x) * x,
                            spawnAreaPivot.y + (spawnAreaSize.y / spawnAreaSegments.y) * y,
                            spawnAreaPivot.z + (spawnAreaSize.z / spawnAreaSegments.z) * z
                            )));
                }
            }
        }

        for (int i = 0; i < spawn.Count; i++)
        {
            UpdateSpawnSegmentGoodness(i);
        }
    }

    RaycastHit rSpot;
    public void UpdateSpawnGridWithHeight()
    {
        UpdateSpawnAreaPivot();

        spawnAreaSegments.x = Mathf.Abs(Mathf.Ceil(spawnAreaSegments.x));
        spawnAreaSegments.y = Mathf.Abs(Mathf.Ceil(spawnAreaSegments.y));
        spawnAreaSegments.z = Mathf.Abs(Mathf.Ceil(spawnAreaSegments.z));

        spawn = new List<SpawnSpot>();

        for (int x = 0; x < spawnAreaSegments.x; x++)
        {
            for (int y = 0; y < spawnAreaSegments.y; y++)
            {
                for (int z = 0; z < spawnAreaSegments.z; z++)
                {
                    Vector3 tempSpot = new Vector3(
                            spawnAreaPivot.x + (spawnAreaSize.x / spawnAreaSegments.x) * x,
                            spawnAreaPivot.y + (spawnAreaSize.y / spawnAreaSegments.y) * y,
                            spawnAreaPivot.z + (spawnAreaSize.z / spawnAreaSegments.z) * z);


                    if (Physics.Raycast(tempSpot + Vector3.up * spawnAreaSize.y / 2, Vector3.down, out rSpot, 2000))
                    {
                        tempSpot = rSpot.point + Vector3.up;
                        if (Physics.OverlapSphere(tempSpot, 0.4f).Length == 0 && Vector3.Angle(rSpot.normal, Vector2.up) < 45f)
                        {
                            spawn.Add(new SpawnSpot(tempSpot));
                        }
                    }
                }
            }
        }

        for (int i = 0; i < spawn.Count; i++)
        {
            UpdateSpawnSegmentGoodness(i);
        }
    }

    void UpdateSpawnAreaPivot()
    {
        spawnAreaPivot = transform.position - spawnAreaSize * 0.5f;
    }

    void UpdateSpawnSegmentGoodness(int i)
    {
        if (camtrans == null) camtrans = Camera.main.transform;

        sDir = camtrans.position - spawn[i].spot;
        spawn[i].good = sDir.magnitude > closestSpawn && sDir.magnitude < furthestSpawn && Physics.Raycast(spawn[i].spot, sDir, out sHit, sDir.magnitude, searchMask);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);

        if (camtrans == null) camtrans = Camera.main.transform;
        for (int i = 0; i < spawn.Count; i++)
        {
            Gizmos.color = spawn[i].good ? new Color(0, 1, 0, 1f) : new Color(1, 0, 0, 1f);
            Gizmos.DrawWireSphere(spawn[i].spot, 0.4f);
        }
    }

    IEnumerator SlowTime(float wait)
    {
        Time.timeScale = 0.2f;
        yield return new WaitForSecondsRealtime(wait);

        while (PauseMenu.inst.isPaused) yield return new WaitForEndOfFrame();

        Time.timeScale = 1.0f;
    }

    public float actionHeat;
    public float heatMult = 1.0f;
    public LayerMask viewMask;
    private void Update()
    {
        if (bot.Count == 0) return;

        if (actionHeat > 0) actionHeat = Mathf.Lerp(actionHeat, 0f, Time.deltaTime);

        foreach (Bot b in bot)
        {
            if (!b.actor.isAlive) continue;

            if (playerActor.isAlive)
            {
                //movement
                if (b.agent.enabled)
                {
                    if (b.agent.remainingDistance <= b.agent.stoppingDistance)
                    {
                        b.movement = Vector3.Lerp(b.movement, Vector3.zero, Time.deltaTime);
                        b.motor.inpDirection = b.movement;

                        b.agent.SetDestination(playerActor.target.position + Random.insideUnitSphere * 10f);
                    }
                    else
                    {
                        b.movement = Vector3.Lerp(b.movement, PathToMovement(b.agent.path), Time.deltaTime);
                        b.motor.inpDirection = b.movement;
                    }
                }
                else
                {
                    b.motor.inpDirection = Vector3.zero;
                }

                //aiming
                b.aiming = Vector3.Lerp(b.aiming, playerActor.target.position, Time.deltaTime * 10f);
                b.look.LookAtPoint(b.aiming);

                //shooting

                BotShoot(b, false);
                Ray view = new Ray(b.actor.target.position + Vector3.up * 0.5f, (playerActor.target.position - (b.actor.target.position + Vector3.up * 0.5f)).normalized);
                RaycastHit hit;
                if (Physics.Raycast(view, out hit, 1000f, viewMask))
                {
                    Debug.DrawLine(view.origin, hit.point, Color.green);
                    if (hit.transform == playerActor.transform)
                    {
                        if (actionHeat < 100)
                        {
                            if (Random.value * 100 > actionHeat)
                            {
                                BotShoot(b, true);
                            }
                        }
                    }
                }
                else
                {
                    Debug.DrawRay(view.origin, view.direction * 100, Color.red);
                }
            }
            else
            {
                if (b.agent.remainingDistance <= b.agent.stoppingDistance)
                {
                    b.movement = Vector3.Lerp(b.movement, Vector3.zero, Time.deltaTime);
                    b.motor.inpDirection = b.movement;

                    b.agent.SetDestination(Random.insideUnitSphere * 400f);
                }
                else
                {
                    b.movement = Vector3.Lerp(b.movement, PathToMovement(b.agent.path), Time.deltaTime);
                    b.motor.inpDirection = b.movement;
                }

                b.aiming = Vector3.Lerp(b.aiming, b.movement.normalized * 10f + b.actor.target.position, Time.deltaTime * 10f);
                b.look.LookAtPoint(b.aiming);

                BotShoot(b, false);
            }
        }
    }

    void BotShoot(Bot bot, bool state)
    {
        //bot.weapon.currWEntity.inp_trigger = state;
    }

    Vector3 PathToMovement(NavMeshPath path)
    {
        Vector3 move = Vector3.zero;

        if(path.corners.Length > 1)
        {
            move = path.corners[1] - path.corners[0];
            move.y = 0;
        }

        return move.normalized;
    }

    private void LogicUpdate()
    {

    }

    public void LoadCustomization()
    {
        if (File.Exists(Application.persistentDataPath + "/player.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.OpenRead(Application.persistentDataPath + "/player.dat");

            playerClothes = (ClothesConfig)bf.Deserialize(file);
            file.Close();

            foreach (var item in playerClothes.clothing)
            {
                if (item.Value >= 0)
                {
                    playerActor.GetComponent<ActorClothing>().ChangeClothes(item.Key, item.Value);
                }
                else
                {
                    playerActor.GetComponent<ActorClothing>().RemoveClothes(item.Key);
                }
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AIController))]
public class AIControllerEditor : Editor
{
    AIController script;
    public override void OnInspectorGUI()
    {
        if (script == null) script = (AIController)target;

        base.OnInspectorGUI();

        GUILayout.Space(16);
        if (GUILayout.Button("Update spawn grid"))
        {
            script.UpdateSpawnGrid();
        }
        if (GUILayout.Button("Update spawn grid with heights"))
        {
            script.UpdateSpawnGridWithHeight();
        }
        GUILayout.Label("Total spawn segments: " + script.spawn.Count);
        GUILayout.Label("Good spots: " + script.spawn.FindAll(x => x.good).Count);
        GUILayout.Label("Coroutine active: " + (script.recalculatingSpots != null));
    }
}
#endif

[System.Serializable]
public class Bot
{
    public Transform transform;
    public NavMeshAgent agent;
    public Actor actor;
    public ActorLook look;
    public ActorMotor motor;
    public ActorWeapon weapon;

    public NavMeshPath path;

    public Vector3 movement;
    public Vector3 aiming;

    public WeaponDATA gun;

    //public float actionHeat;

    public Bot(Actor actor)
    {
        this.actor = actor;
        transform = actor.transform;
        agent = actor.agent;
        look = actor.GetComponent<ActorLook>();
        motor = actor.GetComponent<ActorMotor>();
        weapon = actor.GetComponent<ActorWeapon>();
    }
}

[System.Serializable]
public class SpawnSpot
{
    public Vector3 spot;
    public bool good;

    public SpawnSpot(Vector3 spot)
    {
        this.spot = spot;
        good = false;
    }
}