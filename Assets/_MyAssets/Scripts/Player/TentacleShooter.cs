using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TentacleShooter : NetworkBehaviour
{
    [SerializeField] PlayerController controller;

    [SerializeField] float abilityCD = 2.0f;

    [Header("Targetting System")]
    public float shootDistance = 7.5f;
    [SerializeField] LayerMask hitMask;
    [SerializeField] Tentacles tentacles;

    Collider2D currentHitCollider;
    float currentAbilityCD = 0.0f;
    bool shooting = false;
    bool onCD = false;
    bool chargingShot = false;

    [Networked]
    bool hooked { get; set; }

    [Networked]
    private float currentShotPerc { get; set; }

    [Networked(OnChanged = nameof(ChargeClick))]
    public NetworkBool chargeClick { get; set; }

    public static void ChargeClick(Changed<TentacleShooter> changed)
    {
        if (changed.Behaviour.chargeClick)
        {
            changed.Behaviour.controller.targettingSystem.aiming = true;
            if(changed.Behaviour.HasInputAuthority)
                changed.Behaviour.TryToStartShot();
        }
        else
        {
            if (!changed.Behaviour.onCD && changed.Behaviour.chargingShot)
            {
                changed.Behaviour.ShootTentacles();
            }
            changed.Behaviour.controller.targettingSystem.aiming = false;
        }
    }

    public override void Spawned()
    {
        base.Spawned();
        hooked = false;
    }
    private void Start()
    {
        controller.targettingSystem.onTargetCancel += TargettingSystem_onTargetCancel;
    }

    private void TargettingSystem_onTargetCancel()
    {
        RPC_StopShotStart();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentAbilityCD > 0.0f)
        {
            currentAbilityCD -= Time.deltaTime;
        }
        else
        {
            if (onCD)
            {
                onCD = false;
                if (HasInputAuthority && chargeClick)
                {
                    //ShowTargettingArrow();
                    TryToStartShot();
                }
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            currentShotPerc = data.shotPower;
            chargeClick = data.mb1_Down;
        }
    }

    void TryToStartShot()
    {
        if (!onCD)
        {
            controller.targettingSystem.ExpandTargetArrow();
            RPC_ShowShotStart();
        }
        else
        {
            GameManager.singleton.abilityButtons[0].PlayEffect_CD();
        }
    }

    void ShootTentacles()
    {
        GameManager.singleton.abilityButtons[0].PlayEffect_Use(abilityCD);
        currentAbilityCD = abilityCD;
        onCD = true;
        shooting = true;
        Vector2 direction = (controller.targettingSystem.currentAim - (Vector2)transform.position).normalized;
        float dist = currentShotPerc * 3.85f;

        Debug.DrawRay(transform.position, direction * dist, Color.red, 1.5f);

        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, .35f, direction, dist, hitMask);
        currentHitCollider = null;

        Vector2 currentHitPoint = (Vector2)transform.position + direction * dist;
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

        tentacles.ShootToPoint(currentHitPoint);

        if (HasInputAuthority)
        {
            RPC_StopShotStart();
        }
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
    void RPC_ShowShotStart()
    {
        chargingShot = true;
        LeanTween.color(controller.visualsRend.gameObject, Color.red, .5f).setLoopPingPong().setEaseInOutSine();
        //if (Runner.IsServer)
        //{
        //    Runner.Despawn(fishObject);
        //    FindObjectOfType<FishSpawner>().currentPopulationCount--;
        //}
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    void RPC_StopShotStart()
    {
        chargingShot = false;
        LeanTween.cancel(controller.visualsRend.gameObject);
        LeanTween.color(controller.visualsRend.gameObject, Color.white, .5f).setEaseInOutSine();
        //if (Runner.IsServer)
        //{
        //    Runner.Despawn(fishObject);
        //    FindObjectOfType<FishSpawner>().currentPopulationCount--;
        //}
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
    #endregion

}
