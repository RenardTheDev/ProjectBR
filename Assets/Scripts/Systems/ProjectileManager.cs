using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Exceptions;
using UnityEditor;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager current;
    public GameObject trail_prefab;

    public LayerMask hitMask;

    public List<Projectile> ActiveProj = new List<Projectile>();
    public List<Projectile> InactiveProj = new List<Projectile>();

    public bool debugTracers;
    public float debugTracersTime = 1f;

    private void Awake()
    {
        current = this;
        CreateProjectile();

        if (Application.platform != RuntimePlatform.WindowsEditor) debugTracers = false;
    }

    Actor hitActor;
    Rigidbody hitRig;
    bool isHit;

    Projectile p;
    RaycastHit hit;
    private void Update()
    {
        if (!(Time.timeScale > 0)) return;

        for (int i = 0; i < ActiveProj.Count; i++)
        {
            p = ActiveProj[i];

            if (p.isActive)
            {
                List<RaycastHit> HIT = new List<RaycastHit>(Physics.RaycastAll(p.pos, p.dir, p.speed * Time.deltaTime * 1.1f, hitMask));
                isHit = false;

                // sort by distance
                HIT.Sort((x, y) => x.distance.CompareTo(y.distance));

                for (int h = 0; h < HIT.Count; h++)
                {
                    hit = HIT[h];
                    if (debugTracers) Debug.DrawRay(hit.point, hit.normal, Color.cyan, debugTracersTime);

                    hitActor = hit.collider.GetComponentInParent<Actor>();
                    hitRig = hit.collider.GetComponentInParent<Rigidbody>();

                    if (hitActor != null)
                    {
                        if (hitActor != p.shooter)
                        {
                            OnProjectileHitActor(p, hit.point, hit.normal, hitActor, hit.transform);

                            isHit = true;

                            if (!hitActor.isAlive)
                            {
                                hitRig.AddForceAtPosition(p.dir * p.weapon.impact * 0.1f, p.pos, ForceMode.Impulse);
                            }
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (hitRig != null)
                        {
                            hitRig.AddExplosionForce(p.weapon.impact * 0.1f, hit.point - p.dir * 0.25f, 1f, 1f);
                        }
                        OnProjectileHit(p, hit.point, hit.normal);
                        isHit = true;
                        break;
                    }
                }

                if (isHit)
                {
                    continue;
                }
                else
                {
                    OnProjectileMiss(p);
                }
            }
            else
            {
                ActiveProj.Remove(p);
                InactiveProj.Add(p);
            }
        }
    }

    void OnProjectileHitActor(Projectile p, Vector3 point, Vector3 normal, Actor victim, Transform bone)
    {
        if (debugTracers)
        {
            Vector3 travel = point - p.pos;
            Debug.DrawRay(p.pos, travel, Color.red, debugTracersTime);
        }

        Damage dmg = new Damage(0, point, p.dir, p.weapon, p.shooter, bone);

        if (p.shooter != null && victim != null)
        {
            if ((FriendlyFire && victim.team == p.shooter.team) || victim.team != p.shooter.team)
            {
                dmg.amount = Mathf.Clamp(p.weapon.damage * 1.0f, 0, 100);
                ParticlesManager.inst.ArmorHit(point, normal);
            }
        }

        victim.ApplyDamage(dmg);

        HideProjectile(p);
    }

    Vector3 travel;
    void OnProjectileHit(Projectile p, Vector3 point, Vector3 normal)
    {
        if (debugTracers)
        {
            travel = point - p.pos;
            Debug.DrawRay(p.pos, travel, Color.red, debugTracersTime);
        }

        ParticlesManager.inst.BulletImpact_concrete(point, normal);

        HideProjectile(p);
    }

    void OnProjectileMiss(Projectile p)
    {
        travel = (p.dir * p.speed * Time.deltaTime);
        if (debugTracers)
        {
            Debug.DrawRay(p.pos, travel, Color.green, debugTracersTime);
        }

        p.pos = p.pos + travel;

        p.travel += p.speed * Time.deltaTime;
        p.lifeTime += Time.deltaTime;
        p.frames++;

        if (!p.trail_go.activeSelf)
        {
            if (p.frames > 0)
            {
                p.trail_rend.SetPosition(0, p.pos - travel);
                p.trail_rend.SetPosition(1, p.pos);
                p.trail_go.SetActive(true);
            }
        }
        else
        {
            p.trail_rend.SetPosition(0, p.pos - travel);
            p.trail_rend.SetPosition(1, p.pos);
        }

        if (p.travel >= p.weapon.ammoType.maxTravel || p.lifeTime >= projLifeTime)
        {
            HideProjectile(p);
        }
    }

    void HideProjectile(Projectile p)
    {
        p.isActive = false;
        p.lifeTime = 0;
        p.travel = 0;
        p.frames = 0;
        p.shooter = null;

        p.trail_go.SetActive(false);

        ActiveProj.Remove(p);
        InactiveProj.Add(p);
    }

    public void SpawnProjectile(Vector3 pos, Vector3 dir, Actor shooter, WeaponDATA weapon)
    {
        int id = GetFreeProjectile();
        if (id == -1)
        {
            id = CreateProjectile();
        }

        var p = InactiveProj[id];

        InactiveProj.Remove(p);
        ActiveProj.Add(p);

        p.isActive = true;
        p.shooter = shooter;
        p.pos = pos;

        p.weapon = weapon;
        p.speed = weapon.muzzleSpeed * (weapon.pellets > 1 ? Random.Range(0.95f, 1.05f) : 1f);
        p.dir = dir;
    }

    int GetFreeProjectile()
    {
        return InactiveProj.Count > 0 ? 0 : -1;
    }

    public bool FriendlyFire = false;
    public float projLifeTime = 5f;
    public int preloadProjectiles = 250;
    public WeaponDATA defaultWeap;
    int CreateProjectile()
    {
        for (int i = 0; i < preloadProjectiles; i++)
        {
            var p = new Projectile();
            p.pID = lastProjID;

            p.isActive = false;
            p.lifeTime = 0;
            p.travel = 0;
            p.frames = 0;
            p.shooter = null;
            p.weapon = defaultWeap;

            var go = Instantiate(trail_prefab, transform);
            p.trail_go = go;
            p.trail_rend = p.trail_go.GetComponent<LineRenderer>();

            InactiveProj.Add(p);

            lastProjID++;
        }
        return GetFreeProjectile();
    }

    int lastProjID = 0;
}

[System.Serializable]
public class Projectile
{
    public int pID;

    [HideInInspector]public Vector3 pos;
    [HideInInspector] public Vector3 dir;

    [HideInInspector] public Actor shooter;

    public float lifeTime;
    public float travel;
    public bool isActive;

    public int frames;

    public float speed;
    public WeaponDATA weapon;

    //---Trail---
    public GameObject trail_go;
    public LineRenderer trail_rend;
}


#if UNITY_EDITOR
[CustomEditor(typeof(ProjectileManager))]
public class ProjectileManagerEditor : Editor
{
    ProjectileManager script;
    public override void OnInspectorGUI()
    {
        if (script == null) script = (ProjectileManager)target;

        GUILayout.Label("Projectiles:\nActive: " + script.ActiveProj.Count + "\nInactive: " + script.InactiveProj.Count);

        EditorGUILayout.Space();

        base.OnInspectorGUI();
    }
}
#endif