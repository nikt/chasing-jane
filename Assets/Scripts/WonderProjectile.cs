using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WonderProjectile : FireboltProjectile
{
    protected override float GetDamage()
    {
        // random damage, bias towards less (but still a small chance to insta-kill)
        float r = Random.value;
        return base.GetDamage() * r * r;
    }
}
