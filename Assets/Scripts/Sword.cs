using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Sword : MeleeWeapon
{
    [SerializeField] protected Camera cam;
    // [SerializeField] protected Collider selfCollider;
    [SerializeField] Transform weaponTransform;
    [SerializeField] Transform baseTransform;
    [SerializeField] Transform swingTransform;

    protected PhotonView PV;

    const float duration = 0.2f;
    protected float currentDuration = -1f;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    void FixedUpdate()
    {
        if (!PV.IsMine) {
            return;
        }

        if (currentDuration > 0f)
        {
            currentDuration -= Time.fixedDeltaTime;

            // animate swing
            float progress = 1f - (currentDuration / duration);
            weaponTransform.position = Vector3.Lerp(baseTransform.position, swingTransform.position, progress);
            weaponTransform.rotation = Quaternion.Slerp(baseTransform.rotation, swingTransform.rotation, progress);
        }

        bool complete = (currentDuration <= 0f);
        swingEffect.SetActive(!complete);

        if (complete)
        {
            // reset weapon transform
            weaponTransform.position = new Vector3(baseTransform.position.x, baseTransform.position.y, baseTransform.position.z);
            weaponTransform.rotation = new Quaternion(baseTransform.rotation.x, baseTransform.rotation.y, baseTransform.rotation.z, baseTransform.rotation.w);
        }
    }

    public override void Use()
    {
        if (currentDuration <= 0f)
        {
            // only start swing if not currently swinging
            Swing();
        }
    }

    public override void AlternateUse()
    {
        // do nothing
    }

    void Swing()
    {
        currentDuration = duration;
        Collider[] colliders = Physics.OverlapSphere(cam.transform.position + cam.transform.forward * ((WeaponInfo)itemInfo).range, ((WeaponInfo)itemInfo).range * 0.5f);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider c = colliders[i];
            if (c != selfCollider)
            {
                // Debug.Log("hit: " + c.gameObject.name);
                c.gameObject.GetComponent<IDamageable>()?.TakeDamage(((WeaponInfo)itemInfo).damage);
            }
        }
    }
}
