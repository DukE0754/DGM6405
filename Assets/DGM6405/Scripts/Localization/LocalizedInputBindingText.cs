using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public class LocalizedInputBindingText : MonoBehaviour
{
#region Serialized Fields

	[SerializeField] private LocalizeStringEvent _localizeStringEvent;
	[SerializeField] private InputActionReference _inputActionReference;
	[SerializeField] private InputControlLocalizationMap _controlMap;
	[SerializeField] private string _variableName = "jumpBinding";

#endregion

#region Private Fields

	private PlayerInput _playerInput;
	private string _lastScheme;
	private StringVariable _variable;

#endregion

#region Unity Lifecycle

	private void OnEnable()
	{
		_variable = _localizeStringEvent.StringReference[_variableName] as StringVariable;

		if (_variable == null)
		{
			Debug.LogError($"LocalizedInputBindingText: Variable '{_variableName}' not found.");
			return;
		}

		TryAttachPlayer();
		UpdateBinding();
	}

	private void Update()
	{
		if (_playerInput == null)
		{
			TryAttachPlayer();
			return;
		}

		if (_playerInput.currentControlScheme != _lastScheme)
		{
			_lastScheme = _playerInput.currentControlScheme;
			UpdateBinding();
		}
	}

#endregion

#region Private Methods

	private void TryAttachPlayer()
	{
		if (PlayerMgr.Instance?.PlayerObject == null)
			return;

		if (PlayerMgr.Instance.PlayerObject.TryGetComponent(out PlayerInput playerInput))
		{
			_playerInput = playerInput;
		}
	}

	private void UpdateBinding()
	{
		if (_variable == null || _inputActionReference == null || _controlMap == null)
			return;

		var text = _controlMap.Resolve(_inputActionReference.action, _playerInput);

		if (_variable.Value != text)
			_variable.Value = text;
	}

#endregion
}
