using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public abstract class Item : MonoBehaviourPunCallbacks
{
    public ItemInfo itemInfo;
    public GameObject itemGameObject;
    [SerializeField] protected Collider selfCollider;

    // left click
    public abstract void Use();
    // right click
    public abstract void AlternateUse();
}
