using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firebolt : Projectile
{
    void FixedUpdate()
    {
        MyFixedUpdate();
    }

    // returns false when exiting early
    protected override bool MyFixedUpdate()
    {
        // don't call base here because it resets our rotation
        // if (base.MyFixedUpdate()) {

        if (!PV.IsMine || !shooter)
        {
            // don't control someone elses projectile
            return false;
        }

        // spin along path
        transform.Rotate(Vector3.forward * 360 * Time.fixedDeltaTime, Space.Self);

        return true;
    }
}
