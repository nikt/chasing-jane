using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomerangHitCheck : MonoBehaviour
{
    Boomerang boomerang;

    void Awake()
    {
        boomerang = GetComponentInParent<Boomerang>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other != boomerang.thrower)
        {
            // Debug.Log("hit: " + other.gameObject.name);
            boomerang.SetFinishedTravelling(true);
        }
    }
}
