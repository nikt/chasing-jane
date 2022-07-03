using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class Bow : RangedWeapon
{
    [SerializeField] Camera cam;

    PhotonView PV;

    [SerializeField] string projectileName;
    protected Projectile projectile;

    const float duration = 1.5f;
    protected float currentDuration = -1f;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (!PV.IsMine) {
            return;
        }

        if (currentDuration > 0f)
        {
            currentDuration -= Time.fixedDeltaTime;
        }

        // do something with projectiles?
    }

    public override void Use()
    {
        if (currentDuration <= 0f)
        {
            // only start shoot if not currently shooting
            Shoot();
        }
    }

    public override void AlternateUse()
    {
        // do nothing
    }

    void Shoot()
    {
        currentDuration = duration;

        // shoot projectile
        projectile = PhotonNetwork.Instantiate(
            Path.Combine("PhotonPrefabs", projectileName),
            cam.transform.position + cam.transform.forward * 0.5f,
            cam.transform.rotation,
            0
        ).GetComponent<Projectile>();

        projectile.Shoot(selfCollider, cam.transform.forward, ((WeaponInfo)itemInfo).damage);
    }


}
