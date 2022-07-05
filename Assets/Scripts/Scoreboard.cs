using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Scoreboard : MonoBehaviourPunCallbacks
{
    [SerializeField] Transform container;
    [SerializeField] GameObject scoreboardItemPrefab;
    [SerializeField] GameObject quitButton;
    bool finished = false;
    bool winner = false;

    Dictionary<Player, ScoreboardItem> scoreboardItems = new Dictionary<Player, ScoreboardItem>();

    void Start()
    {
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            AddScoreboardItem(player);
        }

        // disable quit button by default
        quitButton.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Tab))
        {
            container.gameObject.SetActive(true);
        }
        else
        {
            container.gameObject.SetActive(false);
        }

        if (finished)
        {
            // game over, always show score and quit
            container.gameObject.SetActive(true);
            quitButton.SetActive(true);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AddScoreboardItem(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RemoveScoreboardItem(otherPlayer);
    }

    void AddScoreboardItem(Player player)
    {
        ScoreboardItem item = Instantiate(scoreboardItemPrefab, container).GetComponent<ScoreboardItem>();
        item.Initialize(player);

        // save reference to item (for later removal)
        scoreboardItems[player] = item;
    }

    void RemoveScoreboardItem(Player player)
    {
        Destroy(scoreboardItems[player].gameObject);
        scoreboardItems.Remove(player);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        // sync player stats
        if (scoreboardItems.ContainsKey(targetPlayer))
        {
            ScoreboardItem item = scoreboardItems[targetPlayer];

            if (changedProps.ContainsKey("deaths"))
            {
                item.deathsText.text = "" + (int)changedProps["deaths"];
            }

            if (changedProps.ContainsKey("kills"))
            {
                item.killsText.text = "" + (int)changedProps["kills"];
            }

            if (changedProps.ContainsKey("timeHeld"))
            {
                item.timeText.text = "" + (int)changedProps["timeHeld"];
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
            int winningActor = (int)changedProps[RoomProperties.WINNING_ACTOR];
            Debug.Log("WINNER! actor: " + winningActor);

            finished = true;
            winner = (PhotonNetwork.LocalPlayer.ActorNumber == winningActor);

            // find winner item
            foreach (Player player in scoreboardItems.Keys)
            {
                if (player.ActorNumber == winningActor)
                {
                    scoreboardItems[player].winnerImage.SetActive(true);
                    break;
                }
            }
        }
    }

    public void LeaveRoom()
    {
        StartCoroutine(DisconnectAndLoad());
    }

    IEnumerator DisconnectAndLoad()
    {
        // leave and wait for full disconnect
        PhotonNetwork.LeaveRoom();
        while (PhotonNetwork.InRoom)
        {
            yield return null;
        }

        // clean up RoomManager before hitting main menu (so there's no duplicate error)
        Destroy(RoomManager.Instance.gameObject);

        // go back to main menu
        SceneManager.LoadScene("Intro");
    }
}