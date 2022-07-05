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
    Role role;
    int kills, deaths;

    float timeHeld;
    int timeRounded;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    public void SetRole(int r)
    {
        role = (Role)r;

        if (PV.IsMine)  // owned by local player
        {
            CreateController(true);

            // Jane starts holding patches
            if (role == Role.Jane)
            {
                controller.GetComponent<PlayerController>()?.EquipPatches();
            }
        }
    }

    void CreateController(bool first = false)
    {
        Transform spawn = SpawnManager.Instance.GetSpawnPoint(role, first);
        string roleName = RoleManager.GetNameForRole(role);
        string controllerName = roleName + "Controller";

        Debug.Log("creating controller: " + controllerName);

        if (role == Role.Observer)
        {
            // special case for observer: now network instantiation needed
            GameObject prefab = Resources.Load<GameObject>(Path.Combine("PhotonPrefabs", controllerName)) as GameObject;
            Debug.Log(prefab);
            controller = (GameObject)Instantiate(prefab, spawn.position, spawn.rotation);
            return;
        }

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
        
        // set role
        PlayerController pc = controller.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.role = role;
        }
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
