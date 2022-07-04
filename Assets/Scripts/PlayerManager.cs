using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerManager : MonoBehaviour
{
    PhotonView PV;

    GameObject controller;
    int role;
    int kills, deaths;
    float holdTime;

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
        // Debug.Log("creating controller for: " + roleName);

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

        // track kills and deaths
        deaths++;
        Hashtable deathsHash = new Hashtable();
        deathsHash.Add("deaths", deaths);
        PhotonNetwork.LocalPlayer.SetCustomProperties(deathsHash);
    }

    public void ReportKill()
    {
        // track kills
        kills++;
        Hashtable killsHash = new Hashtable();
        killsHash.Add("kills", kills);
        PhotonNetwork.LocalPlayer.SetCustomProperties(killsHash);
    }
}
