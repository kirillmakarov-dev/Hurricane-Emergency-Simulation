using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Rendering;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }
    [Header("Settings")]
    [SerializeField] private Image fadeImage;     // Black Image over Canvas
    [Header("Timing")]
    [SerializeField] private float fadeOut = 0.5f;     // blackout time
    [SerializeField] private float fadeIn = 0.5f;     // time to manifest

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // stretching to full screen
        if (fadeImage)
        {
            var rt = fadeImage.rectTransform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.anchoredPosition = Vector2.zero; rt.sizeDelta = Vector2.zero;
        }
        SetAlpha(0f);
        fadeImage.gameObject.SetActive(false);
    }

    public void Fade(Action midAction = null)
    {
        StartCoroutine(FadeRoutine(midAction));
    }

    public void FadeAsync(Func<System.Threading.Tasks.Task> midAction = null)
    {
        StartCoroutine(FadeRoutineAsync(midAction));
    }

    private IEnumerator FadeRoutine(Action midAction = null)
    {
        // 1) Fade Out
        fadeImage.gameObject.SetActive(true);
        yield return LerpAlpha(0f, 1f, fadeOut);

        // 2) Action "between" (level/screen/resource change)
        midAction?.Invoke();

        // 3) Fade In
        yield return LerpAlpha(1f, 0f, fadeIn);
        fadeImage.gameObject.SetActive(false);
    }

    private IEnumerator FadeRoutineAsync(Func<System.Threading.Tasks.Task> midAction = null)
    {
        // 1) Fade Out
        fadeImage.gameObject.SetActive(true);
        yield return LerpAlpha(0f, 1f, fadeOut);

        // 2) Action "between"
        if (midAction != null)
        {
            var task = midAction.Invoke();
            yield return new WaitUntil(() => task.IsCompleted);
        }

        // 3) Fade In
        yield return LerpAlpha(1f, 0f, fadeIn);
        fadeImage.gameObject.SetActive(false);
    }

    public void FadeIn()
    {
        StartCoroutine(LerpAlpha(1f, 0f, fadeIn));
        fadeImage.gameObject.SetActive(false);
    }
    public void FadeOut()
    {
        fadeImage.gameObject.SetActive(true);
        StartCoroutine(LerpAlpha(0f, 1f, fadeOut));
    }

    private IEnumerator LerpAlpha(float from, float to, float duration)
    {
        Color c = fadeImage.color;
        if (duration <= 0f) { c.a = to; fadeImage.color = c; yield break; }

        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float a = Mathf.Lerp(from, to, t / duration);
            c.a = a; fadeImage.color = c;
            yield return null;
        }
        c.a = to; fadeImage.color = c;
    }

    private void SetAlpha(float a)
    {
        var c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }

}
