using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesManager : MonoBehaviour
{
    public static ParticlesManager inst;

    private void Awake()
    {
        inst = this;
    }

    public ParticleSystem ps_muzzle;
    public void MuzzleFlash(Vector3 pos, Vector3 dir)
    {
        ps_muzzle.transform.position = pos;
        ps_muzzle.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        ps_muzzle.Emit(1);
    }

    public ParticleSystem ps_blood;
    public void BloodSplash(Vector3 pos, Vector3 dir)
    {
        ps_blood.transform.position = pos;
        ps_blood.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        ps_blood.Emit(Random.Range(4, 8));
    }

    public ParticleSystem ps_armorHit;
    public void ArmorHit(Vector3 pos, Vector3 dir)
    {
        ps_armorHit.transform.position = pos;
        ps_armorHit.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        ps_armorHit.Emit(1);
    }

    public ParticleSystem ps_concreteHit;
    public void BulletImpact_concrete(Vector3 pos, Vector3 dir)
    {
        ps_concreteHit.transform.position = pos;
        ps_concreteHit.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        ps_concreteHit.Emit(1);
    }

    public ParticleSystem ps_heal;
    public void Effect_heal(Vector3 pos, Vector3 velocity)
    {
        ps_heal.transform.position = pos;
        ps_heal.Emit(60);
    }
}
