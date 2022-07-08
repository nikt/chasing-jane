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
    [SerializeField] CanvasGroup canvasGroup;
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
        if (finished)
        {
            // game over, always show score and quit
            canvasGroup.alpha = 1;
            quitButton.SetActive(true);

            // don't look for user input after game is over
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            canvasGroup.alpha = 1;
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            canvasGroup.alpha = 0;
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

    public override void OnRoomPropertiesUpdate (Hashtable changedProps)
    {
        // Check for a winner
        if (changedProps.ContainsKey(RoomProperties.WINNING_ACTOR))
        {
            int winningActor = (int)changedProps[RoomProperties.WINNING_ACTOR];
            // Debug.Log("WINNER! actor: " + winningActor);

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