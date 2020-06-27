using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class WeapEntityStandard : WeaponEntity
{
    private void FixedUpdate()
    {
        spread = Mathf.Lerp(spread, 0f, Time.fixedDeltaTime * 1f);

        if (h_actor != null && !holstered && h_motor.dirLock_fade < 0.5f)
        {
            if (inp_fire && h_actor.isAlive && data.type != WeaponType.Melee && _chamberCoroutine == null && _reloadCoroutine == null)
            {
                if (isReloaded && clip > 0)
                {
                    if (Time.time > lastShot + shotDelay)
                    {
                        if (isChambered)
                        {
                            MakeShot();
                        }
                        else
                        {
                            if (_chamberCoroutine == null)
                            {
                                _chamberCoroutine = StartCoroutine(_chamber(false));
                            }
                        }
                    }
                }
                else
                {
                    if (h_eqp.GetCurrentAmmo() > 0 && _reloadCoroutine == null && Time.time > lastShot + shotDelay)
                    {
                        _reloadCoroutine = StartCoroutine(_reload());
                    }
                }
            }

            if (inp_reload)
            {
                TryReload();
            }
        }
    }

    public override void TryReload()
    {
        if (clip < getClipSize() && h_eqp.GetCurrentAmmo() > 0 && _chamberCoroutine == null && _reloadCoroutine == null)
        {
            _reloadCoroutine = StartCoroutine(_reload());
        }
    }

    Vector3 gunVector;
    Vector3 shotPoint;
    Vector3 resDirection;
    RaycastHit vHit;

    void MakeShot()
    {
        lastShot = Time.time;

        h_weapon.PlayShotSFX(data.sfx_shot);

        //!--Make muzzleflash---

        anim.Play("shot", 0, 0);

        gunVector = bullet.forward;

        if (h_actor.isPlayer)
        {
            shotPoint = bullet.position;

            Ray ray = Camera.main.ViewportPointToRay(Vector2.one * 0.5f);
            if (Physics.Raycast(ray, out vHit, 100))
            {
                if (vHit.distance > 1.5f)
                {
                    gunVector = (vHit.point - vHit.normal * 0.1f - bullet.position).normalized;
                }
            }

            for (int i = 0; i < data.pellets; i++)
            {
                resDirection = gunVector * 10f + Random.onUnitSphere * (spread + data.pelletSpreading);
                ProjectileManager.current.SpawnProjectile(shotPoint,
                resDirection.normalized, h_actor, data
                );
            }
        }
        else
        {
            gunVector = (h_look.lookAt - bullet.position).normalized;

            for (int i = 0; i < data.pellets; i++)
            {
                resDirection = gunVector * 100f + Random.onUnitSphere * (spread + data.pelletSpreading + 10 * (1f - h_actor.Accuracy));
                ProjectileManager.current.SpawnProjectile(bullet.position,
                resDirection.normalized, h_actor, data
                );
            }
        }

        spread += data.spreading;

        clip--;

        if (data.firingMode == FiringMode.single)
        {
            isChambered = false;
            if (_chamberCoroutine == null && clip > 0) _chamberCoroutine = StartCoroutine(_chamber(true));
        }

        if (clip == 0)
        {
            isReloaded = false;
            isChambered = false;
        }

        h_events.WeaponShot(data);
    }

    IEnumerator _chamber(bool afterShot)
    {
        if (afterShot) yield return new WaitForSeconds(data.animShot.length);

        isReloading = true;
        anim_Chamber();

        yield return new WaitForSeconds(data.animChamber.length);

        isChambered = true;
        isReloading = false;
        holdDown = false;

        h_events.WeaponChambered();

        _chamberCoroutine = null;
    }

    IEnumerator _reload()
    {
        if (_chamberCoroutine != null) StopCoroutine(_chamberCoroutine); _chamberCoroutine = null;

        h_events.WeaponReloadStart();

        isReloading = true;
        isReloaded = false;
        holdDown = true;

        anim_Reload(isChambered);

        if (data.reloadMode == ReloadMode.clip)
        {
            if (isChambered)
            {
                yield return new WaitForSeconds(data.animRel_tactical.length * 0.8f);
                holdDown = false;
                yield return new WaitForSeconds(data.animRel_tactical.length * 0.2f);
            }
            else
            {
                yield return new WaitForSeconds(data.animRel_full.length * 0.8f);
                holdDown = false;
                yield return new WaitForSeconds(data.animRel_full.length * 0.2f);
                isChambered = true;
            }

            int diff = getClipSize() - clip;

            if (h_eqp.GetCurrentAmmo() > diff)
            {
                h_eqp.RemoveCurrentAmmo(diff);
                clip = getClipSize();
            }
            else
            {
                clip += h_eqp.GetCurrentAmmo();
                h_eqp.RemoveCurrentAmmo(clip);
            }


            isReloaded = true;
            isReloading = false;

            h_events.WeaponReloadEnd();

            if (!isChambered)
            {
                _chamberCoroutine = StartCoroutine(_chamber(false));
            }
        }
        else
        {
            yield return new WaitForSeconds(data.animRel_Start.length);

            while (clip < getClipSize())
            {
                if (inp_fire && clip > 0) break;

                if (h_eqp.GetCurrentAmmo() > 0)
                {
                    h_eqp.RemoveCurrentAmmo(1);
                    clip++;
                    h_events.WeaponShellInsert();
                }
                else
                {
                    break;
                }

                anim_InsertShell();
                yield return new WaitForSeconds(data.animRel_Insert.length);
            }

            anim_ReloadEnd();
            yield return new WaitForSeconds(data.animRel_End.length);

            isReloaded = true;
            isReloading = false;
            holdDown = false;

            h_events.WeaponReloadEnd();

            if (!isChambered)
            {
                _chamberCoroutine = StartCoroutine(_chamber(false));
            }
        }

        _reloadCoroutine = null;
    }
}
