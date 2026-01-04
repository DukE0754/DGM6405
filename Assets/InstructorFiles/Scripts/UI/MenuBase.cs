using System;
using System.Collections;
using UnityEngine;

/// <summary>
///     Base class for menus
///     Allows for transitioning as menu is revealed or hidden
///     Also a half-fade for blockers
/// </summary>
[RequireComponent(typeof(Canvas))] // Ensures Menu has Canvas
[RequireComponent(typeof(CanvasGroup))] // Ensures Canvas has a CanvasGroup
public class MenuBase : MonoBehaviour
{
    /// <summary>
    /// Menus should not be interactable while fading.
    /// Use to prevent double-click issues.
    /// </summary>
    protected bool Interactable = false;

    /// <summary>
    /// Canvas of the menu
    /// We could alternatively have these menus as children of a single canvas
    /// but then fading the canvas is trickier.
    /// </summary>
    private Canvas _canvas;

    /// <summary>
    /// Canvas group allows us to fade the alpha
    /// </summary>
    private CanvasGroup _canvasGroup;

    /// <summary>
    /// Track the currently running coroutine so we can cancel if needed
    /// </summary>
    private Coroutine _fadeRoutine;

    public int SortOrder
    {
        get => _canvas.sortingOrder;
        set => _canvas.sortingOrder = value;
    }

    /// <summary>
    ///     References <see cref="GameMenus" /> to know what type this menu is
    ///     Expects only 1 of each type
    /// </summary>
    /// <returns></returns>
    public virtual GameMenus MenuType()
    {
        return GameMenus.None;
    }

    public void OnInstantiate()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.overrideSorting = true;
        _canvasGroup = GetComponent<CanvasGroup>();
        
        _canvasGroup.alpha = 0;
    }

    private void RevealFader()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.gameObject.SetActive(true);
    }

    private void HideFader()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.gameObject.SetActive(false);
    }

    public void PerformFullFadeIn(float duration, Action onFadeInComplete = null)
    {
        Interactable = false;
        // Turn the object on (or the coroutine can't run)
        RevealFader();
        Fade(1.0f, duration, () =>
        {
            // Override the callback so we can set the menu as interactable
            Interactable = true;
            onFadeInComplete?.Invoke();
        });
    }

    public void PerformHalfFadeIn(float duration, Action onFadeInComplete = null)
    {
        Interactable = false;
        RevealFader();
        Fade(0.5f, duration, onFadeInComplete);
    }

    public void PerformFullFadeOut(float duration, Action onFadeOutComplete = null)
    {
        Interactable = false;
        Fade(0.0f, duration, ()=>
        {
            // Override the callback so we can fully turn the object off.
            HideFader();
            onFadeOutComplete?.Invoke();
        });
    }

    public void Fade(float targetAlpha, float duration, Action onComplete = null)
    {
        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(
            FadeRoutine(targetAlpha, duration, onComplete)
        );
    }

    private IEnumerator FadeRoutine(
        float endAlpha,
        float duration,
        Action onComplete)
    {
        var startAlpha = _canvasGroup.alpha;
        var elapsed = 0f;

        // Edge case: instant fade
        if (duration <= 0f)
        {
            _canvasGroup.alpha = endAlpha;
            onComplete?.Invoke();
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        _canvasGroup.alpha = endAlpha;
        onComplete?.Invoke();
    }
}