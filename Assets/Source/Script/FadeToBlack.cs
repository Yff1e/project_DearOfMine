using UnityEngine;
using System.Collections;

/// Handles screen fade transitions between acts
public class FadeToBlack : MonoBehaviour
{
    public CanvasGroup fadeCanvasGroup;
    public float defaultFadeDuration = 2f;

    private Coroutine currentFadeCoroutine;

    private void Awake()
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogError("[FadeToBlack] CanvasGroup not assigned!");
            return;
        }

        // Start invisible
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }
 /// Fade to black over specified duration
    public IEnumerator FadeOut(float duration = -1f)
    {
        if (duration < 0f) duration = defaultFadeDuration;

        if (fadeCanvasGroup == null)
        {
            Debug.LogError("[FadeToBlack] CanvasGroup is null!");
            yield break;
        }

        fadeCanvasGroup.blocksRaycasts = true;
        float elapsed = 0f;
        float startAlpha = fadeCanvasGroup.alpha;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / duration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
    }

    /// Fade from black over specified duration
    public IEnumerator FadeIn(float duration = -1f)
    {
        if (duration < 0f) duration = defaultFadeDuration;

        if (fadeCanvasGroup == null)
        {
            Debug.LogError("[FadeToBlack] CanvasGroup is null!");
            yield break;
        }

        float elapsed = 0f;
        float startAlpha = fadeCanvasGroup.alpha;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }


    /// Convenience method: fade out then in with optional callback
    public IEnumerator FadeOutAndIn(float fadeOutDuration, float fadeInDuration, System.Action onBlackCallback = null)
    {
        yield return StartCoroutine(FadeOut(fadeOutDuration));

        onBlackCallback?.Invoke();

        yield return StartCoroutine(FadeIn(fadeInDuration));
    }


    /// Stop any ongoing fade
    public void StopFade()
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
            currentFadeCoroutine = null;
        }
    }
}