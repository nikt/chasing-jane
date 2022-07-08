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
    [SerializeField] Transform needleTransform;
    [SerializeField] GameObject ui;
    [SerializeField] GameObject cameraHolder;

    [SerializeField] float mouseSensitivity, sprintSpeed, walkSpeed, jumpForce, smoothTime;

    // damage and materials
    [SerializeField] Material damageMaterial;
    Material originalMaterial;
    MeshRenderer meshRenderer;

    public Role role;

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

    // game state
    bool finished;

    PlayerManager playerManager;

    public bool focusOnEnable = true; // whether or not to focus and lock cursor immediately on enable
    static bool Focused {
		get => Cursor.lockState == CursorLockMode.Locked;
		set {
			Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
			Cursor.visible = value == false;
		}
	}

	public override void OnEnable() {
        base.OnEnable();
		if (focusOnEnable)
        {
            Focused = true;
        }
	}

	public override void OnDisable() {
        base.OnDisable();
        Focused = false;
    }

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

        // save renderer and material
        meshRenderer = GetComponent<MeshRenderer>();
        originalMaterial = meshRenderer.material;
    }

    void Update()
    {
        if (!PV.IsMine)
        {
            // only control ourselves
            return;
        }

        if (finished)
        {
            // stop controlling player, free cursor
            Focused = false;
            return;
        }

        if (!Focused)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Focused = true;
            }

            // unfocused, stop reading input
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

        if (transform.position.y < -10f || Input.GetKeyDown("k"))
        {
            // die if you fall out of bounds (or need to unstuck yourself)
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

        // Quaternion needleRotation = Quaternion.identity;
        // needleRotation.eulerAngles = new Vector3(0, 0, );
        needleTransform.transform.localEulerAngles = new Vector3(0, 0, transform.eulerAngles.y);
    }

    void Move()
    {
        float multiplier = hasPatches ? 0.85f : 1.0f;
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * multiplier * (IsSprinting() ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);
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
        if (!PV.IsMine || finished)
        {
            // only control ourselves
            return;
        }

        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

    public override void OnRoomPropertiesUpdate (Hashtable changedProps)
    {
        // Check for a winner
        if (PV.IsMine && changedProps.ContainsKey(RoomProperties.WINNING_ACTOR))
        {
            ui.SetActive(false);
            finished = true;
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        // sync other players' guns
        if (!PV.IsMine && targetPlayer == PV.Owner && changedProps.ContainsKey(PlayerProperties.ITEM_INDEX))
        {
            EquipItem((int)changedProps[PlayerProperties.ITEM_INDEX], true);
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
            hash.Add(PlayerProperties.ITEM_INDEX, itemIndex);
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
        // only send to the owner of this player controller
        PV.RPC(nameof(RPC_TakeDamage), PV.Owner, damage);

        StartCoroutine(NotifyDamageAll());
    }

    [PunRPC]
    void RPC_TakeDamage(float damage, PhotonMessageInfo info)
    {
        currentHealth -= damage;

        healthbarImage.fillAmount = currentHealth / maxHealth;

        if (currentHealth <= 0)
        {
            // report kill back to last player to hit
            PlayerManager.Find(info.Sender).GetKill();
            Die();
        }
        else
        {
            StartCoroutine(NotifyDamageSelf());
        }
    }

    void Die(bool voided = false)
    {
        if (hasPatches)
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
                Transform spawn = SpawnManager.Instance.GetSpawnPoint(Role.Observer);
                patchesSpawn.Set(spawn.position.x, spawn.position.y, spawn.position.z);
            }

            PatchesPickup.Instance.Drop(patchesSpawn.x, patchesSpawn.y, patchesSpawn.z);
        }

        // reset vignette before dying
        vignette.color.Override(new Color(0f, 0f, 0f, 1f));
        playerManager.Die();
    }

    // Coroutines

    IEnumerator NotifyDamageSelf()
    {
        // flash red vignette
        vignette.color.Override(new Color(1f, 0f, 0f, 1f));
        yield return new WaitForSeconds(.2f);
        vignette.color.Override(new Color(0f, 0f, 0f, 1f));
    }

    IEnumerator NotifyDamageAll()
    {
        // flash red material
        meshRenderer.material = damageMaterial;
        yield return new WaitForSeconds(.2f);
        meshRenderer.material = originalMaterial;
    }
}
