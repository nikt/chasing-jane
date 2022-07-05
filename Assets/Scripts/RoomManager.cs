using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.IO;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;

    void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public override void OnEnable() {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable() {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        // make sure we're in the Game/City scene
        if (scene.buildIndex == 1 || scene.buildIndex == 2)
        {
            GameObject managerObject = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity);

            // load proper character
            if (PlayerPrefs.HasKey("role"))
            {
                int role = PlayerPrefs.GetInt("role");
                managerObject.GetComponent<PlayerManager>()?.SetRole(role);
            }
        }
    }

    public override void OnRoomPropertiesUpdate (Hashtable changedProps)
    {
        // Debug.Log("rooms props changed");
        // Debug.Log(changedProps);

        // Check for a winner
        if (changedProps.ContainsKey(RoomProperties.WINNING_ACTOR))
        {
            // Debug.Log("WINNER! actor: " + (int)changedProps[RoomProperties.WINNING_ACTOR]);
        }
    }
}
