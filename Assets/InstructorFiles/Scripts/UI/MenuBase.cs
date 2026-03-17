using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

/// <summary>
///     Base class for menus
///     Allows for transitioning as menu is revealed or hidden
///     Also a half-fade for blockers
/// </summary>
[RequireComponent(typeof(Canvas))] // Ensures Menu has Canvas
[RequireComponent(typeof(CanvasGroup))] // Ensures Canvas has a CanvasGroup
[Serializable]
public class MenuBase : MonoBehaviour
{
	/// <summary>
	///     Menus should not be interactable while fading.
	///     Use to prevent double-click issues.
	/// </summary>
	[SerializeField] protected bool Interactable;

	[SerializeField] protected Selectable DefaultSelectable;

	/// <summary>
	///     Canvas of the menu
	///     We could alternatively have these menus as children of a single canvas
	///     but then fading the canvas is trickier.
	/// </summary>
	private Canvas _canvas;

	/// <summary>
	///     Canvas group allows us to fade the alpha
	/// </summary>
	private CanvasGroup _canvasGroup;

	/// <summary>
	///     Track the currently running coroutine so we can cancel if needed
	/// </summary>
	private Coroutine _fadeRoutine;

	public int SortOrder
	{
		get => _canvas.sortingOrder;
		set => _canvas.sortingOrder = value;
	}

	protected virtual void OnEnable()
	{
		// If the menu is already active and interactable, we might want to apply selection
		// if we switch to gamepad.
		if (Interactable) ApplyDefaultSelection();

		InputUser.onChange += OnInputUserChange;
		InputSystem.onActionChange += OnActionChange;
	}

	protected virtual void OnDisable()
	{
		InputUser.onChange -= OnInputUserChange;
		InputSystem.onActionChange -= OnActionChange;
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

	private void OnActionChange(object obj, InputActionChange change)
	{
		if (change == InputActionChange.ActionStarted || change == InputActionChange.ActionPerformed)
		{
			var action = obj as InputAction;
			if (action != null && action.activeControl != null)
			{
				var device = action.activeControl.device;
				// If the device is a gamepad and we are currently interactable, try to apply selection
				if (Interactable && (device is Gamepad || device is Joystick))
					// If nothing is currently selected, select the default
					if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
						ApplyDefaultSelection();
			}
		}
	}

	private void OnInputUserChange(InputUser user, InputUserChange change, InputDevice device)
	{
		if (change == InputUserChange.ControlSchemeChanged)
			// Try with a small delay because the InputSystem might not have fully switched yet
			if (Interactable)
				StartCoroutine(DelayedSelection(user));
	}

	private IEnumerator DelayedSelection(InputUser user)
	{
		yield return null; // Wait one frame
		if (Interactable) ApplyDefaultSelection(user);
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
		Fade(
			1.0f, duration, () =>
			{
				// Override the callback so we can set the menu as interactable
				Interactable = true;
				ApplyDefaultSelection();
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
		Fade(
			0.0f, duration, () =>
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

	/// <summary>
	///     Sets the default selectable if using a gamepad or non-mouse/keyboard scheme.
	/// </summary>
	protected void ApplyDefaultSelection(InputUser? specificUser = null)
	{
		if (DefaultSelectable == null) return;

		string scheme = null;

		// 1. If a specific user is provided, use their scheme
		if (specificUser.HasValue && specificUser.Value.controlScheme.HasValue)
			scheme = specificUser.Value.controlScheme.Value.name;

		// 2. Try to find a PlayerInput in the scene (active first)
		if (string.IsNullOrEmpty(scheme))
		{
			var playerInput = FindFirstObjectByType<PlayerInput>();
			if (playerInput != null) scheme = playerInput.currentControlScheme;
		}

		// 3. Fallback: Check InputUser.all (which tracks Project-wide actions)
		if (string.IsNullOrEmpty(scheme))
			foreach (var user in InputUser.all)
				if (user.controlScheme.HasValue)
				{
					scheme = user.controlScheme.Value.name;
					break;
				}

		// 4. Fallback: Check for active Gamepad/Joystick if scheme is still unknown
		if (string.IsNullOrEmpty(scheme))
		{
			if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame) scheme = "Gamepad";
			else if (Joystick.current != null && Joystick.current.wasUpdatedThisFrame) scheme = "Gamepad";
		}

		// If we still don't know, we might be in KBM mode if there's any Mouse device active
		if (string.IsNullOrEmpty(scheme))
		{
			if (Mouse.current != null && Mouse.current.wasUpdatedThisFrame) scheme = "KeyboardMouse";
			else if (Touchscreen.current != null && Touchscreen.current.wasUpdatedThisFrame) scheme = "Touch";
		}

		if (string.IsNullOrEmpty(scheme)) return;

		// Standard scheme names for KBM/Touch.
		// If the scheme is something else (like "Gamepad" or "Joystick"), we select the button.
		if (scheme.Equals("KeyboardMouse", StringComparison.OrdinalIgnoreCase) ||
			scheme.Equals("Touch", StringComparison.OrdinalIgnoreCase))
		{
			// Instead of just returning, we deselect the currently selected object.
			if (EventSystem.current != null)
				EventSystem.current.SetSelectedGameObject(null);
			return;
		}

		DefaultSelectable.Select();
	}
}
