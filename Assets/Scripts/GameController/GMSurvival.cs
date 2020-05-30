using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class GMSurvival : MonoBehaviour
{
    public static GMSurvival current;

    public Transform PlayerSpawner;
    public Transform[] EnemySpawner;

    //---Player stats---
    public int kills;
    public float score;
    public int money;

    //---UI Stats---
    public Text label_wave;
    public Text label_kills;
    public Text label_score;

    //---Controller data---
    public int Wave;
    public List<Actor> enemies = new List<Actor>();
    public int enemiesLeft { get => enemies.Count; }

    //---UI---

    private void Awake()
    {
        current = this;
    }

    private void Start()
    {
        GlobalEvents.current.onActorKilled += OnActorKilled;
        GlobalEvents.current.onActorGetHit += OnActorHit;

        StartCoroutine(GameStart());
    }

    Coroutine cor_gameStart;
    IEnumerator GameStart()
    {
        label_wave.text = "Wave: <color=#FF9632>" + Wave + "</color>";

        CameraControllerBase.current.ChangeToState("Cinematic", 0);

        CharacterManager.current.SpawnCharacter(PlayerSpawner.position, PlayerSpawner.rotation, true);
        PlayerUI.current.ToggleUIElement(ui_element.all, false);
        PlayerUI.current.ToggleUIElement(ui_element.messages, true);

        yield return new WaitForSeconds(2f);

        PlayerUI.current.ShowBigCenterMSG("SURVIVAL MODE", col_attention, "Survive 10 rounds to win", 3, 1);

        CameraControllerBase.current.ChangeToState("idle", 0);

        yield return new WaitForSeconds(5f);

        PlayerUI.current.ToggleUIElement(ui_element.all, true);
        PlayerUI.current.ToggleUIElement(ui_element.inventory, false);
        StartCoroutine(WavePreparation());
    }

    Coroutine cor_wavePreparation;
    IEnumerator WavePreparation()
    {
        PlayerUI.current.ShowBigCenterMSG("Next wave incoming!", col_attention, "", 1, 0.5f);

        yield return new WaitForSeconds(3f);
        StartCoroutine(WaveStart());
    }

    Coroutine cor_waveStart;
    IEnumerator WaveStart()
    {
        for (int i = 0; i < 3 + Wave * 2; i++)
        {
            var spawn = EnemySpawner[0].position + Random.onUnitSphere * 5;
            spawn.y = EnemySpawner[0].position.y;
            var act = CharacterManager.current.SpawnCharacter(spawn, Quaternion.Euler(0, Random.value * 360f, 0), false);

            enemies.Add(act);
        }

        yield return new WaitForSeconds(1f);

        if (cor_waveCycle != null) StopCoroutine(cor_waveCycle);
        cor_waveCycle = StartCoroutine(WaveCycle());
    }

    Coroutine cor_waveCycle;
    IEnumerator WaveCycle()
    {
        if (enemies.Count == 0 && ProjectileManager.current.ActiveProj.Count == 0)
        {
            //---WaveEnded---
            yield return new WaitForSeconds(1f);
            if (Actor.PLAYERACTOR.isAlive)
            {
                if (enemies.Count == 0) StartCoroutine(WaveComplete());
            }
            else
            {
                StartCoroutine(GameOver());
            }
        }
        else
        {
            //---Cycle---
            yield return new WaitForEndOfFrame();

            if (Actor.PLAYERACTOR.isAlive)
            {
                cor_waveCycle = StartCoroutine(WaveCycle());
            }
            else
            {
                StartCoroutine(GameOver());
            }
        }
    }

    Coroutine cor_waveComplete;
    IEnumerator WaveComplete()
    {
        PlayerUI.current.ShowBigCenterMSG("Wave complete", col_win, "", 2, 0.5f);

        PlayerUI.current.ToggleUIElement(ui_element.all, false);
        CameraControllerBase.current.ChangeToState("waveEnd_0", 0);
        yield return new WaitForSeconds(0.25f);

        Actor.PLAYERACTOR.GetComponent<ActorMotor>().ChangeAimState(false);
        ActorWeapon aw = Actor.PLAYERACTOR.GetComponent<ActorWeapon>();
        if (aw.isArmed) aw.currWEntity.TryToReload();

        yield return new WaitForSeconds(3.75f);
        PlayerUI.current.ToggleUIElement(ui_element.all, true);

        Wave++;
        label_wave.text = "Wave: <color=#FF9632>" + Wave + "</color>";
        /*if (cor_gameMsg != null) StopCoroutine(cor_gameMsg);
        cor_gameMsg = StartCoroutine(WavePreparation());*/
    }

    Coroutine cor_gameOver;
    IEnumerator GameOver()
    {
        PlayerUI.current.ShowBigCenterMSG("WASTED", col_lose, "", 1, 0.5f);

        Time.timeScale = 0.1f;
        yield return new WaitForSeconds(3f);
        Time.timeScale = 1f;
        yield return new WaitForSeconds(5f);
        SceneFade.current.FadeToMenu(1f);
    }

    private void OnActorKilled(Actor victim, Damage dmg)
    {
        if (victim.isPlayer)
        {
            //StartCoroutine(GameOver());
        }
        else
        {
            if (enemies.Contains(victim))
            {
                enemies.Remove(victim);
            }
        }
    }

    private void OnActorHit(Actor victim, Damage dmg)
    {

    }

    private void Update()
    {
        if (action.Count > 0) action.RemoveAll(x => Time.time - x.updated > reportTimeLength);

        action_log.text = "";

        var sortedAction = action.OrderBy(o => o.type).ToList();
        foreach (var i in sortedAction)
        {
            switch (i.type)
            {
                case ActionType.sum:
                    {
                        var sumAct = (ActionRecordSum)i.item;
                        action_log.text += "<size=" + Mathf.RoundToInt(action_log.fontSize * 1.4f) + ">" + sumAct.sum.ToString("0") + "</size>\n";
                        break;
                    }
                case ActionType.damage:
                    {
                        var damAct = (ActionRecordDamage)i.item;
                        action_log.text += "<color=#FF9632>+" + damAct.damage.ToString("0") + "</color> <size=" + Mathf.RoundToInt(action_log.fontSize * 0.6f) + ">enemy hit</size>\n";
                        break;
                    }
                case ActionType.kill:
                    {
                        var killAct = (ActionRecordKill)i.item;
                        action_log.text += "<color=#FF9632>+" + killAct.victim.maxHealth + "</color> <size=" + Mathf.RoundToInt(action_log.fontSize * 0.6f) + ">killed</size> <color=#FF9632>" + killAct.victim.ActorName + "</color>\n";
                        break;
                    }
            }
        }
    }

    public void ActionLog_Damage(Actor victim, Damage damage, bool isKill)
    {
        float scoreCalc = damage.amount;

        //---DAMAGE LOG---
        var damageAction = action.Find(x => x.type == ActionType.damage);
        if (damageAction != null)
        {
            damageAction.updated = Time.time;
            ((ActionRecordDamage)damageAction.item).damage += scoreCalc;
        }
        else
        {
            action.Add(new ActionRecordItem(ActionType.damage, Time.time, new ActionRecordDamage(scoreCalc)));
        }

        //---KILL LOG---
        if (isKill)
        {
            action.Add(new ActionRecordItem(ActionType.kill, Time.time, new ActionRecordKill(victim)));
            GMSurvival.current.kills++;
            scoreCalc += victim.maxHealth;
        }

        GMSurvival.current.score += scoreCalc;

        //---SUM LOG---
        var sumAction = action.Find(x => x.type == ActionType.sum);
        if (sumAction != null)
        {
            sumAction.updated = Time.time;
            ((ActionRecordSum)sumAction.item).sum += scoreCalc;
        }
        else
        {
            action.Add(new ActionRecordItem(ActionType.sum, Time.time, new ActionRecordSum(scoreCalc)));
        }

        label_kills.text = "Kills: <color=#FA4B32>" + kills + "</color>";
        label_score.text = "Score: <color=#00AF00>" + score.ToString("0") + "</color>";
    }


    [Header("Game message colors")]
    public Color col_win;
    public Color col_lose;
    public Color col_attention;

    [Header("Action log")]
    public Text action_log;
    public float reportTimeLength = 10f;
    public List<ActionRecordItem> action = new List<ActionRecordItem>();
}

[System.Serializable]
public class ActionRecordItem
{
    public ActionType type;
    public float updated;
    public object item;

    public ActionRecordItem(ActionType type, float updated, object item)
    {
        this.type = type;
        this.updated = updated;
        this.item = item;
    }
}
public class ActionRecordDamage
{
    public float damage;

    public ActionRecordDamage(float damage)
    {
        this.damage = damage;
    }
}
public class ActionRecordKill
{
    public Actor victim;

    public ActionRecordKill(Actor victim)
    {
        this.victim = victim;
    }
}
public class ActionRecordSum
{
    public float sum;

    public ActionRecordSum(float sum)
    {
        this.sum = sum;
    }
}

public enum ActionType
{
    sum, damage, kill
}