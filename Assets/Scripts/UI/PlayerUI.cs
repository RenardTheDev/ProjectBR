using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI current;
    public GameObject[] ui_elements;

    //---Specific parts---
    public GameObject ui_damageIndicators;
    public GameObject ui_health;
    public GameObject ui_weapon;
    public GameObject ui_crosshair;
    public GameObject ui_hitmarker;

    Camera mainCamera;

    float scrRatio;

    [Header("Health")]
    public Image hpBar;
    float lastDamageTime;
    public RawImage lowHealthTint;
    public RawImage damageTint;
    public float lowHPperc = 0.2f;

    [Header("Damage")]
    public Color dmgIndColor;
    public GameObject dmgIndPrefab;
    public Transform IndicatorsPivot;
    public List<DamageIndicator> dmgIndicator;

    [Header("Hitmarker")]
    public GameObject hm_base;
    float hm_scrTime = 0.2f;
    Image[] hm_parts;

    [Header("Weapon")]
    public GameObject weaponPanel;
    public Vector3 wPanelOffset;
    public Image weaponIcon_first;
    public Image clipFill;
    public Text clip;
    public Color activeClip = Color.white;
    public Color emptyClip = Color.grey;

    RectTransform wPanelTrans;
    RectTransform crosshair;
    WeaponSlot slot;
    public int clipFontSize = 32;
    public int ammoFontSize = 24;

    //[Header("Inventory")]
    bool isInvOpen;

    Actor playerActor;
    ActorInventory playerInv;
    ActorEvents playerEvents;
    ActorMotor playerMotor;
    ActorWeapon playerWeapon;
    ActorEquipment playerEqp;

    private void Awake()
    {
        current = this;

        mainCamera = Camera.main;

        wPanelTrans = weaponPanel.GetComponent<RectTransform>();

        dmgIndicator = new List<DamageIndicator>();
        for (int i = 0; i < 10; i++)
        {
            dmgIndicator.Add(new DamageIndicator(Instantiate(dmgIndPrefab, IndicatorsPivot)));
        }
        dmgIndPrefab.SetActive(false);

        hm_parts = hm_base.GetComponentsInChildren<Image>();
        crosshair = ui_crosshair.GetComponent<RectTransform>();

        ToggleWeaponUI(false);
    }

    private void Start()
    {
        GlobalEvents.current.onActorGetHit += OnActorGetHit;
        GlobalEvents.current.onActorHealed += OnActorHealed;
        GlobalEvents.current.onActorKilled += OnActorKilled;
        GlobalEvents.current.onActorRevived += OnActorRevived;
    }

    private void OnDestroy()
    {
        GlobalEvents.current.onActorGetHit -= OnActorGetHit;
        GlobalEvents.current.onActorHealed -= OnActorHealed;
        GlobalEvents.current.onActorKilled -= OnActorKilled;
        GlobalEvents.current.onActorRevived -= OnActorRevived;
    }

    private void LateUpdate()
    {
        
    }

    float lowHP;
    private void Update()
    {
        if (playerActor == null)
        {
            foreach (var item in ui_elements)
            {
                if (item.activeSelf)
                {
                    ToggleUIElement(player_ui_element.all, false);
                    break;
                }
            }
            return;
        }

        scrRatio = (float)Screen.width / Screen.height;

        //---HEALTH---

        if (playerActor.isAlive)
        {
            lowHP = Mathf.MoveTowards(lowHP, 1f - hpBar.fillAmount / lowHPperc, Time.deltaTime);
        }
        else
        {
            lowHP = Mathf.MoveTowards(lowHP, 0, Time.deltaTime * 0.25f);
        }

        lowHealthTint.enabled = lowHP > 0;
        lowHealthTint.color = new Color(1, 1, 1, Mathf.Lerp(0, Mathf.Pow(Mathf.Sin(Time.time * 5), 4) * 0.3f + 0.7f, Mathf.Pow(lowHP, 0.5f)));

        //---DAMAGE---

        for (int i = 0; i < dmgIndicator.Count; i++)
        {
            var ind = dmgIndicator[i];

            if (ind.isActive)
            {
                ind.time += Time.deltaTime;

                if (ind.time >= 1f)
                {
                    ind.sprite.enabled = false;
                    ind.isActive = false;
                }

                var col = ind.sprite.color;
                col.a = 1f - (ind.time / 1f);
                ind.sprite.color = col;

                float dir = Quaternion.LookRotation(
                    Vector3.ProjectOnPlane(playerActor.transform.position - ind.from, Vector3.up
                        ).normalized).eulerAngles.y;

                ind.indTrans.rotation = Quaternion.Euler(0, 0, -dir + mainCamera.transform.eulerAngles.y);
            }
        }

        //---WEAPON---
        if (crosshair.gameObject.activeSelf)
        {
            if (playerEqp.isArmed)
            {
                crosshair.sizeDelta = new Vector2(playerEqp.currSlot.entity.spread * 64 + 8, playerEqp.currSlot.entity.spread * 64 + 8);
                
                if (playerMotor.aiming && playerEqp.currSlot.entity.data.hasScope)
                {
                    crosshair.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            if (playerEqp.isArmed && !(playerMotor.aiming && playerEqp.currSlot.entity.data.hasScope)) crosshair.gameObject.SetActive(true);
        }
    }

    private void OnActorGetHit(Actor actor, Damage damage)
    {
        if (actor.isPlayer)
        {
            hpBar.fillAmount = playerActor.Health / playerActor.maxHealth;
            lastDamageTime = Time.time;

            //int id = IsAttackerMarked(damage.attacker.transform);
            int id = -1;
            if (id == -1)
            {
                for (int i = 0; i < dmgIndicator.Count; i++)
                {
                    if (!dmgIndicator[i].isActive)
                    {
                        id = i;
                        break;
                    }
                }
            }

            if (id == -1) id = 0;

            dmgIndicator[id].time = 0;
            dmgIndicator[id].isActive = true;
            dmgIndicator[id].sprite.enabled = true;
            dmgIndicator[id].sprite.color = dmgIndColor;
            dmgIndicator[id].from = playerActor.transform.position + damage.direction * 100f;
        }
        else
        {
            if (damage.attacker.isPlayer && !actor.isPlayer)
            {
                GMSurvival.current.ActionLog_Damage(actor, damage, false);

                for (int i = 0; i < hm_parts.Length; i++)
                {
                    hm_parts[i].color = Color.white;
                }

                if (hm_coroutine != null) StopCoroutine(hm_coroutine);
                hm_coroutine = StartCoroutine(ProcessHitmarker());
            }
        }
    }

    Coroutine hm_coroutine;
    IEnumerator ProcessHitmarker()
    {
        hm_base.SetActive(true); 
        for (int i = 0; i < hm_parts.Length; i++)
        {
            hm_parts[i].fillAmount = 1;
        }

        while (hm_parts[0].fillAmount > 0)
        {
            yield return new WaitForEndOfFrame();
            for (int i = 0; i < hm_parts.Length; i++)
            {
                hm_parts[i].fillAmount = Mathf.MoveTowards(hm_parts[i].fillAmount, 0, Time.deltaTime);
            }
        }

        hm_base.SetActive(false);
    }

    private void OnActorHealed(Actor actor, float amount)
    {
        if (actor != playerActor) return;

        hpBar.fillAmount = playerActor.Health / playerActor.maxHealth;
    }

    private void OnActorKilled(Actor actor, Damage damage)
    {
        if (actor == playerActor)
        {
            ToggleUIElement(player_ui_element.all, false);
        }
        else
        {
            GMSurvival.current.ActionLog_Damage(actor, damage, true);

            for (int i = 0; i < hm_parts.Length; i++)
            {
                hm_parts[i].color = Color.red;
            }

            if (hm_coroutine != null) StopCoroutine(hm_coroutine);
            hm_coroutine = StartCoroutine(ProcessHitmarker());
        }
    }

    private void OnActorRevived(Actor actor)
    {
        if (actor != playerActor) return;

        hpBar.fillAmount = playerActor.Health / playerActor.maxHealth;
        ToggleUIElement(player_ui_element.all, true);
    }

    private void OnWeaponShot(WeaponDATA data)
    {

    }
    private void OnWeaponChambered()
    {

    }

    private void OnWeaponShellInserted()
    {

    }

    private void OnWeaponReloaded()
    {

    }

    public void AssignPlayer(Actor playerActor)
    {
        this.playerActor = playerActor;
        playerInv = playerActor.GetComponent<ActorInventory>();
        playerMotor = playerActor.GetComponent<ActorMotor>();
        playerWeapon = playerActor.GetComponent<ActorWeapon>();
        playerEqp = playerActor.GetComponent<ActorEquipment>();
        playerEvents = playerActor.GetComponent<ActorEvents>();

        hpBar.fillAmount = playerActor.Health / playerActor.maxHealth;

        playerEqp.OnSlotDraw += OnSlotDraw;
        playerEqp.OnSlotHolster += OnSlotHolster;

        playerEvents.onWeaponShot += OnWeaponShot;
        playerEvents.onWeaponReloadEnd += OnWeaponReloaded;
        playerEvents.onWeaponShellInsert += OnWeaponShellInserted;
        playerEvents.onWeaponChambered += OnWeaponChambered;

        ToggleUIElement(player_ui_element.all, true);
    }

    private void OnSlotHolster(WeaponSlot slot)
    {
        Debug.Log($"OnSlotHolster({slot.data.Name});");
        this.slot = slot;
        ToggleWeaponUI(false);
    }

    private void OnSlotDraw(WeaponSlot slot)
    {
        Debug.Log($"OnSlotDraw({slot.data.Name});");
        this.slot = slot;
        ToggleWeaponUI(true);
    }

    public void ClearPlayer()
    {
        playerEvents.onWeaponShot -= OnWeaponShot;
        playerEvents.onWeaponReloadEnd -= OnWeaponReloaded;
        playerEvents.onWeaponShellInsert -= OnWeaponShellInserted;
        playerEvents.onWeaponChambered -= OnWeaponChambered;

        playerEqp.OnSlotDraw -= OnSlotDraw;
        playerEqp.OnSlotHolster -= OnSlotHolster;

        playerActor = null;
        playerMotor = null;
        playerWeapon = null;

        ToggleUIElement(player_ui_element.all, false);
    }

    public void ToggleUIElement(player_ui_element element, bool state)
    {
        switch (element)
        {
            case player_ui_element.all:
                foreach (var item in ui_elements)
                {
                    item.SetActive(state);
                }
                break;
            case player_ui_element.damage:
                ui_damageIndicators.SetActive(state);
                break;
            case player_ui_element.health:
                ui_health.SetActive(state);
                break;
            case player_ui_element.weapon:
                ui_weapon.SetActive(state);
                break;
            case player_ui_element.crosshair:
                ui_crosshair.SetActive(state);
                break;
            case player_ui_element.hitmarker:
                ui_hitmarker.SetActive(state);
                break;
            default:
                break;
        }
    }

    public void ToggleWeaponUI(bool toggle)
    {
        if (ui_weapon.activeSelf != toggle)
        {
            if (ui_weapon_update != null) StopCoroutine(ui_weapon_update);
            ui_weapon.SetActive(toggle);
            if (toggle) ui_weapon_update = StartCoroutine(WeaponPanelUpdater());
        }
    }

    Coroutine ui_weapon_update;
    IEnumerator WeaponPanelUpdater()
    {
        Debug.Log("WeaponPanelUpdater();");
        while (ui_weapon.activeSelf)
        {
            UpdateWeaponUI();
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void UpdateWeaponUI()
    {
        Debug.Log("UpdateWeaponUI();");

        if (!playerEqp.isArmed || slot.isEmpty)
        {
            ToggleWeaponUI(false);
            return;
        }

        ToggleWeaponUI(true);

        clipFill.fillAmount = (float)slot.entity.clip / slot.entity.data.clipSize;

        weaponIcon_first.sprite = slot.data.icon;

        if (slot.caliber == null)
        {
            clip.text = "";
        }
        else
        {
            if (playerEqp.GetCurrentAmmo() > 0)
            {
                clip.text = $"<size={clipFontSize}>{slot.clip}</size><size={ammoFontSize}> / {playerEqp.GetCurrentAmmo()}</size>";
            }
            else
            {
                clip.text = $"<size={clipFontSize}>{slot.clip}</size>";
            }
        }

        clip.color = (slot.chambered && slot.clip > 0) ? activeClip : emptyClip;
    }
    

    [Header("Game message")]
    public Image gameMsgFiller;
    public Text gMSG_Line;
    public Text gMSG_subLine;
    Coroutine cor_gameMsg;

    public void ShowBigCenterMSG(string line, Color lineColor, string subline, float time, float transition)
    {
        if (cor_gameMsg != null) StopCoroutine(cor_gameMsg);
        cor_gameMsg = StartCoroutine(_GameMSGShow(line, lineColor, subline, time, transition));
    }

    IEnumerator _GameMSGShow(string line, Color lineColor, string subline, float time, float transition)
    {
        gMSG_Line.text = line;
        gMSG_Line.color = lineColor;
        gMSG_subLine.text = subline;

        gameMsgFiller.fillOrigin = 0;
        while (gameMsgFiller.fillAmount < 1f)
        {
            gameMsgFiller.fillAmount = Mathf.MoveTowards(gameMsgFiller.fillAmount, 1f, Time.unscaledDeltaTime / transition);
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSecondsRealtime(time);

        gameMsgFiller.fillOrigin = 1;
        while (gameMsgFiller.fillAmount > 0f)
        {
            gameMsgFiller.fillAmount = Mathf.MoveTowards(gameMsgFiller.fillAmount, 0f, Time.unscaledDeltaTime / transition);
            yield return new WaitForEndOfFrame();
        }
    }
}

public enum player_ui_element
{
    all,
    damage,
    health,
    weapon,
    crosshair,
    hitmarker
}

[System.Serializable]
public class DamageIndicator
{
    public RectTransform indTrans;
    public Image sprite;

    public bool isActive;
    public Vector3 from;
    public float time;

    public DamageIndicator(GameObject gm)
    {
        indTrans = gm.GetComponent<RectTransform>();
        sprite = gm.GetComponent<Image>();

        isActive = false;
        time = 1;

        sprite.enabled = false;
    }
}