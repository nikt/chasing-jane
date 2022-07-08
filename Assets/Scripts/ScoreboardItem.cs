using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ScoreboardItem : MonoBehaviourPunCallbacks
{
    public TMP_Text usernameText;
    public TMP_Text killsText;
    public TMP_Text deathsText;
    public TMP_Text timeText;
    public GameObject winnerImage;

    public Player player;

    public void Initialize(Player p)
    {
        player = p;
        usernameText.text = player.NickName;

        // make sure late joining players get updated stats
        UpdateStats();
    }

    void UpdateStats()
    {
        if (player.CustomProperties.TryGetValue(PlayerProperties.DEATHS, out object deaths))
        {
            deathsText.text = deaths.ToString();
        }

        if (player.CustomProperties.TryGetValue(PlayerProperties.KILLS, out object kills))
        {
            killsText.text = kills.ToString();
        }

        if (player.CustomProperties.TryGetValue(PlayerProperties.TIME_HELD, out object timeHeld))
        {
            timeText.text = timeHeld.ToString();
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        // sync player stats
        if (player == targetPlayer)
        {
            if (changedProps.ContainsKey(PlayerProperties.DEATHS) || changedProps.ContainsKey(PlayerProperties.KILLS) || changedProps.ContainsKey(PlayerProperties.TIME_HELD))
            {
                UpdateStats();
            }
        }
    }
}
