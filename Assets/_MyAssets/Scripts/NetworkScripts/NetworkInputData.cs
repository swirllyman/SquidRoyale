using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector3 direction;
    public Vector2 currentAim;

    public float aimAngle;
    public float shotPower;

    public bool mb1_Down;
    public bool mb1_Up;
    public bool mb2_Down;
    public bool mb2_Up;
}
