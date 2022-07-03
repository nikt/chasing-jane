using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WonderProjectile : FireboltProjectile
{
    protected override float GetDamage()
    {
        // random damage, bias towards less
        float r = Random.value;
        Debug.Log("damage: " + (base.GetDamage() * r * r));
        return base.GetDamage() * r * r;
    }
}
