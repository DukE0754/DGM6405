using System.Collections;
using UnityEngine;

/// <summary>
///     Handles the player's death sequence, including disabling components,
///     handling water floating, and triggering game over.
/// </summary>
public class PlayerDeathHandler : MonoBehaviour, IHealthListener, IWaterVolumeListener
{
	[Header("Settings")]
	[SerializeField] private float _gameOverDelay = 3f;
	[SerializeField] private float _waterHeight = 0f;
	[SerializeField] private float _floatOffset = 0.5f;
	[SerializeField] private float _floatSpeed = 2f;

	[Header("References")]
	[SerializeField] private CharacterContext _context;
	
	private bool _isDead;
	private bool _isFloating;
	private float _currentWaterHeight;

	private void Awake()
	{
		if (_context == null) _context = GetComponent<CharacterContext>();
	}

	private void OnEnable()
	{
		_context?.EventBus?.Register<IHealthListener>(this);
		_context?.EventBus?.Register<IWaterVolumeListener>(this);
	}

	private void OnDisable()
	{
		_context?.EventBus?.Unregister<IHealthListener>(this);
		_context?.EventBus?.Unregister<IWaterVolumeListener>(this);
	}

	public void OnEnteredWater(float surfaceHeight)
	{
		_currentWaterHeight = surfaceHeight;
	}

	public void OnExitedWater()
	{
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

		// Smoothly float to water surface if we are in/near water
		float timer = 0;
		Vector3 startPos = transform.position;
		
		while (timer < _gameOverDelay)
		{
			// If we are below the floating target, move up smoothly
			float targetY = _currentWaterHeight + _floatOffset;
			if (transform.position.y < targetY)
			{
				Vector3 pos = transform.position;
				pos.y = Mathf.MoveTowards(pos.y, targetY, _floatSpeed * Time.deltaTime);
				transform.position = pos;
			}

			timer += Time.deltaTime;
			yield return null;
		}

		// Trigger Game Over
		GameMgr.Instance.GameOver();
	}
}
