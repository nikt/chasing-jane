using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RangedWeapon : Item
{
    public abstract override void Use();
    public abstract override void AlternateUse();

    public GameObject impactPrefab;
}
