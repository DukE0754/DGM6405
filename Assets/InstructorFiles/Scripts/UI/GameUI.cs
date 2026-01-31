using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// In game HUD shown when not paused
/// </summary>
public class GameUI : MenuBase
{
	[Header("Timer")]
	[SerializeField] private TMP_Text _timerText;

	[Header("Abilities")]
	[SerializeField] private Image _blockIcon;
	[SerializeField] private Image _shootIcon;
	[SerializeField] private Image _meleeIcon;
	[SerializeField] private Color _lockedColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
	[SerializeField] private Color _unlockedColor = Color.white;

	private GameLoopManager _gameLoop;

	public override GameMenus MenuType()
	{
		return GameMenus.InGameUI;
	}

	protected virtual void Update()
	{
		UpdateTimer();
	}

	private void OnEnable()
	{
		RefreshAbilityIcons();
	}

	private void UpdateTimer()
	{
		if (_timerText == null) return;

		if (_gameLoop == null)
		{
			_gameLoop = FindFirstObjectByType<GameLoopManager>();
		}

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
}
