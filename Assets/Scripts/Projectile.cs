using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Projectile : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float maxDuration;

    Collider shooter;
    float damage = 0f;

    float duration = 0f;
    Vector3 direction;

    bool hasFinished = false;

    PhotonView PV;
    Rigidbody rb;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
    }

    public void Shoot(Collider _shooter, Vector3 _direction, float _damage)
    {
        shooter = _shooter;
        direction = _direction;
        damage = _damage;
        duration = 0f;

        rb.velocity = direction * speed;
        hasFinished = false;

        Physics.IgnoreCollision(GetComponent<Collider>(), shooter);
    }

    public void SetFinishedTravelling(bool t)
    {
        if (t)
        {
            // done travelling, max out duration
            duration = maxDuration + 1f;
            hasFinished = true;
        }
    }

    void Update()
    {
        if (!PV.IsMine || !shooter)
        {
            // don't control someone elses projectile
            return;
        }

        float delta = Time.deltaTime;
        duration += delta;

        if (duration >= maxDuration || transform.position.y < -10f)
        {
            // lifetime expired
            hasFinished = true;
        }

        if (hasFinished)
        {
            // clean ourselves up
            PhotonNetwork.Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        if (!PV.IsMine || !shooter)
        {
            // don't control someone elses projectile
            return;
        }

        // rotate to mtach movement
        transform.rotation = Quaternion.LookRotation(rb.velocity);
    }

    public bool HasFinished()
    {
        return hasFinished;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!PV.IsMine)
        {
            // not our projectile, don't bother
            return;
        }

        if (collision.gameObject != shooter.gameObject)
        {
            // Debug.Log("inner hit: " + collision.gameObject.name);
            collision.gameObject.GetComponent<IDamageable>()?.TakeDamage(damage * 0.5f);

            // time to clean up
            SetFinishedTravelling(true);
        }
    }
}
