using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerController : NetworkBehaviour
{
    public static PlayerController localPlayer;

    public TentacleShooter shooter;
    public CircleCollider2D myCollider;
    public Rigidbody2D myBody;

    [SerializeField] float moveCD = .5f;
    [SerializeField] float moveSpeedBase = 100.0f;
    [SerializeField] SpriteRenderer visualsRend;

    [Networked]
    public Vector2 moveDirection { get; set; }

    float currentMoveTimer = 0.0f;
    bool moving = false;
    private void Awake()
    {
        myBody = GetComponent<Rigidbody2D>();
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

    void Update()
    {
        CheckMoveTimer();

        //if(HasInputAuthority)
        //    moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);
;    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            moveDirection = data.direction;
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

    void Move(Vector2 moveDirection)
    {
        myBody.AddForce(moveDirection.normalized * moveSpeedBase * (shooter.Aiming() ? .25f : 1.0f));
        moving = true;
        currentMoveTimer = moveCD;
    }
}
