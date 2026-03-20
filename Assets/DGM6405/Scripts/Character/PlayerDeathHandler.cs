using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
///     Handles the player's death sequence, including disabling components,
///     handling water floating, and triggering game over.
/// </summary>
public class PlayerDeathHandler : MonoBehaviour, IHealthListener, IWaterVolumeListener, ISkipDeathListener
{
	[FormerlySerializedAs("_settings")]
	[Header("Settings")]
	[SerializeField] private WaterData Data;

	[SerializeField] private float _gameOverDelay = 3f;

	[Header("References")]
	[SerializeField] private CharacterContext _context;

	private float _currentWaterHeight;

	private bool _isDead;
	private bool _isFloating;
	private bool _skipRequested;

	public bool IsDead => _isDead;

	private void Awake()
	{
		if (_context == null) _context = GetComponent<CharacterContext>();
	}

	private void OnEnable()
	{
		_context?.EventBus?.Register<IHealthListener>(this);
		_context?.EventBus?.Register<IWaterVolumeListener>(this);
		_context?.EventBus?.Register<ISkipDeathListener>(this);
	}

	private void OnDisable()
	{
		_context?.EventBus?.Unregister<IHealthListener>(this);
		_context?.EventBus?.Unregister<IWaterVolumeListener>(this);
		_context?.EventBus?.Unregister<ISkipDeathListener>(this);
	}

	public void OnHealthChanged(float current, float max)
	{
	}

	public void OnDamageTaken(int amount, Vector3 direction)
	{
	}

	public void OnDied()
	{
		if (_isDead) return;
		_isDead = true;

		StartCoroutine(DeathSequence());
	}

	public void OnEnteredWater(float surfaceHeight)
	{
		_currentWaterHeight = surfaceHeight;
	}

	public void OnExitedWater()
	{
	}

	public void OnSkipDeathAnimation()
	{
		if (_isDead) _skipRequested = true;
	}

	private IEnumerator DeathSequence()
	{
		// Disable character controller to stop movement and gravity
		if (_context.Controller != null) _context.Controller.enabled = false;

		// Smoothly float to water surface if we are in/near water
		float timer = 0;
		var startPos = transform.position;

		var floatOffset = Data != null ? Data.FloatOffset : 0.5f;
		var floatSpeed = Data != null ? Data.FloatSpeed : 2f;

		while (timer < _gameOverDelay)
		{
			if (_skipRequested) break;

			// If we are below the floating target, move up smoothly
			var targetY = _currentWaterHeight + floatOffset;
			if (transform.position.y < targetY)
			{
				var pos = transform.position;
				pos.y = Mathf.MoveTowards(pos.y, targetY, floatSpeed * Time.deltaTime);
				transform.position = pos;
			}

			timer += Time.deltaTime;
			yield return null;
		}

		// Trigger Game Over
		GameMgr.Instance.GameOver();
	}
}
