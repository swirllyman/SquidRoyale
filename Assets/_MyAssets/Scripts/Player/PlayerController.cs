using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveCD = .5f;
    [SerializeField] float moveSpeedBase = 100.0f;

    [Header("Targetting System")]
    [SerializeField] float shootDistance = 7.5f;
    [SerializeField] Transform targetArrowTransform;
    [SerializeField] SpriteRenderer targettingArrowRend;
    [SerializeField] SpriteRenderer targettingArrowRendBG;

    Rigidbody2D myBody;
    Vector2 moveDirection;

    float currentMoveTimer = 0.0f;
    bool moving = false;
    bool showingArrow = true;

    private void Awake()
    {
        myBody = GetComponent<Rigidbody2D>();
        HideTargetArrow();
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

    private void FixedUpdate()
    {
        if (!moving)
        {
            if(Mathf.Abs(moveDirection.x) > 0 || Mathf.Abs(moveDirection.y) > 0)
            {
                Move();
            }
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
    }

    void CheckInput()
    {
        moveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetMouseButtonDown(0))
        {
            ShowTargetArrow();
        }

        if (Input.GetMouseButtonUp(0) && showingArrow)
        {
            HideTargetArrow();
        }
    }

    void Move()
    {
        myBody.AddForce(moveDirection * moveSpeedBase);
        moving = true;
        currentMoveTimer = moveCD;
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
        targettingArrowRend.size = new Vector2(f, 1);
    }

    [ContextMenu("Hide Arrow")]
    void HideTargetArrow()
    {
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
