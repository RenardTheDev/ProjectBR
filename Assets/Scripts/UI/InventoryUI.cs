using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI current;

    public ItemObject[] itemBase;
    public int y_space_between_item = 2;

    public GameObject itemPrefab;

    public LayerMask pickUpMask;

    [Header("Equipment")]
    public ActorWeapon aWeap;

    [Header("Player container")]
    public InventoryObject inv;
    public RectTransform playerListParent;
    public List<InventorySlotUI> plSlots = new List<InventorySlotUI>();

    [Header("Vicinity container")]
    public InventoryObject vic; // temporary inventory for vicinity objects
    public RectTransform vicinityListParent;
    public List<InventorySlotUI> vicSlots = new List<InventorySlotUI>();
    public Dictionary<InventorySlot, GameObject> pickups = new Dictionary<InventorySlot, GameObject>();

    private void Awake()
    {
        current = this;
    }

    private void Start()
    {

    }

    private void Update()
    {
        GenerateVicinityInventory(Vector3.zero, 4f, pickUpMask);

        UpdatePlayerDisplay();
        UpdateVicinityDisplay();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.25f);
        Gizmos.DrawSphere(Vector3.zero, 4f);
    }

    public void OnItemClicked(InventorySlot slot)
    {
        if (vic.ContainsSlot(slot))
        {
            vic.RemoveSlot(slot);
            Destroy(pickups[slot].gameObject, 0f);
            pickups.Remove(slot);

            UpdateVicinityDisplay();

            inv.AddItem(slot.item, slot.amount);
            UpdatePlayerDisplay();
        }
    }

    public void UpdatePlayerDisplay()
    {
        if (plSlots == null) plSlots = new List<InventorySlotUI>();

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
                plSlots[i].rt.localPosition = new Vector3(8, -(48 + y_space_between_item) * i, 0);
                plSlots[i].AssignInfo(inv.container[i]);
            }
            else
            {
                if(plSlots[i].gameObject.activeSelf) plSlots[i].gameObject.SetActive(false);
            }
        }

        playerListParent.sizeDelta = new Vector2(playerListParent.sizeDelta.x, inv.container.Count * (48 + y_space_between_item) - y_space_between_item);
    }

    public void GenerateVicinityInventory(Vector3 pos, float radius, LayerMask mask)
    {
        vic = new InventoryObject();
        pickups = new Dictionary<InventorySlot, GameObject>();

        Collider[] colls = Physics.OverlapSphere(pos, radius, mask);

        for (int i = 0; i < colls.Length; i++)
        {
            var pickup = colls[i].GetComponent<Item>();
            if (pickup != null)
            {
                var slot = vic.AddStandaloneItem(pickup.item, pickup.amount);
                pickups.Add(slot, pickup.gameObject);
            }
        }

    }

    public void UpdateVicinityDisplay()
    {
        if (vicSlots == null) vicSlots = new List<InventorySlotUI>();

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
                vicSlots[i].rt.localPosition = new Vector3(8, -(48 + y_space_between_item) * i, 0);
                vicSlots[i].AssignInfo(vic.container[i]);
            }
            else
            {
                if (vicSlots[i].gameObject.activeSelf) vicSlots[i].gameObject.SetActive(false);
            }
        }

        playerListParent.sizeDelta = new Vector2(playerListParent.sizeDelta.x, vic.container.Count * (48 + y_space_between_item) - y_space_between_item);
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