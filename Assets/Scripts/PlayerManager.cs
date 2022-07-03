using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class PlayerManager : MonoBehaviour
{
    PhotonView PV;

    GameObject controller;
    int role;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    public void SetRole(int r)
    {
        role = r;

        if (PV.IsMine)  // owned by local player
        {
            CreateController();
        }
    }

    void CreateController()
    {
        string roleName = RoleManager.GetNameForRole((Role)role);
        Debug.Log("creating controller for: " + roleName);

        string controllerName = roleName + "Controller";
        if (roleName == "Observer")
        {
            controllerName = "PlayerController";
        }

        Transform spawn = SpawnManager.Instance.GetSpawnPoint();
        // Instantiate our player controller
        controller = PhotonNetwork.Instantiate(
            Path.Combine("PhotonPrefabs", controllerName),
            spawn.position,
            spawn.rotation,
            0,
            new object[] {
                PV.ViewID
            }
        );
    }

    public void Die()
    {
        PhotonNetwork.Destroy(controller);

        // respawn (remake controller)
        CreateController();
    }
}
