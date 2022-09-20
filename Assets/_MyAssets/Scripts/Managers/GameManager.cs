using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager singleton;
    public ScreenSaver screenSaver;

    private void Awake()
    {
        singleton = this;
    }
}
