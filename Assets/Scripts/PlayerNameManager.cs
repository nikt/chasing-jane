using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerNameManager : MonoBehaviour
{
    public static PlayerNameManager Instance;

    [SerializeField] TMP_InputField usernameInput;
    [SerializeField] TMP_Dropdown roleInput;
    Role role;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // load saved username if present
        if (PlayerPrefs.HasKey("role"))
        {
            roleInput.value = PlayerPrefs.GetInt("role");
            PhotonNetwork.NickName = roleInput.captionText.text;
        }
        else
        {
            // placeholder nickname
            // usernameInput.text = "Player " + Random.Range(0, 10000).ToString("0000");
            roleInput.value = 0;
            OnUsernameInputValueChanged();
        }
    }

    public void OnUsernameInputValueChanged()
    {
        PhotonNetwork.NickName = roleInput.captionText.text;
        PlayerPrefs.SetInt("role", roleInput.value);

        role = (Role) roleInput.value;
    }

    public Role GetRole()
    {
        return role;
    }
}
