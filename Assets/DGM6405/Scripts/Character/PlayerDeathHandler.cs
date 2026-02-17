using System.Collections;
using UnityEngine;

/// <summary>
///     Handles the player's death sequence, including disabling components,
///     handling water floating, and triggering game over.
/// </summary>
public class PlayerDeathHandler : MonoBehaviour, IHealthListener
{
	[Header("Settings")]
	[SerializeField] private float _gameOverDelay = 3f;
	[SerializeField] private float _waterHeight = 0f;
	[SerializeField] private float _floatOffset = 0.5f;

	[Header("References")]
	[SerializeField] private CharacterContext _context;
	
	private bool _isDead;
	private bool _isFloating;

	private void Awake()
	{
		if (_context == null) _context = GetComponent<CharacterContext>();
	}

	private void OnEnable()
	{
		_context?.EventBus?.Register<IHealthListener>(this);
	}

	private void OnDisable()
	{
		_context?.EventBus?.Unregister<IHealthListener>(this);
	}

	public void OnHealthChanged(float current, float max) { }
	public void OnDamageTaken(int amount, Vector3 direction) { }

	public void OnDied()
	{
		if (_isDead) return;
		_isDead = true;

		StartCoroutine(DeathSequence());
	}

	private IEnumerator DeathSequence()
	{
		// Disable character controller to stop movement and gravity
		if (_context.Controller != null)
		{
			_context.Controller.enabled = false;
		}

		// Check for water height continuously if we are below it or near it
		float timer = 0;
		while (timer < _gameOverDelay)
		{
			if (!_isFloating && transform.position.y <= _waterHeight + _floatOffset)
			{
				_isFloating = true;
				// Snap to water surface
				Vector3 pos = transform.position;
				pos.y = _waterHeight + _floatOffset;
				transform.position = pos;
			}

			if (_isFloating)
			{
				// Keep at water surface
				Vector3 pos = transform.position;
				pos.y = _waterHeight + _floatOffset;
				transform.position = pos;
			}

			timer += Time.deltaTime;
			yield return null;
		}

		// Trigger Game Over
		GameMgr.Instance.GameOver();
	}
}
