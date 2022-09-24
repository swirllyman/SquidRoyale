using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatyArms : MonoBehaviour
{
    [SerializeField] Transform ik;
    [SerializeField] float returnSpeed = 1.5f;
    // Start is called before the first frame update
    void Start()
    {
        ik.SetParent(null);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ik.transform.position = Vector3.Lerp(ik.transform.position, transform.position, Time.deltaTime * returnSpeed);
        ik.transform.rotation = Quaternion.Slerp(ik.transform.rotation, transform.rotation, Time.deltaTime * returnSpeed);
    }
}
