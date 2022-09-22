using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public const byte MOUSEBUTTON0_DOWN = 0x01;
    public const byte MOUSEBUTTON0_UP = 0x02;

    public const byte MOUSEBUTTON1_DOWN = 0x03;
    public const byte MOUSEBUTTON1_UP = 0x04;

    public byte buttons;
    public Vector3 direction;

    public Vector2 currentAim;
    public float aimAngle;
    public float shotPower;
}
