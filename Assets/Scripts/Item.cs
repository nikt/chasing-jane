using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public ItemInfo itemInfo;
    public GameObject itemGameObject;

    // left click
    public abstract void Use();
    // right click
    public abstract void AlternateUse();
}
