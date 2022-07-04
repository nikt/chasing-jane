using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PatchesPickup : MonoBehaviourPunCallbacks
{
    public static PatchesPickup Instance;

    [SerializeField] GameObject pickupCheck;
    [SerializeField] Transform modelTransform;

    PhotonView PV;

    void Awake()
    {
        if (Instance)
        {
            Destroy(Instance);
        }

        // highlander
        Instance = this;

        PV = GetComponent<PhotonView>();

        // start game inactive (Jane will start holding)
        pickupCheck.SetActive(false);
    }

    void FixedUpdate()
    {
        if (!pickupCheck.activeSelf)
        {
            return;
        }

        // bounce up and down
        modelTransform.localPosition = new Vector3(0f, Mathf.Sin(Time.time) * 0.5f, 0f);

        // rotate object
        modelTransform.Rotate(Vector3.up * 90 * Time.fixedDeltaTime, Space.Self);
    }

    public void Pickup(PlayerController pc)
    {
        pc.EquipPatches();

        // disable ourselves
        pickupCheck.gameObject.SetActive(false);

        // disable for everyone
        Hashtable hash = new Hashtable();
        hash.Add("patchesActive", false);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
    }

    public void Drop(float x, float y, float z)
    {
        Vector3 target = new Vector3(x, y, z);
        transform.position = target;

        // move for everyone
        Hashtable hash = new Hashtable();
        hash.Add("patchesPosition", target);
        hash.Add("patchesActive", true);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
    }

    public override void OnRoomPropertiesUpdate (Hashtable changedProps)
    {
        // Debug.Log("rooms props changed");
        // Debug.Log(changedProps);
        if (changedProps.ContainsKey("patchesActive"))
        {
            pickupCheck.gameObject.SetActive((bool)changedProps["patchesActive"]);
        }

        if (changedProps.ContainsKey("patchesPosition"))
        {
            pickupCheck.transform.position = (Vector3)changedProps["patchesPosition"];
        }
    }
}
