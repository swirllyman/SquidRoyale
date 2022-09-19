using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tentacle : MonoBehaviour
{
    public Transform tentacleTip;
    [SerializeField] PlayerController playerController;
    [SerializeField] LineRenderer tentacleLine;
    [SerializeField] float shootTime = .5f;
    bool shooting = false;
    [SerializeField] Vector2 offset;
    Vector2 newTarget;

    public void ShootToPoint(Vector2 target)
    {
        shooting = true;
        offset = transform.position;
        newTarget = target;
        LeanTween.cancel(gameObject);
        LeanTween.value(gameObject, 0, 1, shootTime).setOnComplete(FinishShot).setOnUpdate(UpdateTentacle);
    }

    void UpdateTentacle(float amount)
    {
        offset = Vector2.Lerp(offset, newTarget, amount);
        tentacleTip.right = tentacleTip.position - transform.position;
        //print("Shooting Tentacles: " + tentacleLine.GetPosition(1));
    }

    void FinishShot()
    {
        offset = newTarget;
        tentacleTip.right = tentacleTip.position - transform.position;
        if (playerController.CheckCatch())
        {
            StartCoroutine(RetractRoutine(.25f, true));
        }
        else
        {
            StartCoroutine(RetractRoutine(.0f, false));
        }
    }

    IEnumerator RetractRoutine(float waitTime, bool caughtSomething)
    {
        //if(caughtSomething 

        yield return new WaitForSeconds(waitTime);
        LeanTween.value(gameObject, 0, 1, .25f).setOnUpdate(RetractTentacle).setOnComplete(FinishRetract);
    }

    void RetractTentacle(float amount)
    {
        offset = Vector2.Lerp(offset, transform.position, amount);
    }

    void FinishRetract()
    {
        shooting = false;
        offset = Vector2.zero;
        playerController.TentaclesRetracted();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        tentacleLine.SetPosition(0, transform.position);

        //tentacleLine.SetPosition(1, (Vector3)offset);

        //tentacleLine.SetPosition(1, transform.position + (Vector3)offset);
        if (!shooting)
            tentacleLine.SetPosition(1, transform.position + (Vector3)offset);
        else
            tentacleLine.SetPosition(1, (Vector3)offset);

        tentacleTip.transform.position = tentacleLine.GetPosition(1);
    }
}
