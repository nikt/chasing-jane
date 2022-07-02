using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class UsernameDisplay : MonoBehaviour
{
    [SerializeField] PhotonView playerPV;
    [SerializeField] TMP_Text text;

    void Start()
    {
        if (playerPV.IsMine)
        {
            // hide our own username
            gameObject.SetActive(false);
        }
        
        text.text = playerPV.Owner.NickName;
    }
}
