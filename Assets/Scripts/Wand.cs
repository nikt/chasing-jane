using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Wand : RangedWeapon
{
    [SerializeField] Camera cam;

    PhotonView PV;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    public override void Use()
    {
        Shoot();
    }

    public override void AlternateUse()
    {
        // do nothing
    }

    void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = cam.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((WeaponInfo)itemInfo).damage);
            PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);
        }
    }

    [PunRPC]
    void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        // move impacts with player
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        if (colliders.Length != 0)
        {
            GameObject impactObject = Instantiate(
                impactPrefab,
                hitPosition + hitNormal * 0.001f,   // small offset to avoid z-fighting
                Quaternion.LookRotation(hitNormal, Vector3.up) * impactPrefab.transform.rotation
            );

            // decay after 10s
            Destroy(impactObject, 10f);

            // set collision as parent so impact will follow and clean up if parent is killed
            impactObject.transform.SetParent(colliders[0].transform);
        }

    }
}
