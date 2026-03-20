using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

/// <summary>
///     In game HUD shown when not paused
/// </summary>
public class GameUI : MenuBase, IUnsupportedInputListener
{
	[Header("Timer")]
	[SerializeField] private TMP_Text _timerText;

	[Header("Abilities")]
	[SerializeField] private Image _blockIcon;

	[SerializeField] private Image _shootIcon;
	[SerializeField] private Image _meleeIcon;
	[SerializeField] private Color _lockedColor = new(0.2f, 0.2f, 0.2f, 0.5f);
	[SerializeField] private Color _unlockedColor = Color.white;


	[SerializeField] private TMP_Text _unsupportedInputText;
	[SerializeField] private LocalizeStringEvent _localizeStringEvent;
	[SerializeField] private string _controlNameVariableName = "controlName";
	[SerializeField] private float _messageDuration = 1.5f;
	[SerializeField] private AudioSource _unboundAudioSource;
	[SerializeField] private bool _debug;
	
	
	private GameLoopManager _gameLoop;

	private Coroutine _unsupportedRoutine;

	private void Update()
	{
		UpdateTimer();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (_unsupportedInputText != null)
		{
			var color = _unsupportedInputText.color;
			color.a = 0f;
			_unsupportedInputText.color = color;
			_unsupportedInputText.gameObject.SetActive(false);
		}

		RefreshAbilityIcons();
		GlobalEventBus.Instance?.Register<IUnsupportedInputListener>(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		GlobalEventBus.Instance?.Unregister<IUnsupportedInputListener>(this);
	}

	public override GameMenus MenuType()
	{
		return GameMenus.InGameUI;
	}

	private void UpdateTimer()
	{
		if (_timerText == null) return;

		if (_gameLoop == null) _gameLoop = FindFirstObjectByType<GameLoopManager>();

		if (_gameLoop != null)
		{
			var time = TimeSpan.FromSeconds(_gameLoop.GameTimer);
			_timerText.text = time.ToString(@"mm\:ss\.ff");
		}
		else
		{
			_timerText.text = "00:00.00";
		}
	}

	public void RefreshAbilityIcons()
	{
		if (LevelMgr.Instance == null) return;
		if (LevelMgr.Instance.TryGetCurrentLevelInfo(out var info))
		{
			SetIconStatus(_blockIcon, info.AllowBlock);
			SetIconStatus(_shootIcon, info.AllowShoot);
			SetIconStatus(_meleeIcon, info.AllowMelee);
		}
		else
		{
			// Test scene: everything unlocked
			SetIconStatus(_blockIcon, true);
			SetIconStatus(_shootIcon, true);
			SetIconStatus(_meleeIcon, true);
		}
	}

	private void SetIconStatus(Image icon, bool unlocked)
	{
		if (icon == null) return;
		icon.color = unlocked ? _unlockedColor : _lockedColor;
	}

#region Unsupported Input UI

	public void OnUnsupportedInput(string controlName)
	{
		if (_debug) Debug.Log($"[GameUI] Received OnUnsupportedInput event for: {controlName}");
		HandleUnsupportedInput(controlName);
	}

	private void HandleUnsupportedInput(string controlName)
	{
		if (_unsupportedInputText == null)
		{
			if (_debug) Debug.LogWarning("[GameUI] _unsupportedInputText is null! Cannot show message.");
			return;
		}

		if (_unsupportedRoutine != null)
			StopCoroutine(_unsupportedRoutine);

		_unsupportedRoutine = StartCoroutine(UnsupportedInputRoutine(controlName));
	}

	private IEnumerator UnsupportedInputRoutine(string controlName)
	{
		if (_localizeStringEvent != null && _localizeStringEvent.StringReference != null)
		{
			var variable = _localizeStringEvent.StringReference[_controlNameVariableName] as StringVariable;
			if (variable != null)
			{
				variable.Value = controlName;
			}
			else
			{
				if (_debug)
					Debug.LogWarning(
						$"[GameUI] Could not find variable '{_controlNameVariableName}' in LocalizedString.");
			}

			// LocalizeStringEvent automatically updates the text if it's connected to a TMP_Text component via its events,
			// or we can manually trigger a refresh. But usually setting the variable is enough if the component is active.
			_localizeStringEvent.RefreshString();
		}
		else
		{
			_unsupportedInputText.text = $"{controlName} not bound";
		}

		_unsupportedInputText.gameObject.SetActive(true);
		_unboundAudioSource.Play();
		
		// Fade in
		yield return FadeText(_unsupportedInputText, 0f, 1f, 0.2f);

		yield return new WaitForSecondsRealtime(_messageDuration);

		// Fade out
		yield return FadeText(_unsupportedInputText, 1f, 0f, 0.3f);

		_unsupportedInputText.gameObject.SetActive(false);
	}

	private IEnumerator FadeText(TMP_Text text, float start, float end, float duration)
	{
		var elapsed = 0f;
		var color = text.color;

		while (elapsed < duration)
		{
			elapsed += Time.unscaledDeltaTime;
			var t = Mathf.Clamp01(elapsed / duration);

			color.a = Mathf.Lerp(start, end, t);
			text.color = color;

			yield return null;
		}

		color.a = end;
		text.color = color;
	}

#endregion
}
