using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class Axe : Sword
{
    GameObject projectile;

    public override void AlternateUse()
    {
        // throw axe
        projectile = PhotonNetwork.Instantiate(
            Path.Combine("PhotonPrefabs", "Boomerang"),
            cam.transform.position + cam.transform.forward * 0.5f,
            cam.transform.rotation,
            0
        );
        projectile.GetComponent<Boomerang>()?.Throw(selfCollider, cam.transform.forward);
    }
}
