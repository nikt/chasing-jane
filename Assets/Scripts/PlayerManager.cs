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

    float timeHeld;
    int timeRounded;

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

            // Jane starts holding patches
            if (role == (int)Role.Jane)
            {
                controller.GetComponent<PlayerController>()?.EquipPatches();
            }
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

    public void AddTime(float delta)
    {
        timeHeld += delta;
        int round = (int)timeHeld;

        // only report rounded numbers so we don't flood with requests
        if (round > timeRounded) {
            timeRounded = round;

            Hashtable hash = new Hashtable();
            hash.Add("timeHeld", timeRounded);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }
}
