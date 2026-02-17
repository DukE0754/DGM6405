using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "MenuConfiguration", menuName = "UI/Menu Configuration")]
public class MenuConfiguration : ScriptableObject
{
	[AssetSelector(Paths = "Assets/DGM6405/Prefabs/UI")]
	[AssetsOnly]
	[Header("Prefabs must have a component inheriting from MenuBase")]
	[SerializeField] private MenuBase[] _menuPrefabs;

	private Dictionary<GameMenus, MenuBase> _prefabLookup;

	public MenuBase[] MenuPrefabs => _menuPrefabs;

	private void OnValidate()
	{
		// Optional: Clear lookup to force re-initialization if needed at runtime/editor
		_prefabLookup = null;
	}

	public MenuBase GetPrefab(GameMenus menuType)
	{
		if (_prefabLookup == null) InitializeLookup();
		return _prefabLookup?.GetValueOrDefault(menuType);
	}

	private void InitializeLookup()
	{
		_prefabLookup = new Dictionary<GameMenus, MenuBase>();
		foreach (var menuBase in _menuPrefabs)
		{
			if (menuBase == null) continue;

			var type = menuBase.MenuType();
			if (type == GameMenus.None) continue;

			if (!_prefabLookup.TryAdd(type, menuBase))
				Debug.LogWarning($"Duplicate menu prefab for {type} in {name}", menuBase);
		}
	}
}
