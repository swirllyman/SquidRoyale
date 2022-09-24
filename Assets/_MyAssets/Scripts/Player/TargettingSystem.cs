using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class TargettingSystem : NetworkBehaviour
{
    public float shootDistance = 7.5f;
    [SerializeField] LayerMask hitMask;
    [SerializeField] Transform eyeballTransform;
    [SerializeField] Transform targetArrowTransform;
    [SerializeField] SpriteRenderer targettingArrowRend;
    [SerializeField] SpriteRenderer targettingArrowRendBG;

    public delegate void OnTargetCancel();
    public event OnTargetCancel onTargetCancel;

    [Networked]
    public Vector2 currentAim { get; set; }

    [Networked(OnChanged = nameof(CanceledAim))]
    public NetworkBool cancelled { get; set; }

    [Networked(OnChanged = nameof(Aiming))]
    public NetworkBool aiming { get; set; }


    public static void Aiming(Changed<TargettingSystem> changed)
    {
        if (changed.Behaviour.aiming)
        {
            changed.Behaviour.ShowAim();
        }
        else if (!changed.Behaviour.aiming)
        {
            changed.Behaviour.CancelAim();
        }
    }

    public static void CanceledAim(Changed<TargettingSystem> changed)
    {
        if (changed.Behaviour.cancelled)
        {
            changed.Behaviour.CancelAim();
        }
    }

    private void Start()
    {
        CancelAim();
        targettingArrowRend.size = new Vector2(0, 1);
    }

    private void LateUpdate()
    {
        eyeballTransform.up = (currentAim - (Vector2)transform.position).normalized;
        targetArrowTransform.right = (currentAim - (Vector2)transform.position).normalized;
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            currentAim = data.currentAim;
            cancelled = data.mb2_Down;
        }
    }

    internal float GetShotPower()
    {
        return targettingArrowRend.size.x / targettingArrowRendBG.size.x;
    }

    /// <summary>
    /// NOTE: Only Call from Local Player
    /// </summary>
    /// <returns></returns>
    internal Vector2 LocalPlayer_CurrentAimWorldCoords() 
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPosition.z = 0;
        return worldPosition;
    }

    void ShowAim()
    {
        if (HasInputAuthority)
        {
            ShowTargetArrow();
            cancelled = false;
        }
    }

    void ShowTargetArrow()
    {
        cancelled = false;

        targettingArrowRend.size = new Vector2(0, 1);
        targettingArrowRendBG.size = new Vector2(shootDistance, 1);
        targettingArrowRendBG.enabled = true;
        targettingArrowRend.enabled = true;

        GameManager.singleton.abilityButtons[0].PlayEffect_Press();
    }

    public void ExpandTargetArrow()
    {
        LeanTween.value(gameObject, UpdateArrow, 0.0f, targettingArrowRendBG.size.x, .5f);
    }

    void UpdateArrow(float f)
    {
        targettingArrowRend.size = new Vector2(f, 1);
    }

    void HideTargetArrow()
    {
        targettingArrowRend.enabled = false;
        targettingArrowRendBG.enabled = false;
    }

    void CancelAim()
    {
        if (HasInputAuthority)
        {
            HideTargetArrow();
            onTargetCancel?.Invoke();
        }
    }
}
