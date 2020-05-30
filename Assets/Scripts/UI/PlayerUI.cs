using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI current;
    public GameObject[] ui_elements;

    //---Specific parts---
    public GameObject ui_radar;
    public GameObject ui_damageIndicators;
    public GameObject ui_health;
    public GameObject ui_weapon;
    public GameObject ui_crosshair;
    public GameObject ui_hitmarker;
    public GameObject ui_actionlog;
    public GameObject ui_stats;

    public GameObject ui_inventory;
    public GameObject ui_controls;
    public GameObject ui_messages;

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
    public AnimationCurve fillCurve;

    float hm_scrTime = 0.2f;
    Image[] hm_parts;
    float hm_fill;

    [Header("Weapon")]
    public GameObject weaponPanel;
    public Vector3 wPanelOffset;
    public Image weaponIcon_first;
    public Image weaponIcon_second;
    public Image clipFill;
    public Text clip;
    public Color activeClip = Color.white;
    public Color emptyClip = Color.grey;

    RectTransform wPanelTrans;
    RectTransform crosshair;
    WeaponEntity currWeapon;
    int clipFontSize = 32;
    int ammoFontSize = 24;

    [Header("Buttons")]
    float lastPickup;
    public RectTransform pickupHintBase;
    public RectTransform pickupNamePlate;
    public Text pickupName;
    Vector3 hintPoint;

    //[Header("Inventory")]
    bool isInvOpen;

    Actor playerActor;
    ActorInventory playerInv;
    ActorEvents playerEvents;
    ActorMotor playerMotor;
    ActorWeapon playerWeapon;

    private void Awake()
    {
        current = this;

        mainCamera = Camera.main;
        StartCoroutine(CheckForGunPickUps());

        wPanelTrans = weaponPanel.GetComponent<RectTransform>();

        dmgIndicator = new List<DamageIndicator>();
        for (int i = 0; i < 10; i++)
        {
            dmgIndicator.Add(new DamageIndicator(Instantiate(dmgIndPrefab, IndicatorsPivot)));
        }
        dmgIndPrefab.SetActive(false);

        hm_parts = hm_base.GetComponentsInChildren<Image>();
        crosshair = ui_crosshair.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateWeaponInfoPanel();

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
                    ToggleUIElement(ui_element.all, false);
                    break;
                }
            }
            return;
        }

        scrRatio = (float)Screen.width / Screen.height;

        //---HEALTH---

        //hpBar.fillAmount = playerActor.Health / playerActor.maxHealth;

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

        //---Hitmarker---

        if (hm_base.activeSelf)
        {
            hm_fill = Mathf.MoveTowards(hm_fill, 1f, 2f * (Time.deltaTime / hm_scrTime));
            for (int i = 0; i < hm_parts.Length; i++)
            {
                hm_parts[i].fillOrigin = hm_fill > 0 ? 1 : 0;
                hm_parts[i].fillAmount = fillCurve.Evaluate(1f - Mathf.Abs(hm_fill));
            }
        }

        //---WEAPON---
        if (crosshair.gameObject.activeSelf)
        {
            if (playerWeapon.isArmed)
            {
                crosshair.sizeDelta = new Vector2(playerWeapon.currWEntity.spread * 64 + 8, playerWeapon.currWEntity.spread * 64 + 8);
                
                if (playerMotor.aiming && playerWeapon.currWData.hasScope)
                {
                    crosshair.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            if (playerWeapon.isArmed && !(playerMotor.aiming && playerWeapon.currWData.hasScope)) crosshair.gameObject.SetActive(true);
        }

        if (playerActor != null && weaponToPickUp != null && Time.time > lastPickup + 1)
        {
            if (Controls.pickup.state == bindState.down) PlayerPickUpWeapon();
            pickupHintBase.gameObject.SetActive(true);
        }
        else
        {
            pickupHintBase.gameObject.SetActive(false);
        }

        //---INVENTORY---
        if (Controls.inventory.state == bindState.down)
        {
            if (playerInv != null)
            {

            }
        }
    }

    public LayerMask weaponMask;
    public float pickupRadius = 3f;
    List<WeaponEntity> foundPicks;
    WeaponEntity weaponToPickUp;
    IEnumerator CheckForGunPickUps()
    {
        ProcessPickups();

        yield return new WaitForSecondsRealtime(0.2f);
        StartCoroutine(CheckForGunPickUps());
    }

    void ProcessPickups()
    {
        if (playerActor != null && Time.time > lastPickup + 1)
        {
            if (foundPicks != null)
            {
                foundPicks.Clear();
            }
            else
            {
                foundPicks = new List<WeaponEntity>();
            }

            var picks = new List<Collider>(Physics.OverlapSphere(playerActor.transform.position, pickupRadius, weaponMask));

            foreach (var item in picks)
            {
                foundPicks.Add(item.GetComponentInParent<WeaponEntity>());
            }

            if (foundPicks.Count == 0)
            {
                pickupHintBase.gameObject.SetActive(false);
                weaponToPickUp = null;
                return;
            }

            foundPicks.RemoveAll(x => !x.isDropped);

            if(foundPicks.Count == 0)
            {
                pickupHintBase.gameObject.SetActive(false);
                weaponToPickUp = null;
                return;
            }

            for (int pw = 0; pw < playerWeapon.weapon.Length; pw++)
            {
                foundPicks.RemoveAll(x => x.data == playerWeapon.GetEntity(pw));
            }
            //foundPicks.RemoveAll(x => x.clip == 0);

            if (foundPicks.Count > 0)
            {
                pickupHintBase.gameObject.SetActive(true);

                if (foundPicks.Count > 1)
                    foundPicks.Sort((x, y) => Vector3.Distance(
                        x.labelPivot.position, playerActor.transform.position + Vector3.up).CompareTo(
                        Vector3.Distance(y.labelPivot.position, playerActor.transform.position + Vector3.up)));

                weaponToPickUp = foundPicks[0];
                pickupName.text = weaponToPickUp.data.Name;

                pickupNamePlate.sizeDelta = new Vector2(pickupName.preferredWidth + 20f, pickupNamePlate.sizeDelta.y);
            }
            else
            {
                pickupHintBase.gameObject.SetActive(false);
                weaponToPickUp = null;
            }
        }
    }

    void PlayerPickUpWeapon()
    {
        playerWeapon.PickupWeapon(weaponToPickUp);
        lastPickup = Time.time;
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
            if (damage.attacker.isPlayer)
            {
                GMSurvival.current.ActionLog_Damage(actor, damage, false);

                for (int i = 0; i < hm_parts.Length; i++)
                {
                    hm_parts[i].color = Color.white;
                    hm_fill = -1;
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
        yield return new WaitForSeconds(hm_scrTime);
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
            ToggleUIElement(ui_element.all, false);
        }
        else
        {
            GMSurvival.current.ActionLog_Damage(actor, damage, true);

            for (int i = 0; i < hm_parts.Length; i++)
            {
                hm_parts[i].color = Color.red;
                hm_fill = -1;
            }

            if (hm_coroutine != null) StopCoroutine(hm_coroutine);
            hm_coroutine = StartCoroutine(ProcessHitmarker());
        }
    }

    private void OnActorRevived(Actor actor)
    {
        if (actor != playerActor) return;

        hpBar.fillAmount = playerActor.Health / playerActor.maxHealth;
        ToggleUIElement(ui_element.all, true);
    }

    private void OnSlotChanged(int oldSlot, int newSlot, WeaponSlot[] slotInfo)
    {
        weaponPanel.SetActive(slotInfo.Length > 0 && slotInfo[newSlot].slotType != WeaponType.Melee);

        if (slotInfo.Length == 0) return;

        if (!playerWeapon.IsEntityEmpty(oldSlot))
            ShowSecondWeapIcon(playerWeapon.GetWData(oldSlot).icon);
        else
            HideSecondWeapIcon();

        UpdateWeaponInfoPanel();

        if (playerWeapon.currWData.type == WeaponType.Melee)
        {
            ToggleUIElement(ui_element.weapon, false);
        }
    }

    private void OnWeaponShot(WeaponDATA data)
    {
        UpdateWeaponInfoPanel();
    }
    private void OnWeaponChambered()
    {
        UpdateWeaponInfoPanel();
    }

    private void OnWeaponShellInserted()
    {
        UpdateWeaponInfoPanel();
    }

    private void OnWeaponReloaded()
    {
        UpdateWeaponInfoPanel();
    }

    private void OnAmmoPickedUp(AmmoDATA caliber, int amount)
    {
        UpdateWeaponInfoPanel();
    }

    public void AssignPlayer(Actor playerActor)
    {
        this.playerActor = playerActor;
        playerInv = playerActor.GetComponent<ActorInventory>();
        playerMotor = playerActor.GetComponent<ActorMotor>();
        playerWeapon = playerActor.GetComponent<ActorWeapon>();
        playerEvents = playerActor.GetComponent<ActorEvents>();

        hpBar.fillAmount = playerActor.Health / playerActor.maxHealth;

        playerWeapon.OnSlotChanged += OnSlotChanged;
        playerWeapon.OnAmmoPickedup += OnAmmoPickedUp;

        playerEvents.onWeaponShot += OnWeaponShot;
        playerEvents.onWeaponReloadEnd += OnWeaponReloaded;
        playerEvents.onWeaponShellInsert += OnWeaponShellInserted;
        playerEvents.onWeaponChambered += OnWeaponChambered;

        ToggleUIElement(ui_element.all, true);
    }

    public void ClearPlayer()
    {
        playerWeapon.OnSlotChanged -= OnSlotChanged;
        playerWeapon.OnAmmoPickedup -= OnAmmoPickedUp;

        playerEvents.onWeaponShot -= OnWeaponShot;
        playerEvents.onWeaponReloadEnd -= OnWeaponReloaded;
        playerEvents.onWeaponShellInsert -= OnWeaponShellInserted;
        playerEvents.onWeaponChambered -= OnWeaponChambered;

        playerActor = null;
        playerMotor = null;
        playerWeapon = null;

        ToggleUIElement(ui_element.all, false);
    }

    void UpdateWeaponInfoPanel()
    {
        if (playerActor == null) return;

        currWeapon = playerWeapon.currWEntity;

        if (currWeapon != null)
        {
            clipFill.fillAmount = (float)currWeapon.clip / currWeapon.data.clipSize;

            weaponIcon_first.sprite = currWeapon.data.icon;

            if (playerWeapon.currCaliber == null)
            {
                clip.text = "";
            }
            else
            {
                if (playerWeapon.GetCurrentAmmo() > 0)
                {
                    clip.text = "<size=" + clipFontSize + ">" + currWeapon.clip + "</size><size=" + ammoFontSize + ">/" + playerWeapon.GetCurrentAmmo() + "</size>";
                }
                else
                {
                    clip.text = "<size=" + clipFontSize + ">" + currWeapon.clip + "</size>";
                }
            }

            clip.color = (currWeapon.chambered && currWeapon.clip > 0) ? activeClip : emptyClip;
        }
    }

    void ShowSecondWeapIcon(Sprite icon)
    {
        weaponIcon_second.transform.parent.gameObject.SetActive(true);
        weaponIcon_second.enabled = true;
        weaponIcon_second.sprite = icon;
    }

    void HideSecondWeapIcon()
    {
        weaponIcon_second.enabled = false;
        weaponIcon_second.transform.parent.gameObject.SetActive(false);
    }

    public void ToggleUIElement(ui_element element, bool state)
    {
        switch (element)
        {
            case ui_element.all:
                foreach (var item in ui_elements)
                {
                    item.SetActive(state);
                }
                break;
            case ui_element.radar:
                ui_radar.SetActive(state);
                break;
            case ui_element.damage:
                ui_damageIndicators.SetActive(state);
                break;
            case ui_element.health:
                ui_health.SetActive(state);
                break;
            case ui_element.weapon:
                ui_weapon.SetActive(state);
                break;
            case ui_element.crosshair:
                ui_crosshair.SetActive(state);
                break;
            case ui_element.hitmarker:
                ui_hitmarker.SetActive(state);
                break;
            case ui_element.actionlog:
                ui_actionlog.SetActive(state);
                break;
            case ui_element.stats:
                ui_stats.SetActive(state);
                break;
            case ui_element.inventory:
                ui_inventory.SetActive(state);
                break;
            case ui_element.controls:
                ui_controls.SetActive(state);
                break;
            case ui_element.messages:
                ui_messages.SetActive(state);
                break;
            default:
                break;
        }

        //--- conditional ---
        if (playerWeapon != null && playerWeapon.currWData != null)
        {
            if (playerWeapon.currWData.type == WeaponType.Melee)
            {
                ui_weapon.SetActive(false);
            }
        }
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

public enum ui_element
{
    all,
    radar,
    damage,
    health,
    weapon,
    crosshair,
    hitmarker,
    actionlog,
    stats,
    inventory,
    controls,
    messages
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