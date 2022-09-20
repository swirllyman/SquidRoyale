using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TentacleShooter : NetworkBehaviour
{
    public static TentacleShooter localPlayer;
    [SerializeField] PlayerController controller;
    [SerializeField] Transform playerVisuals;

    [SerializeField] float shotCD = 2.0f;

    [Header("Targetting System")]
    public Transform aimPositionTransform;
    public float shootDistance = 7.5f;
    [SerializeField] LayerMask hitMask;
    [SerializeField] Transform targetArrowTransform;
    [SerializeField] SpriteRenderer targettingArrowRend;
    [SerializeField] SpriteRenderer targettingArrowRendBG;
    [SerializeField] Tentacles tentacles;

    Collider2D currentHitCollider;
    float shotPercInternal;
    float currentShotCD = 0.0f;
    bool showingArrow = true;
    bool shooting = false;
    bool onCD = false;

    [Networked]
    private float currentShotPerc { get; set; }

    [Networked]
    public Vector2 currentAim { get; set; }

    [Networked]
    public Vector2 currentHitPoint { get; set; }

    [Networked(OnChanged = nameof(ChargeClick))]
    public NetworkBool chargeClick { get; set; }

    public static void ChargeClick(Changed<TentacleShooter> changed)
    {
        if (!changed.Behaviour.chargeClick && changed.Behaviour.currentShotCD <= 0.0f)
        {
            changed.Behaviour.ShootTentacles();
        }
    }

    private void Awake()
    {
        HideTargetArrow();
    }

    public override void Spawned()
    {
        base.Spawned();
        if (HasInputAuthority)
        {
            localPlayer = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        if (currentShotCD > 0.0f)
        {
            currentShotCD -= Time.deltaTime;
        }
        else
        {
            if (onCD)
            {
                onCD = false;
                if (showingArrow)
                {
                    targettingArrowRend.enabled = true;
                    LeanTween.value(gameObject, UpdateArrow, 0.0f, targettingArrowRendBG.size.x, .5f);
                }
            }
        }
    }

    private void LateUpdate()
    {
        CheckTargettingArrow();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
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

    void CheckInput()
    {
        //moveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (!HasInputAuthority) return;

        if (Input.GetMouseButtonDown(0))
        {
            ShowTargetArrow();
        }

        if (Input.GetMouseButtonUp(0) && showingArrow)
        {
            HideTargetArrow();
        }
    }

    void ShootTentacles()
    {
        currentShotCD = shotCD;
        onCD = true;
        shooting = true;
        Vector2 direction = (currentAim - (Vector2)transform.position).normalized;
        float dist = currentShotPerc * 4;

        Debug.DrawRay(transform.position, direction * dist, Color.red, 1.5f);


        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, .35f, direction, dist, hitMask);
        currentHitPoint = currentAim;
        currentHitCollider = null;

        for (int i = hits.Length - 1; i >= 0; i--)
        {
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.collider != controller.myCollider)
                {
                    currentHitPoint = hit.point;
                    currentHitCollider = hit.collider;
                    break;
                }
            }
        }

        aimPositionTransform.position = currentHitPoint;
        tentacles.ShootToPoint(currentHitPoint);
    }


    internal bool CheckCatch()
    {
        Collider2D[] connectedTargets = Physics2D.OverlapCircleAll(tentacles.tentacleTips[0].transform.position, .35f);
        foreach (Collider2D connectedTarget in connectedTargets)
        {
            if (connectedTarget == currentHitCollider)
            {
                if (currentHitCollider.CompareTag("Fish"))
                {
                    currentHitCollider.GetComponent<AI_Base>().Catch();
                    currentHitCollider.transform.parent = tentacles.tentacleTips[0];
                }
                return true;
            }
        }

        currentHitCollider = null;

        return false;
    }

    internal void TentaclesRetracted()
    {
        if (currentHitCollider != null)
        {
            if (currentHitCollider.CompareTag("Fish"))
            {
                if (HasInputAuthority)
                    RPC_DestroyFish(currentHitCollider.GetComponent<NetworkObject>());

                currentHitCollider = null;
            }
        }

        shooting = false;
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

    internal bool Aiming()
    {
        return chargeClick & !onCD;
    }

    internal bool Shooting()
    {
        return shooting;
    }

    #region RPCs

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    void RPC_DestroyFish(NetworkObject fishObject)
    {
        if (Runner.IsServer)
        {
            Runner.Despawn(fishObject);
            FindObjectOfType<FishSpawner>().currentPopulationCount--;
        }
    }
    #endregion

    #region Targetting
    [ContextMenu("Show Arrow")]
    void ShowTargetArrow()
    {
        showingArrow = true;

        targettingArrowRend.size = new Vector2(0, 1);
        targettingArrowRendBG.size = new Vector2(shootDistance, 1);
        targettingArrowRendBG.enabled = true;

        if (currentShotCD <= 0.0f)
        {
            targettingArrowRend.enabled = true;
            LeanTween.value(gameObject, UpdateArrow, 0.0f, targettingArrowRendBG.size.x, .5f);
        }
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
        if (chargeClick)
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
            worldPosition.z = 0;

            targetArrowTransform.transform.right = worldPosition - transform.position;

            if(!onCD)
                playerVisuals.transform.up = Vector3.Lerp(playerVisuals.transform.up, /*-targetArrowTransform.transform.right*/(Vector2)playerVisuals.transform.position - currentHitPoint, Time.deltaTime * 5);
            else
                playerVisuals.transform.up = Vector3.Lerp(playerVisuals.transform.up, Vector2.up, Time.deltaTime * 5);
        }
        else if (shooting)
        {
            playerVisuals.transform.up = Vector3.Lerp(playerVisuals.transform.up, /*-targetArrowTransform.transform.right*/ (Vector2)playerVisuals.transform.position - currentHitPoint, Time.deltaTime * 5);
        }
        else
        {
            playerVisuals.transform.up = Vector3.Lerp(playerVisuals.transform.up, controller.moveDirection == Vector2.zero ? Vector2.up : controller.moveDirection, Time.deltaTime * 10);
        }
    }
    #endregion
}
