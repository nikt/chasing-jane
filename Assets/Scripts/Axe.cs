using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Axe : Sword
{
    Boomerang projectile;

    void Update()
    {
        if (!PV.IsMine) {
            return;
        }

        if (projectile && projectile.HasReturned())
        {
            // Debug.Log("axe returned");
            PhotonNetwork.Destroy(projectile.gameObject);
            projectile = null;

            // show axe in hand
            ShowWeapon(true);
        }
    }

    public override void Use()
    {
        if (!projectile)
        {
            base.Use();
        }
    }

    public override void AlternateUse()
    {
        if (!projectile && currentDuration <= 0f)
        {
            // throw axe
            projectile = PhotonNetwork.Instantiate(
                Path.Combine("PhotonPrefabs", "Boomerang"),
                cam.transform.position + cam.transform.forward * 0.5f,
                cam.transform.rotation,
                0
            ).GetComponent<Boomerang>();

            // note: ranged attack deals half damage
            projectile.Throw(selfCollider, cam.transform.forward, ((WeaponInfo)itemInfo).damage * 0.5f);

            // hide axe in hand
            ShowWeapon(false);
        }
    }

    void ShowWeapon(bool show)
    {
        itemGameObject.SetActive(show);

        // weapon sync
        if (PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("itemShown", show);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        // sync other players' weapons
        if (!PV.IsMine && targetPlayer == PV.Owner && changedProps.ContainsKey("itemShown"))
        {
            itemGameObject.SetActive((bool)changedProps["itemShown"]);
        }
    }
}
