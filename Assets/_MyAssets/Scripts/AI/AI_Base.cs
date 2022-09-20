using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Base : NetworkBehaviour
{
    [SerializeField] Vector2 moveCheckCDMinMax = new Vector2(2.5f, 7.5f);
    [SerializeField] float standardMoveSpeed = 1;
    [SerializeField] Vector2 moveMinMax = new Vector2(1, 5);
    [SerializeField] Vector2 swimTimeMinMax = new Vector2(.75f, 2.5f);
    [SerializeField] LayerMask hitMask;
    [SerializeField] Animator myAnim;
    [SerializeField] Transform myVisualsTransform;
    [SerializeField] SpriteRenderer myVisualsRend;
    [SerializeField] Rigidbody2D myBody;
    [SerializeField] Collider2D myCollider;

    float currentMoveAmount = 0;
    bool spinAround = false;

    public void Catch()
    {
        myBody.isKinematic = true;
        myBody.velocity = Vector2.zero;
        myCollider.enabled = false;
        GetComponent<NetworkRigidbody2D>().enabled = false;
    }

    public override void Spawned()
    {
        base.Spawned();
        if (Runner.IsServer)
        {
            StartCoroutine(MoveRoutine());
        }
    }

    IEnumerator MoveRoutine()
    {
        yield return new WaitForSeconds(Random.Range(0, moveCheckCDMinMax.x));
        Move();

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(moveCheckCDMinMax.x, moveCheckCDMinMax.y));
            Move();
        }
    }

    void Move()
    {
        Vector2 moveAngle = GetMoveAngle();
        myBody.AddForce(moveAngle * currentMoveAmount * standardMoveSpeed);
        if(moveAngle != Vector2.one)
            RPC_Move(moveAngle, currentMoveAmount);
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    void RPC_Move(Vector2 moveAngle, float moveAmount)
    {
        myAnim.SetBool("Swim", true);
        if (moveAngle.x < 0)
        {
            spinAround = true;
        }
        else
        {
            spinAround = false;
        }
        myVisualsRend.flipX = spinAround;

        StartCoroutine(SwimRoutine(moveAmount));
    }

    IEnumerator SwimRoutine(float moveAmount)
    {
        yield return new WaitForSeconds(Mathf.Lerp(swimTimeMinMax.x, swimTimeMinMax.y, moveAmount / moveMinMax.y));
        myAnim.speed = .15f;
        yield return new WaitForSeconds(.25f);

        myAnim.SetBool("Swim", false);
        myAnim.speed = 1.0f;
    }

    //Very rudimentary raycast check to see if anything is in our way
    Vector2 GetMoveAngle()
    {
        Vector2 returnVector = Vector2.one;

        int tries = 50;
        while(returnVector == Vector2.one && tries > 0)
        {
            tries--;

            //Random Angle
            Vector2 randomAngleVector = Random.insideUnitCircle;

            currentMoveAmount = Random.Range(moveMinMax.x, moveMinMax.y);

            if (!Physics2D.Raycast(myVisualsTransform.position, randomAngleVector, currentMoveAmount, hitMask))
            {
                returnVector = randomAngleVector;
                Debug.DrawRay(myVisualsTransform.position, randomAngleVector * currentMoveAmount, Color.red, 1.5f);
            }
        }

        return returnVector;
    }
}
