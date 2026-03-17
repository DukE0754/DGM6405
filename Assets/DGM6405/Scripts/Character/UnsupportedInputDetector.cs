using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;

public class UnsupportedInputDetector : MonoBehaviour
{
#region Events

	// Replaced with IUnsupportedInputListener pattern
	// public static Action<string> OnUnsupportedInput;

#endregion

#region Private Fields

	[SerializeField] private bool _debug;
	[SerializeField] private PlayerInput _playerInput;
	private IDisposable _anyButtonListener;

	private bool _receivedAnyInputThisFrame;
	private bool _receivedActionThisFrame;
	private string _lastControlName;

	private float _lastWarningTime;
	private const float WarningCooldown = 1f;

#endregion

#region Unity Lifecycle

	private void Awake()
	{
		if (_playerInput == null)
		{
			_playerInput = GetComponent<PlayerInput>();
			if (_playerInput != null)
			{
				Debug.LogWarning($"[UnsupportedInputDetector] _playerInput was not assigned in the inspector. Found via GetComponent on {gameObject.name}. Please assign it manually for better performance.");
			}
			else
			{
				Debug.LogError($"[UnsupportedInputDetector] _playerInput is missing on {gameObject.name} and was not found via GetComponent!");
			}
		}
	}

	private void OnEnable()
	{
		if (_debug) Debug.Log($"[UnsupportedInputDetector] OnEnable called on {gameObject.name}. Subscribing to InputSystem.onAnyButtonPress.");
		_anyButtonListener = InputSystem.onAnyButtonPress
			.Call(OnAnyButtonPress);

		_playerInput.onActionTriggered += OnActionTriggered;
	}

	private void OnDisable()
	{
		if (_debug) Debug.Log($"[UnsupportedInputDetector] OnDisable called on {gameObject.name}. Disposing listeners.");
		_anyButtonListener?.Dispose();
		_playerInput.onActionTriggered -= OnActionTriggered;
	}

	private void LateUpdate()
	{
		if (_debug && (_receivedAnyInputThisFrame || _receivedActionThisFrame))
		{
			Debug.Log($"[UnsupportedInputDetector] LateUpdate: _receivedAnyInputThisFrame: {_receivedAnyInputThisFrame}, _receivedActionThisFrame: {_receivedActionThisFrame}, _lastControlName: {_lastControlName}");
		}

		if (_receivedAnyInputThisFrame)
		{
			if (!_receivedActionThisFrame)
			{
				if (Time.unscaledTime - _lastWarningTime > WarningCooldown)
				{
					_lastWarningTime = Time.unscaledTime;
					if (_debug)
						Debug.Log($"[UnsupportedInputDetector] Raising unsupported input event: {_lastControlName}");
					GlobalEventBus.Instance?.Raise<IUnsupportedInputListener>(l =>
						l.OnUnsupportedInput(_lastControlName));
				}
				else if (_debug)
				{
					Debug.Log(
						$"[UnsupportedInputDetector] Unsupported input detected but on cooldown. Last: {_lastControlName}");
				}
			}
		}

		_receivedAnyInputThisFrame = false;
		_receivedActionThisFrame = false;
	}

#endregion

#region Input Handling

	private void OnAnyButtonPress(InputControl control)
	{
		if (_debug)
			Debug.Log(
				$"[UnsupportedInputDetector] OnAnyButtonPress received control: {control.path}, Type: {control.GetType().Name}, Value: {control.ReadValueAsObject()}");

		// IsPressed() is unreliable, simply receiving the control is sufficient to proceed with checking if it is mapped

		_receivedAnyInputThisFrame = true;
		_lastControlName = control.displayName;

		// Check if this control is mapped to any action in the current player input
		_receivedActionThisFrame = IsControlMapped(control);

		if (_debug)
			Debug.Log(
				$"[UnsupportedInputDetector] Set _receivedAnyInputThisFrame to TRUE for {control.displayName}. Mapped: {_receivedActionThisFrame}");
	}

	private bool IsControlMapped(InputControl control)
	{
		if (_playerInput == null || _playerInput.actions == null) return false;

		foreach (var action in _playerInput.actions)
		{
			foreach (var binding in action.bindings)
			{
				if (InputControlPath.Matches(binding.effectivePath, control))
				{
					if (_debug)
						Debug.Log($"[UnsupportedInputDetector] Control {control.path} matches mapped action: {action.name}");
					return true;
				}
			}
		}

		return false;
	}

	private void OnActionTriggered(InputAction.CallbackContext ctx)
	{
		if (ctx.performed)
		{
			_receivedActionThisFrame = true;
			if (_debug) Debug.Log($"[UnsupportedInputDetector] OnActionTriggered Performed: {ctx.action.name}");
		}
	}

#endregion
}
