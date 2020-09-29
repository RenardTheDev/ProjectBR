using UnityEditor;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "weapon", menuName = "Scriptables/Weapon")]
public class WeaponDATA : ScriptableObject
{
    public string Name = "The gun";
    public WeaponType type = WeaponType.Primary;

    public GameObject prefab;
    public ItemObject inv_object;
    public Sprite icon;

    [Header("Audio")]
    public AudioClip sfx_shot;
    public AudioClip sfx_chamber;
    public AudioClip sfx_insert;
    public AudioClip sfx_remove;

    public FiringMode firingMode = FiringMode.fullauto;
    public ReloadMode reloadMode = ReloadMode.clip;

    public int clipSize = 30;

    public float fireRate = 500f;
    public float muzzleSpeed = 100f;
    public float spreading = 0.2f;
    public float pelletSpreading = 0.2f;

    public float aimFOV = 55f;

    [Header("Caliber")]
    public AmmoDATA ammoType;
    public float damageMult = 1.0f;
    public float impactMult = 1.0f;
    public float caliberMass;
    public int pellets = 1; // > 1 for shotguns

    [Header("Aiming")]
    public bool hasScope;
    public Vector3 adsOffset;

    [Header("Recoil")]
    public Vector3 recoilPower;
    public Vector3 recoilOffsetPower;
    public float recoilAttack;
    public float recoilDecay;

    [Header("Movement")]
    public bool canCrouch;
    public float speedCap;

    [Header("Animating")]
    public bool useAim;
    public bool pitchShift;
    public HumanBodyBones bonePivot;
    public Vector3 offset;

    [Header("anim_sprinting")]
    public HumanBodyBones sprintPivot = HumanBodyBones.Chest;
    public Vector3 inHandOffset;
    public Vector3 inHandRotation;
    [Range(0, 1)] public float[] sprintHIK = { 0, 0 };
    public GripType grip;
    //public AnimationClip sprintAnimation;

    [Header("Holster")]
    public Vector3 hPosition;
    public Vector3 hRotation;

    [Header("Animations")]
    public AnimationClip animRel_full;
    public AnimationClip animRel_tactical;
    [Space]
    public AnimationClip animRel_Start;
    public AnimationClip animRel_Insert;
    public AnimationClip animRel_End;
    [Space]
    public AnimationClip animChamber;
    public AnimationClip animShot;

    //--- auto calculated ---
    [HideInInspector] public float impact;
    [HideInInspector] public float damage;
    [HideInInspector] public float dps;

    private void OnValidate()
    {
        damage = muzzleSpeed * caliberMass * 0.01f * damageMult;
        impact = muzzleSpeed * caliberMass * 0.005f * impactMult;
        dps = damage * (fireRate / 60);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WeaponDATA))]
public class WeaponDATAEditor : Editor
{
    WeaponDATA script;
    public override void OnInspectorGUI()
    {
        if (script == null) script = (WeaponDATA)target;
        GUILayout.Label("Some stats.");
        if (script.pellets > 1)
        {
            GUILayout.Label("Damage: [pellet]=" + script.damage + " [full]=" + script.damage * script.pellets);
        }
        else
        {
            GUILayout.Label("Damage: " + script.damage);
        }
        GUILayout.Label("DPS: " + script.dps);
        GUILayout.Label("Impact: " + script.impact);

        EditorGUILayout.Space();

        base.OnInspectorGUI();
    }
}
#endif

public enum FiringMode
{
    single,
    semi_auto,
    burst,
    fullauto
}

public enum ReloadMode
{
    clip,
    one
}

//[Flags]
public enum WeaponType //: int
{
    Primary = 0,
    Secondary = 1,
    Melee = 2,
    Throwable = 3
}

public enum GripType
{
    oneHand = 0,
    twoHand = 1
}