using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     The UI manager for showing various menus and state
/// </summary>
public class UIMgr : Singleton<UIMgr>
{
	[Header("Timing and sorting")]
	[SerializeField] private float _fadeInDuration = 0.5f;

	[SerializeField] private float _fadeOutDuration = 0.5f;
	[SerializeField] private int _sortGap = 10;

	[Header("Menus")]
	[SerializeField] private MenuConfiguration _menuConfiguration;

	private readonly Stack<MenuBase> _activeMenus = new();
	private readonly Dictionary<GameMenus, MenuBase> _disabledMenus = new();

	private readonly Dictionary<GameMenus, MenuBase> _menuInstances = new();

	/// <summary>
	///     Clear the stack and close all menus
	/// </summary>
	public void CloseAllMenus()
	{
		while (_activeMenus.Count > 0)
		{
			var menu = _activeMenus.Pop();
			menu.PerformFullFadeOut(_fadeOutDuration);
			_disabledMenus.Add(menu.MenuType(), menu);
		}
	}

	/// <summary>
	///     Show a menu by adding it to the stack
	/// </summary>
	/// <param name="menuToOpen"></param>
	/// <param name="onMenuOpenComplete"></param>
	/// <param name="fadeIn"></param>
	/// <returns></returns>
	public MenuBase ShowMenu(GameMenus menuToOpen, Action onMenuOpenComplete = null, bool fadeIn = true)
	{
		var menu = PushMenu(menuToOpen);
		if (!menu) return null;

		if (fadeIn)
		{
			menu.PerformFullFadeIn(_fadeInDuration, onMenuOpenComplete);
		}
		else
		{
			menu.PerformFullFadeIn(0);
			onMenuOpenComplete?.Invoke();
		}

		return menu;
	}

	public void ShowMenu(MenuBase menuPrefab, Action onMenuOpenComplete = null, bool fadeIn = true)
	{
		ShowMenu(menuPrefab.MenuType(), onMenuOpenComplete, fadeIn);
	}

	/// <summary>
	///     Use polymorphism to call a function specific to <see cref="SplashMenu" />
	/// </summary>
	/// <param name="onComplete"></param>
	public void ShowSplash(Action onComplete)
	{
		var menu = ShowMenu(GameMenus.Splash);
		if (menu != null)
		{
			menu.PerformFullFadeIn(_fadeInDuration);
			if (menu is SplashMenu splashMenu) splashMenu.OnShow(onComplete);
		}
	}

	/// <summary>
	///     Half fade the screen when long processing happens
	///     Usually only needed if contacting the internet
	/// </summary>
	/// <param name="onComplete"></param>
	/// <returns></returns>
	public MenuBase ShowHalfFader(Action onComplete)
	{
		var menu = ShowMenu(GameMenus.Fader, fadeIn: false);
		if (menu is ScreenFadeOverlay screenFadeOverlay)
			screenFadeOverlay.PerformHalfFadeIn(_fadeInDuration, onComplete);

		return menu;
	}

	/// <summary>
	///     Internal function.
	///     Pushes the given menu to the stack
	/// </summary>
	/// <param name="menu"></param>
	/// <returns></returns>
	private MenuBase PushMenu(GameMenus menu)
	{
		// Check if object already exists
		if (!_menuInstances.ContainsKey(menu))
		{
			// instantiate the game object
			var createdMenu = Instantiate(GetMenuPrefabFromType(menu), transform);
			// Call for self-setup
			createdMenu.OnInstantiate();
			_menuInstances.Add(menu, createdMenu);
		}

		var uiObj = _menuInstances[menu];

		if (_activeMenus.Contains(uiObj))
		{
			Debug.LogError($"Already opened menu {menu}");
			return uiObj;
		}

		if (_disabledMenus.ContainsKey(menu)) _disabledMenus.Remove(menu);

		int sortOverride;

		if (_activeMenus.TryPeek(out var currentTop))
			sortOverride = currentTop.SortOrder + _sortGap;
		else
			sortOverride = 0;

		uiObj.SortOrder = sortOverride;

		_activeMenus.Push(uiObj);

		return uiObj;
	}

	/// <summary>
	///     Hide a given menu
	/// </summary>
	/// <param name="menuToClose"></param>
	/// <param name="onMenuFullyHidden"></param>
	/// <param name="fadeOut"></param>
	public void HideMenu(GameMenus menuToClose, Action onMenuFullyHidden = null, bool fadeOut = true)
	{
		var menu = PopMenu(menuToClose);
		if (menu == null)
			return;

		if (fadeOut)
			menu.PerformFullFadeOut(_fadeOutDuration, onMenuFullyHidden);
		else
			onMenuFullyHidden?.Invoke();
	}

	/// <summary>
	///     Internal function.
	///     Removes a menu from the stack
	/// </summary>
	/// <param name="menu"></param>
	/// <returns></returns>
	private MenuBase PopMenu(GameMenus menu)
	{
		if (!_menuInstances.TryGetValue(menu, out var uiObj))
		{
			Debug.LogError($"Menu {menu} was never created");
			return null;
		}

		if (_activeMenus.TryPeek(out var peekedUI))
			if (peekedUI != uiObj)
			{
				Debug.LogError(
					$"The top of the stack {peekedUI.name} wasn't the object we wanted to hide {uiObj.name}");
				return null;
			}

		if (_activeMenus.TryPop(out var poppedUI))
			if (!_disabledMenus.TryAdd(menu, poppedUI))
				Debug.LogError($"Failed to add {menu} to the disabled menus list. Was it already marked as disabled?");

		return poppedUI;
	}

	/// <summary>
	///     Gets a prefab by the passed <see cref="GameMenus" /> type
	///     TODO there are more efficient ways to do this
	/// </summary>
	/// <param name="menuType"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	private MenuBase GetMenuPrefabFromType(GameMenus menuType)
	{
		if (_menuConfiguration == null)
		{
			Debug.LogError("Menu Configuration is not assigned in UIMgr");
			return null;
		}

		var menu = _menuConfiguration.GetPrefab(menuType);

		if (menu == null) Debug.LogError($"Failed to find prefab for {menuType}");

		return menu;
	}
}
