using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityButton : MonoBehaviour
{
    [SerializeField] Image buttonIconImage;
    [SerializeField] Image buttonFillImage;
    [SerializeField] Image buttonBGImage;

    [Header("On Press Effects")]
    [SerializeField] float buttonScaleAmount_Press;

    [Header("On Use Effects")]
    [SerializeField] float buttonScaleAmount_Use;
    [SerializeField] float useEffectTime;
    [SerializeField] Color selectedColor;

    [Header("On CD Effects")]
    [SerializeField] Color cooldownColor;

    Vector3 startScale;
    Color startColorCD;
    Color startColorBG;

    private void Start()
    {
        startScale = buttonIconImage.rectTransform.localScale;
        startColorCD = buttonFillImage.color;
        startColorBG = buttonBGImage.color;
        buttonFillImage.fillAmount = 0;
    }

    [ContextMenu("Play Press Effect")]
    public void PlayEffect_Press()
    {
        buttonIconImage.rectTransform.localScale = startScale;
        LeanTween.scale(buttonIconImage.rectTransform, startScale * buttonScaleAmount_Press, useEffectTime).setLoopPingPong(1).setEaseInOutSine();
    }

    [ContextMenu("Play CD Effect")]
    public void PlayEffect_CD()
    {
        buttonFillImage.color = startColorCD;
        LeanTween.color(buttonFillImage.rectTransform, cooldownColor, .1f).setLoopPingPong(2).setEaseInOutSine();
        buttonIconImage.rectTransform.localScale = startScale;
        LeanTween.scale(buttonIconImage.rectTransform, startScale * .8f, .25f).setLoopPingPong(1).setEaseInOutSine();
    }

    [ContextMenu("Play Use Effect")]
    void PlayEffect_Use()
    {
        PlayEffect_Use(5.0f);
    }

    public void PlayEffect_Use(float cd)
    {
        buttonIconImage.rectTransform.localScale = startScale;
        LeanTween.scale(buttonIconImage.rectTransform, startScale * buttonScaleAmount_Use, useEffectTime).setLoopPingPong(1).setEaseInOutSine();
        
        buttonBGImage.color = startColorBG;
        LeanTween.color(buttonBGImage.rectTransform, selectedColor, useEffectTime).setLoopPingPong(3).setEaseInOutSine();
        buttonFillImage.fillAmount = 1;
        PlayCooldown(cd);
    }

    public void PlayCooldown(float cooldownLength)
    {
        //LeanTween.value(1, 0, cooldownLength);
        //buttonFillImage.fillAmount = 1;

        LeanTween.value(buttonFillImage.gameObject, 1, 0, cooldownLength).setOnUpdate((float f) => { buttonFillImage.fillAmount = f; }).setOnComplete(CooldownFinished);
    }

    void CooldownFinished()
    {
        buttonBGImage.color = startColorBG;
        LeanTween.color(buttonBGImage.rectTransform, selectedColor, useEffectTime).setLoopPingPong(1).setEaseInOutSine();
    }
}
