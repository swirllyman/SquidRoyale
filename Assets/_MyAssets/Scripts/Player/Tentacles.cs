using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tentacles : MonoBehaviour
{
    public Transform[] tentacleTips;
    [SerializeField] TentacleShooter shooter;
    [SerializeField] LineRenderer[] tentacleLine;
    [SerializeField] float shootTime = .5f;
    [SerializeField] float retractTime = .5f;

    [SerializeField] float catchWaitTime = 1.0f;
    [SerializeField] float missWaitTime = .25f;

    bool shooting = false;
    [SerializeField] Vector2 offset;
    Vector2 newTarget;

    public void ShootToPoint(Vector2 target)
    {
        shooting = true;
        offset = transform.position;
        newTarget = target;
        LeanTween.cancel(gameObject);

        float lerpedTime = Mathf.Lerp(.05f, shootTime, Vector2.Distance(transform.position, target) / shooter.shootDistance);

        LeanTween.value(gameObject, 0, 1, lerpedTime).setOnComplete(FinishShot).setOnUpdate(UpdateTentacle);
    }

    void UpdateTentacle(float amount)
    {
        offset = Vector2.Lerp(transform.position, newTarget, amount);
        tentacleTips[0].right = tentacleTips[0].position - transform.position;
        tentacleTips[1].right = tentacleTips[1].position - transform.position;
        //print("Shooting Tentacles: " + tentacleLine.GetPosition(1));
    }

    void FinishShot()
    {
        offset = newTarget;
        tentacleTips[0].right = tentacleTips[0].position - transform.position;
        tentacleTips[1].right = tentacleTips[1].position - transform.position;
        if (shooter.CheckCatch())
        {
            StartCoroutine(RetractRoutine(catchWaitTime, true));
        }
        else
        {
            StartCoroutine(RetractRoutine(missWaitTime, false));
        }
    }

    IEnumerator RetractRoutine(float waitTime, bool caughtSomething)
    {
        //if(caughtSomething 

        yield return new WaitForSeconds(waitTime);
        float lerpedTime = Mathf.Lerp(.05f, retractTime, Vector2.Distance(transform.position, tentacleTips[0].position) / shooter.shootDistance);

        LeanTween.value(gameObject, 0, 1, retractTime).setOnUpdate(RetractTentacle).setOnComplete(FinishRetract);
    }

    void RetractTentacle(float amount)
    {
        offset = Vector2.Lerp(newTarget, transform.position, amount);
    }

    void FinishRetract()
    {
        shooting = false;
        offset = Vector2.zero;
        shooter.TentaclesRetracted();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        tentacleLine[0].SetPosition(0, tentacleLine[0].transform.position);
        tentacleLine[1].SetPosition(0, tentacleLine[1].transform.position);
        //tentacleLine.SetPosition(1, (Vector3)offset);

        //tentacleLine.SetPosition(1, transform.position + (Vector3)offset);
        if (!shooting)
        {
            tentacleLine[0].SetPosition(1, tentacleLine[0].transform.position + (Vector3)offset);
            tentacleLine[1].SetPosition(1, tentacleLine[1].transform.position + (Vector3)offset);
        }
        else
        {
            tentacleLine[0].SetPosition(1, (Vector3)offset);
            tentacleLine[1].SetPosition(1, (Vector3)offset);
        }

        tentacleTips[0].transform.position = tentacleLine[0].GetPosition(1);
        tentacleTips[1].transform.position = tentacleLine[1].GetPosition(1);
    }
}
