using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Boomerang : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float maxDuration;

    [HideInInspector] public Collider thrower;
    [HideInInspector] public float damage = 0f;

    bool hasReturned = false;
    float duration = 0f;
    Vector3 direction;

    PhotonView PV;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    public void Throw(Collider _thrower, Vector3 _direction, float _damage)
    {
        thrower = _thrower;
        direction = _direction;
        damage = _damage;
        duration = 0f;
    }

    public void SetFinishedTravelling(bool t)
    {
        if (t)
        {
            // done travelling, max out duration
            duration = maxDuration + 1f;
        }
    }

    void Update()
    {
        if (!PV.IsMine || !thrower)
        {
            // don't control someone elses boomerang
            return;
        }

        float delta = Time.deltaTime;
        duration += delta;

        if (duration < maxDuration)
        {
            // move toward destination
            transform.position = transform.position + (direction * speed * delta);
        }
        else
        {
            // move toward thrower
            transform.position = Vector3.MoveTowards(transform.position, thrower.transform.position, speed * delta);

            // clean up if we make it back to the thrower
            float dist = Vector3.Distance(transform.position, thrower.transform.position);
            if (dist < 0.5f)
            {
                hasReturned = true;
            }
        }

        // rotate object
        transform.Rotate(Vector3.right * 360 * delta, Space.Self);
    }

    public bool HasReturned()
    {
        return hasReturned;
    }
}
