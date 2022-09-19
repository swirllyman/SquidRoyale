using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerController : NetworkBehaviour
{
    public static PlayerController localPlayer;

    [SerializeField] float moveCD = .5f;
    [SerializeField] float moveSpeedBase = 100.0f;
    [SerializeField] SpriteRenderer visualsRend;

    [Header("Targetting System")]
    public Transform aimPositionTransform;
    [SerializeField] LayerMask hitMask;
    [SerializeField] float shootDistance = 7.5f;
    [SerializeField] Transform targetArrowTransform;
    [SerializeField] SpriteRenderer targettingArrowRend;
    [SerializeField] SpriteRenderer targettingArrowRendBG;
    [SerializeField] Rigidbody2D myBody;
    [SerializeField] CircleCollider2D myCollider;
    [SerializeField] Tentacle[] tentacles;

    Collider2D currentHitCollider;
    Vector2 moveDirection;

    float currentMoveTimer = 0.0f;
    bool moving = false;
    bool showingArrow = true;
    float shotPercInternal;

    [Networked]
    private float currentShotPerc { get; set; }

    [Networked]
    public Vector2 currentAim { get; set; }

    [Networked(OnChanged = nameof(ChargeClick))]
    public NetworkBool chargeClick { get; set; }

    public static void ChargeClick(Changed<PlayerController> changed)
    {
        if (!changed.Behaviour.chargeClick)
        {
            changed.Behaviour.ShootTentacles();
        }
    }

    internal float GetShotPower()
    {
        return shotPercInternal;
    }
    internal Vector2 GetCurrentAim()
    {
        return aimPositionTransform.position;
    }

    internal float GetCurrentLookDirection()
    {
        return targetArrowTransform.eulerAngles.x;
    }


    public override void Render()
    {
        if(chargeClick)
            visualsRend.material.color = Color.Lerp(visualsRend.material.color, Color.red, Time.deltaTime);
        else
        {
            visualsRend.material.color = Color.Lerp(visualsRend.material.color, Color.white, Time.deltaTime);
        }
    }

    private void Awake()
    {
        myBody = GetComponent<Rigidbody2D>();
        HideTargetArrow();
    }

    public override void Spawned()
    {
        base.Spawned();
        if (HasInputAuthority)
        {
            localPlayer = this;
            FindObjectOfType<CinemachineVirtualCamera>().Follow = transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        CheckMoveTimer();
    }

    private void LateUpdate()
    {
        CheckTargettingArrow();
    }

    public override void FixedUpdateNetwork()
    {

        if (GetInput(out NetworkInputData data))
        {
            moveDirection = data.direction;
            currentAim = data.currentAim;
            currentShotPerc = data.shotPower;
        }

        if ((data.buttons & NetworkInputData.MOUSEBUTTON1) != 0)
        {
            chargeClick = true;
        }

        if ((data.buttons & NetworkInputData.MOUSEBUTTON1_UP) != 0)
        {
            chargeClick = false;
        }
    }

    void CheckMoveTimer()
    {
        if (moving)
        {
            currentMoveTimer -= Time.deltaTime;
            if (currentMoveTimer <= 0.0f)
            {
                moving = false;
            }
        }

        if (!moving && moveDirection != Vector2.zero)
        {
            Move(moveDirection);
        }
    }

    void CheckInput()
    {
        //moveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if(!HasInputAuthority) return;

        if (Input.GetMouseButtonDown(0))
        {
            ShowTargetArrow();
        }

        if (Input.GetMouseButtonUp(0) && showingArrow)
        {
            HideTargetArrow();
        }
    }

    void Move(Vector2 moveDirection)
    {
        myBody.AddForce(moveDirection.normalized * moveSpeedBase);
        moving = true;
        currentMoveTimer = moveCD;
    }


    void ShootTentacles()
    {
        Vector2 direction = (currentAim - (Vector2)transform.position).normalized;
        float dist = currentShotPerc * 4;

        Debug.DrawRay(transform.position, direction * dist, Color.red, 1.5f);


        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, .35f, direction, dist, hitMask);
        Vector2 hitPoint = currentAim;
        currentHitCollider = null;

        for (int i = hits.Length - 1; i >= 0; i--)

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider != myCollider)
            {
                hitPoint = hit.point;
                currentHitCollider = hit.collider;
                    break;
            }
        }
        foreach (Tentacle t in tentacles)
        {
            t.ShootToPoint(hitPoint);
        }
    }

    internal bool CheckCatch()
    {
        Collider2D[] connectedTargets = Physics2D.OverlapCircleAll(tentacles[0].tentacleTip.position, .35f);
        foreach (Collider2D connectedTarget in connectedTargets)
        {
            if (connectedTarget == currentHitCollider)
            {
                if (currentHitCollider.CompareTag("Fish"))
                {
                    currentHitCollider.GetComponent<AI_Base>().Catch();
                    currentHitCollider.transform.parent = tentacles[0].tentacleTip;
                }
                return true;
            }
        }

        return false;
    }

    internal void TentaclesRetracted()
    {
        if(currentHitCollider != null)
        {
            if (currentHitCollider.CompareTag("Fish"))
            {
                if (HasInputAuthority)
                    RPC_DestroyFish(currentHitCollider.GetComponent<NetworkObject>());
            }
        }
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    void RPC_DestroyFish(NetworkObject fishObject)
    {
        if (Runner.IsServer)
        {
            Runner.Despawn(fishObject);
            FindObjectOfType<FishSpawner>().currentPopulationCount--;
        }
    }


    [ContextMenu("Show Arrow")]
    void ShowTargetArrow()
    {
        showingArrow = true;
        targettingArrowRend.size = new Vector2(0, 1);
        targettingArrowRendBG.size = new Vector2(shootDistance, 1);
        targettingArrowRend.enabled = true;
        targettingArrowRendBG.enabled = true;
        LeanTween.value(gameObject, UpdateArrow, 0.0f, targettingArrowRendBG.size.x, .5f);
    }

    void UpdateArrow(float f)
    {
        shotPercInternal = f / targettingArrowRendBG.size.x;
        aimPositionTransform.localPosition = new Vector3(f / 2, -.33f, 0);
        targettingArrowRend.size = new Vector2(f, 1);
    }

    [ContextMenu("Hide Arrow")]
    void HideTargetArrow()
    {
        //aimPositionTransform.localPosition = new Vector3(0, -.33f, 0);
        showingArrow = false;
        targettingArrowRend.enabled = false;
        targettingArrowRendBG.enabled = false;
    }

    void CheckTargettingArrow()
    {
        if (showingArrow)
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
            worldPosition.z = 0;

            targetArrowTransform.transform.right = worldPosition - transform.position;
        }
    }
}
