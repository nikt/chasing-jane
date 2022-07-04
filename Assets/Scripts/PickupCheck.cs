using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupCheck : MonoBehaviour
{
    PatchesPickup patchesPickup;

    void Awake()
    {
        patchesPickup = GetComponentInParent<PatchesPickup>();
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerController pc = other.gameObject.GetComponent<PlayerController>();
        if (pc != null)
        {
            patchesPickup.Pickup(pc);
        }
    }
}
