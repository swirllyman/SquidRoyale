using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager singleton;
    public ScreenSaver screenSaver;
    public AbilityButton[] abilityButtons;

    private void Awake()
    {
        singleton = this;
    }

    private void Start()
    {
        screenSaver.HideLoadScreen();
    }
}
