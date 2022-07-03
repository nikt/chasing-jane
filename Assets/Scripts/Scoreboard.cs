using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Scoreboard : MonoBehaviourPunCallbacks
{
    [SerializeField] Transform container;
    [SerializeField] GameObject scoreboardItemPrefab;

    Dictionary<Player, ScoreboardItem> scoreboardItems = new Dictionary<Player, ScoreboardItem>();

    void Start()
    {
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            AddScoreboardItem(player);
        }
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
                item.deathsText.text = "" + (int)changedProps["kills"];
            }

            if (changedProps.ContainsKey("time"))
            {
                item.deathsText.text = "" + (int)changedProps["time"];
            }
        }
    }
}