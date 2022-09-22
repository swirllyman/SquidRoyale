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
    [SerializeField] Color startColor;
    [SerializeField] Transform targetArrowTransform;
    [SerializeField] SpriteRenderer targettingArrowRend;
    [SerializeField] SpriteRenderer targettingArrowRendBG;
    [SerializeField] Tentacles tentacles;

    Collider2D currentHitCollider;
    float shotPercInternal;
    float currentShotCD = 0.0f;
    bool shooting = false;
    bool onCD = false;

    [Networked]
    bool hooked { get; set; }


    [Networked]
    bool showingArrow { get; set; }

    [Networked]
    private bool cancelled { get; set; }

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
        if (!changed.Behaviour.chargeClick && changed.Behaviour.currentShotCD <= 0.0f &! changed.Behaviour.cancelled)
        {
            changed.Behaviour.ShootTentacles();
        }
    }

    private void Awake()
    {
        targettingArrowRend.enabled = false;
        targettingArrowRendBG.enabled = false;
    }

    public override void Spawned()
    {
        base.Spawned();
        showingArrow = false;
        hooked = false;
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
                if (showingArrow &!targettingArrowRend.enabled && HasInputAuthority)
                {
                    ShowTargettingArrow();
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

            if(!hooked)
                currentHitPoint = currentAim;
        }

        if ((data.buttons & NetworkInputData.MOUSEBUTTON0_DOWN) != 0)
        {
            chargeClick = true;
        }

        if ((data.buttons & NetworkInputData.MOUSEBUTTON0_UP) != 0)
        {
            chargeClick = false;
        }

        //if ((data.buttons & NetworkInputData.MOUSEBUTTON1_DOWN) != 0 &! cancelled)
        //{
        //    //print("MB1");
        //    cancelled = true;
        //    HideTargetArrow();
        //}

        //if ((data.buttons & NetworkInputData.MOUSEBUTTON0_UP) != 0)
        //{
        //    chargeClick = false;
        //}
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

        if (Input.GetMouseButtonDown(1) && showingArrow)
        {
            CancelAim();
            HideTargetArrow();
        }
    }

    void ShootTentacles()
    {
        GameManager.singleton.abilityButtons[0].PlayEffect_Use(shotCD);
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
                hooked = true;
                return true;
            }
        }

        currentHitCollider = null;
        hooked = false;
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
        hooked = false;
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
        return chargeClick & !onCD &! cancelled;
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
        cancelled = false;
        showingArrow = true;

        targettingArrowRend.size = new Vector2(0, 1);
        targettingArrowRendBG.size = new Vector2(shootDistance, 1);
        targettingArrowRendBG.enabled = true;

        if (currentShotCD <= 0.0f)
        {
            ShowTargettingArrow();
        }
        else
        {
            GameManager.singleton.abilityButtons[0].PlayEffect_CD();
        }
    }

    void ShowTargettingArrow()
    {
        GameManager.singleton.abilityButtons[0].PlayEffect_Press();
        targettingArrowRend.enabled = true;
        LeanTween.value(gameObject, UpdateArrow, 0.0f, targettingArrowRendBG.size.x, .5f);
        LeanTween.color(controller.visualsRend.gameObject, Color.red, .25f).setLoopPingPong();
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
        LeanTween.cancel(controller.visualsRend.gameObject);
        LeanTween.color(controller.visualsRend.gameObject, startColor, .25f);
    }

    void CancelAim()
    {
        cancelled = true;
        HideTargetArrow();
    }

    void CheckTargettingArrow()
    {
        if (showingArrow)
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
            worldPosition.z = 0;

            targetArrowTransform.transform.right = worldPosition - transform.position;

            if(!onCD)
                playerVisuals.transform.up = Vector3.Lerp(playerVisuals.transform.up, /*-targetArrowTransform.transform.right*/(Vector2)playerVisuals.transform.position - currentHitPoint, Time.deltaTime * 5);
            else
                playerVisuals.transform.up = Vector3.Lerp(playerVisuals.transform.up, controller.moveDirection == Vector2.zero ? Vector2.up : controller.moveDirection, Time.deltaTime * 5);
        }
        else if (shooting)
        {
            playerVisuals.transform.up = Vector3.Lerp(playerVisuals.transform.up, (Vector2)playerVisuals.transform.position - currentHitPoint, Time.deltaTime * 5);
        }
        else
        {
            playerVisuals.transform.up = Vector3.Lerp(playerVisuals.transform.up, controller.moveDirection == Vector2.zero ? Vector2.up : controller.moveDirection, Time.deltaTime * 10);
        }
    }
    #endregion
}
