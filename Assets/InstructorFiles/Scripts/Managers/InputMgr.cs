using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
///     Manages the input actions of the game across all menus and states
///     TODO Save and load input actions from JSON to allow for custom key binds
///     https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Actions.html#loading-actions-from-json
/// </summary>
public class InputMgr : Singleton<InputMgr>
{
	[SerializeField] private InputActionAsset _inputActionsAsset;
	private InputAction _pauseAction;

	private void OnEnable()
	{
		if (_inputActionsAsset != null)
		{
			_pauseAction = _inputActionsAsset.FindAction("Pause");
			if (_pauseAction != null)
			{
				_pauseAction.Enable();
				_pauseAction.performed += OnPauseAction;
			}
		}
	}

	private void OnDisable()
	{
		if (_pauseAction != null)
		{
			_pauseAction.performed -= OnPauseAction;
			_pauseAction.Disable();
		}
	}

	private void OnPauseAction(InputAction.CallbackContext context)
	{
		// If the player object does not exist, we are not in gameplay
		PlayerMgr.Instance?.PauseInput();
	}
}
