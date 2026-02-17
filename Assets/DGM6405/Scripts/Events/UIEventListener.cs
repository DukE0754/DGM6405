using System;
using UnityEngine;

/// <summary>
///     Bridges GlobalEventBus events to UIMgr.
///     Decouples other systems from direct UIMgr.Instance calls.
/// </summary>
public class UIEventListener : MonoBehaviour, IUIEventListener
{
	private void OnEnable()
	{
		GlobalEventBus.Instance.Register<IUIEventListener>(this);
	}

	private void OnDisable()
	{
		GlobalEventBus.Instance.Unregister<IUIEventListener>(this);
	}

	public void OnShowMenu(GameMenus menu, Action onComplete, bool fadeIn)
	{
		UIMgr.Instance.ShowMenu(menu, onComplete, fadeIn);
	}

	public void OnCloseAllMenus()
	{
		UIMgr.Instance.CloseAllMenus();
	}

	public void OnHideMenu(GameMenus menu)
	{
		UIMgr.Instance.HideMenu(menu);
	}
}
