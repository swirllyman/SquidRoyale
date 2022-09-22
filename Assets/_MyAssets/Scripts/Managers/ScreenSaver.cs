using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScreenSaver : MonoBehaviour
{
    [SerializeField] TMP_Text loadingText;
    [SerializeField] Image loadScreenImage;
    [SerializeField] Color[] loadingTextColors;
    bool loading = false;
    Coroutine loadingRoutine;
    string[] dots = { "", ".", ". .", ". . ." };

    private void Awake()
    {
        PlayLoadScreen(0.0f);
    }

    [ContextMenu("Show Load Screen")]
    public void PlayLoadScreen(float loadInTime = 1.0f)
    {
        loading = true;
        if (loadingRoutine != null) StopCoroutine(loadingRoutine);
        loadingRoutine = StartCoroutine(LoadRoutine());
        loadScreenImage.enabled = true;
        loadScreenImage.color = Color.clear;
        LeanTween.color(loadScreenImage.rectTransform, Color.white, loadInTime);
        loadingText.color = new Color(loadingText.color.r, loadingText.color.g, loadingText.color.b, 1.0f);
        loadingText.enabled = true;
        loadingText.text = "Loading";
        LeanTween.value(loadingText.gameObject, 0, 1, 1.0f).setLoopPingPong().setOnUpdate(UpdateTextColor);
        //print("Loading SS");
    }

    [ContextMenu("Hide Load Screen")]
    public void HideLoadScreen()
    {
        loading = false;
        if (loadingRoutine != null) StopCoroutine(loadingRoutine);
        LeanTween.color(loadScreenImage.rectTransform, Color.clear, 1.0f).setOnComplete(FinishedLoading);
        LeanTween.cancel(loadingText.rectTransform);
        loadingText.enabled = false;
        //print("Hiding SS");
    }

    void UpdateTextColor(float amount)
    {
        loadingText.color = Color.Lerp(loadingTextColors[0], loadingTextColors[1], amount);
    }

    void FinishedLoading()
    {
        loadScreenImage.enabled = false;
        //print("SS Hidden");
    }

    IEnumerator LoadRoutine()
    {
        while (loading)
        {
            for (int i = 0; i < 4; i++)
            {
                loadingText.text = "Loading " + dots[i];
                yield return new WaitForSeconds(1.0f);
            }
        }
    }
}
