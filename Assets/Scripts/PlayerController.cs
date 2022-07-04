using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
    [SerializeField] Image healthbarImage;
    [SerializeField] GameObject ui;
    [SerializeField] GameObject cameraHolder;

    [SerializeField] float mouseSensitivity, sprintSpeed, walkSpeed, jumpForce, smoothTime;

    [SerializeField] Item[] items;

    int itemIndex;
    int previousItemIndex = -1;

    // Patches
    bool hasPatches = false;

    float verticalLookRotation;
    bool grounded;
    Vector3 smoothMoveVelocity;
    Vector3 moveAmount;

    Rigidbody rb;

    PhotonView PV;

    Vignette vignette;
    const float maxHealth = 150f;
    float currentHealth = maxHealth;

    PlayerManager playerManager;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();

        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();

        // find vignette in scene
        PostProcessVolume volume = GameObject.Find("PostProcessing").GetComponent<PostProcessVolume>();
        PostProcessProfile profile = volume.profile;
        vignette = profile.GetSetting<Vignette>();
    }

    void Start()
    {
        if (PV.IsMine)
        {
            EquipItem(0);
        }
        else
        {
            // destroy other cameras
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(ui);

            // Destroy(rb);
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        if (!PV.IsMine)
        {
            // only control ourselves
            return;
        }

        Look();
        Move();
        Jump();
        Equip();

        if (Input.GetMouseButtonDown(0))
        {
            items[itemIndex].Use();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            items[itemIndex].AlternateUse();
        }

        if (transform.position.y < -10f)
        {
            // die if you fall out of bounds
            Die(true);
        }

        if (hasPatches)
        {
            // save time held
            playerManager.AddTime(Time.deltaTime);
        }
    }

    void Look()
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);

        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }

    void Move()
    {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (IsSprinting() ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.AddForce(transform.up * jumpForce);
        }
    }

    void Equip()
    {
        for (int i = 0; i < items.Length - 1; i++)
        {
            // check number keys to equip that item
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                EquipItem(i);
                break;
            }
        }

        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            EquipItem(itemIndex + 1);
        }
        else if (scroll < 0f)
        {
            EquipItem(itemIndex - 1);
        }
    }

    bool IsSprinting()
    {
        return Input.GetKey(KeyCode.LeftShift);
    }

    public void SetGroundedState(bool g)
    {
        grounded = g;
    }

    void FixedUpdate() {
        if (!PV.IsMine)
        {
            // only control ourselves
            return;
        }

        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        // sync other players' guns
        if (!PV.IsMine && targetPlayer == PV.Owner && changedProps.ContainsKey("itemIndex"))
        {
            EquipItem((int)changedProps["itemIndex"], true);
        }
    }

    void EquipItem(int i, bool force = false)
    {
        if (hasPatches)
        {
            // can't do anything while holding patches
            return;
        }

        if (!force)
        {
            // make sure new index is in range (range is all except last item - which is Patches)
            int range = items.Length - 1;
            i = (i + range) % range;
        }
        else
        {
            // still perform sanity check, but allow last item
            i = (i + items.Length) % items.Length;
        }

        if (i == previousItemIndex)
        {
            // switching to the same item, do nothing
            return;
        }

        itemIndex = i;
        items[itemIndex].itemGameObject.SetActive(true);

        if (previousItemIndex != -1)
        {
            // disable previous item
            items[previousItemIndex].itemGameObject.SetActive(false);
        }

        previousItemIndex = itemIndex;

        // weapon sync
        if (PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    public void EquipPatches()
    {
        EquipItem(items.Length - 1, true);
        hasPatches = true;
    }

    // IDamageable

    public void TakeDamage(float damage)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage, int actorNumber)
    {
        if (!PV.IsMine)
        {
            // ignore everyone who isn't the victim
            return;
        }

        currentHealth -= damage;

        healthbarImage.fillAmount = currentHealth / maxHealth;

        if (currentHealth <= 0)
        {
            // report kill back to last player to hit
            PV.RPC("RPC_ReportKill", RpcTarget.All, actorNumber);
            Die();
        }
        else
        {
            StartCoroutine(NotifyDamage());
        }
    }

    void Die(bool voided = false)
    {
        // drop patches
        hasPatches = false;
        PatchesPickup.Instance.gameObject.SetActive(true);

        Vector3 patchesSpawn = new Vector3(
            transform.position.x,
            transform.position.y,
            transform.position.z
        );

        if (voided)
        {
            // fell off the map, spawn patches on a spawn point instead
            Transform spawn = SpawnManager.Instance.GetSpawnPoint();
            patchesSpawn.Set(spawn.position.x, spawn.position.y, spawn.position.z);
        }

        PatchesPickup.Instance.Drop(patchesSpawn.x, patchesSpawn.y, patchesSpawn.z);

        // reset vignette before dying
        vignette.color.Override(new Color(0f, 0f, 0f, 1f));
        playerManager.Die();
    }

    [PunRPC]
    void RPC_ReportKill(int actorNumber)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
        {
            ReportKill();
        }

    }

    void ReportKill()
    {
        playerManager.ReportKill();
    }

    // Coroutines

    IEnumerator NotifyDamage()
    {
        // flash red vignette
        vignette.color.Override(new Color(1f, 0f, 0f, 1f));
        yield return new WaitForSeconds(.2f);
        vignette.color.Override(new Color(0f, 0f, 0f, 1f));
    }
}
