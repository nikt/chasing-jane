using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Projectile : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float maxDuration;

    protected Collider shooter;
    float damage = 0f;

    float duration = 0f;
    Vector3 direction;

    bool hasFinished = false;

    protected PhotonView PV;
    protected Rigidbody rb;

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
        MyFixedUpdate();
    }

    // returns false when exiting early
    protected virtual bool MyFixedUpdate()
    {
        if (!PV.IsMine || !shooter)
        {
            // don't control someone elses projectile
            return false;
        }

        // rotate to mtach movement
        transform.rotation = Quaternion.LookRotation(rb.velocity);

        return true;
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
            collision.gameObject.GetComponent<IDamageable>()?.TakeDamage(GetDamage());

            // time to clean up
            SetFinishedTravelling(true);
        }
    }

    protected virtual float GetDamage()
    {
        return damage;
    }
}
