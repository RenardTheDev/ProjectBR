using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    Canvas canvas;
    public static InventoryUI current;

    public ItemObject[] itemBase;
    public int y_space_between_item = 2;
    public int item_height = 48;

    public GameObject itemPrefab;

    public LayerMask pickUpMask;

    [Header("Equipment")]
    public ActorEquipment eqp;
    public EquipmentSlotUI[] eqSlotUI;
    public int selectedEQSlot = -1;

    [Header("Player container")]
    public Actor player;
    public InventoryObject inv;
    public RectTransform playerListParent;
    public List<InventorySlotUI> plSlots = new List<InventorySlotUI>();

    [Header("Vicinity container")]
    public float vicRadius = 2f;
    public Vector3 vicOffset = new Vector3(0, 1.0f, 0);

    public InventoryObject vic; // temporary inventory for vicinity objects
    public RectTransform vicinityListParent;
    public List<InventorySlotUI> vicSlots = new List<InventorySlotUI>();
    public Dictionary<InventorySlot, Item> pickups = new Dictionary<InventorySlot, Item>();

    private void Awake()
    {
        current = this;
        canvas = GetComponent<Canvas>();
    }

    private void Start()
    {

    }

    private void Update()
    {
        if (player != null && canvas.enabled)
        {
            GenerateVicinityInventory(player.transform.position + vicOffset, vicRadius, pickUpMask);
            UpdateVicinityDisplay();
        }
    }

    private void OnDrawGizmos()
    {
        if (player != null && canvas.enabled)
        {
            Gizmos.color = new Color(0, 1, 0, 0.25f);
            Gizmos.DrawSphere(player.transform.position + vicOffset, vicRadius);
        }
    }

    public void AssignPlayer(Actor actor)
    {
        player = actor;
        inv = player.GetComponent<ActorInventory>().inventory;
        eqp = player.GetComponent<ActorEquipment>();

        for (int i = 0; i < eqSlotUI.Length; i++)
        {
            eqSlotUI[i].AssignActor(eqp);
        }

        inv.InventoryUpdate += OnInventoryUpdate;

        UpdatePlayerDisplay();
    }

    private void OnInventoryUpdate()
    {
        UpdatePlayerDisplay();
    }

    private void OnEnable()
    {
        if (inv != null) inv.InventoryUpdate += OnInventoryUpdate;
        if (player != null && canvas.enabled)
        {
            OnInventoryUpdate();
            GenerateVicinityInventory(player.transform.position + vicOffset, vicRadius, pickUpMask);
            UpdateVicinityDisplay();
        }
    }

    private void OnDisable()
    {
        if (inv != null) inv.InventoryUpdate -= OnInventoryUpdate;
    }

    public void OnItemClicked(InventorySlot slot)
    {
        if (slot.item is WeaponObject)
        {
            if (selectedEQSlot != -1)
            {
                if (eqp.slots[selectedEQSlot].isEmpty)
                {
                    if (vic.ContainsSlot(slot))
                    {
                        var wSlot = eqp.AssignWeaponToSlot(selectedEQSlot, ((WeaponObject)slot.item).weapon);
                        eqSlotUI[selectedEQSlot].UpdateSlotPlate();

                        eqSlotUI[selectedEQSlot].ChangeSelection(false);
                        selectedEQSlot = -1;

                        DeletePickup(slot);

                    }
                }
                else
                {
                    if (vic.ContainsSlot(slot))
                    {
                        PickupManager.current.SpawnWeaponPickup(
                            eqp.slots[selectedEQSlot].entity.transform.position,
                            eqp.slots[selectedEQSlot].entity.transform.rotation,
                            eqp.slots[selectedEQSlot].entity.data);

                        var wSlot = eqp.AssignWeaponToSlot(selectedEQSlot, ((WeaponObject)slot.item).weapon);
                        eqSlotUI[selectedEQSlot].UpdateSlotPlate();

                        eqSlotUI[selectedEQSlot].ChangeSelection(false);
                        selectedEQSlot = -1;

                        DeletePickup(slot);
                    }
                }
            }
            else
            {
                int toSlot = 0;
                WeaponDATA weap = ((WeaponObject)slot.item).weapon;

                switch (weap.type)
                {
                    case WeaponType.Primary:
                        {
                            for (int i = 1; i >= 0; i--)
                            {
                                if (eqp.slots[i].isEmpty) toSlot = i;
                            }
                        }
                        break;
                    case WeaponType.Secondary:
                        {
                            toSlot = 2;
                        }
                        break;
                    case WeaponType.Melee:
                        //--- for later implementation
                        break;
                    case WeaponType.Throwable:
                        //--- for later implementation
                        break;
                }

                if (eqp.slots[toSlot].isEmpty)
                {
                    if (vic.ContainsSlot(slot))
                    {
                        var wSlot = eqp.AssignWeaponToSlot(toSlot, weap);
                        eqSlotUI[selectedEQSlot].UpdateSlotPlate();

                        DeletePickup(slot);
                    }
                }
                else
                {
                    if (vic.ContainsSlot(slot))
                    {
                        PickupManager.current.SpawnWeaponPickup(
                            eqp.slots[toSlot].entity.transform.position,
                            eqp.slots[toSlot].entity.transform.rotation,
                            eqp.slots[toSlot].entity.data);

                        var wSlot = eqp.AssignWeaponToSlot(toSlot, ((WeaponObject)slot.item).weapon);
                        eqSlotUI[selectedEQSlot].UpdateSlotPlate();

                        DeletePickup(slot);
                    }

                }
            }
        }
        else
        {
            if (vic.ContainsSlot(slot))
            {
                TakeSlot(slot);
            }
        }
    }

    InventorySlot TakeSlot(InventorySlot slot)
    {
        DeletePickup(slot);

        UpdateVicinityDisplay();

        var newSlot = inv.AddItem(slot.item, slot.amount);
        UpdatePlayerDisplay();

        return newSlot;
    }

    void DeletePickup(InventorySlot slot)
    {
        vic.RemoveSlot(slot);

        PickupManager.current.HidePickup(pickups[slot].pickup);

        pickups.Remove(slot);
    }

    public void OnEquipmentSlotClicked(int slotID)
    {
        if (GameUI.current.invOpened)
        {
            if (selectedEQSlot == -1)
            {
                //--- Start equipment selection ---
                selectedEQSlot = slotID;
                eqSlotUI[selectedEQSlot].ChangeSelection(true);
            }
            else
            {
                if (slotID == selectedEQSlot)
                {
                    //--- Cancel equipment selection ---
                    eqSlotUI[selectedEQSlot].ChangeSelection(false);
                    selectedEQSlot = -1;
                }
            }
        }
        else if (GameUI.current.eqpOpened)
        {
            if (!eqp.slots[slotID].isEmpty)
            {
                eqp.ChangeSlot(slotID);

                GameUI.current.ToggleEquipment(false);
            }
        }
    }

    public void OnToggleInventory(bool toggle)
    {
        //Debug.Log($"OnToggleInventory({toggle})");

        if (toggle)
        {
            for (int i = 0; i < eqSlotUI.Length; i++)
            {
                eqSlotUI[i].UpdateSlotPlate();
            }
        }
        else
        {
            if (selectedEQSlot != -1)
            {
                eqSlotUI[selectedEQSlot].ChangeSelection(false);
                selectedEQSlot = -1;
            }
        }
    }

    public void UpdatePlayerDisplay()
    {
        if (plSlots == null && GameUI.current.invOpened) plSlots = new List<InventorySlotUI>();

        inv.container.Sort((x, y) => x.item.type.CompareTo(y.item.type));

        if (plSlots.Count < inv.container.Count)
        {
            for (int i = 0; i < inv.container.Count - plSlots.Count; i++)
            {
                var obj = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity, playerListParent);
                var slotUI = obj.GetComponent<InventorySlotUI>();
                plSlots.Add(slotUI);
            }
        }

        for (int i = 0; i < plSlots.Count; i++)
        {
            if (i < inv.container.Count)
            {
                if (!plSlots[i].gameObject.activeSelf) plSlots[i].gameObject.SetActive(true);
                plSlots[i].rt().localPosition = new Vector3(8, -(item_height + y_space_between_item) * i, 0);
                plSlots[i].AssignInfo(inv.container[i]);
            }
            else
            {
                if(plSlots[i].gameObject.activeSelf) plSlots[i].gameObject.SetActive(false);
            }
        }

        playerListParent.sizeDelta = new Vector2(playerListParent.sizeDelta.x, inv.container.Count * (item_height + y_space_between_item) - y_space_between_item);
    }

    public void GenerateVicinityInventory(Vector3 pos, float radius, LayerMask mask)
    {
        vic = new InventoryObject();
        pickups = new Dictionary<InventorySlot, Item>();

        Collider[] colls = Physics.OverlapSphere(pos, radius, mask);

        for (int i = 0; i < colls.Length; i++)
        {
            var pickup = colls[i].GetComponent<Item>();
            if (pickup != null)
            {
                var slot = vic.AddStandaloneItem(pickup.item, pickup.amount);
                pickups.Add(slot, pickup.GetComponent<Item>());
            }
        }
    }

    public void UpdateVicinityDisplay()
    {
        if (vicSlots == null && GameUI.current.invOpened) vicSlots = new List<InventorySlotUI>();

        vic.container.Sort((x, y) => x.item.type.CompareTo(y.item.type));

        if (vicSlots.Count < vic.container.Count)
        {
            for (int i = 0; i < vic.container.Count - vicSlots.Count; i++)
            {
                var obj = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity, vicinityListParent);
                var slotUI = obj.GetComponent<InventorySlotUI>();
                vicSlots.Add(slotUI);
            }
        }

        for (int i = 0; i < vicSlots.Count; i++)
        {
            if (i < vic.container.Count)
            {
                if (!vicSlots[i].gameObject.activeSelf) vicSlots[i].gameObject.SetActive(true);
                vicSlots[i].rt().localPosition = new Vector3(8, -(item_height + y_space_between_item) * i, 0);
                vicSlots[i].AssignInfo(vic.container[i]);
            }
            else
            {
                if (vicSlots[i].gameObject.activeSelf) vicSlots[i].gameObject.SetActive(false);
            }
        }

        playerListParent.sizeDelta = new Vector2(playerListParent.sizeDelta.x, vic.container.Count * (item_height + y_space_between_item) - y_space_between_item);
    }

    public bool isActive()
    {
        return canvas.enabled;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(InventoryUI))]
public class InventoryUIEditor : Editor
{
    InventoryUI script;
    public override void OnInspectorGUI()
    {
        if (script == null) script = (InventoryUI)target;
        base.OnInspectorGUI();

        EditorGUILayout.Space();

        if (GUILayout.Button("Add random item"))
        {
            script.inv.AddItem(script.itemBase[Random.Range(0, script.itemBase.Length)], Random.Range(1, 100));
        }

        if (GUILayout.Button("Check random item"))
        {
            script.inv.ContainsItem(script.itemBase[Random.Range(0, script.itemBase.Length)]);
        }

        if (GUILayout.Button("Remove random item"))
        {
            script.inv.RemoveItem(script.itemBase[Random.Range(0, script.itemBase.Length)], Random.Range(1, 100));
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Sort by NAME up"))
        {
            script.inv.container.Sort((x, y) => x.item.Name.CompareTo(y.item.Name));
        }

        if (GUILayout.Button("Sort by NAME down"))
        {
            script.inv.container.Sort((x, y) => y.item.Name.CompareTo(x.item.Name));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Sort by TYPE up"))
        {
            script.inv.container.Sort((x, y) => x.item.type.CompareTo(y.item.type));
        }

        if (GUILayout.Button("Sort by TYPE down"))
        {
            script.inv.container.Sort((x, y) => y.item.type.CompareTo(x.item.type));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Sort by AMOUNT up"))
        {
            script.inv.container.Sort((x, y) => x.amount.CompareTo(y.amount));
        }

        if (GUILayout.Button("Sort by AMOUNT down"))
        {
            script.inv.container.Sort((x, y) => y.amount.CompareTo(x.amount));
        }
        GUILayout.EndHorizontal();
    }
}
#endif