using UnityEngine;

/// <summary>
///     Singular source of truth for global game state and shared references.
///     Acts as a global counterpart to CharacterContext.
/// </summary>
public class GlobalContext : Singleton<GlobalContext>
{
	[Header("Core Systems")]
	[SerializeField] private GameMgr _gameMgr;

	[SerializeField] private UIMgr _uiMgr;
	[SerializeField] private SceneMgr _sceneMgr;
	[SerializeField] private LevelMgr _levelMgr;
	[SerializeField] private AudioMgr _audioMgr;
	[SerializeField] private InputMgr _inputMgr;
	[SerializeField] private CameraMgr _cameraMgr;
	[SerializeField] private PlayerMgr _playerMgr;
	[SerializeField] private ColliderMgr _colliderMgr;

	public GameMgr GameMgr => _gameMgr;
	public UIMgr UIMgr => _uiMgr;
	public SceneMgr SceneMgr => _sceneMgr;
	public LevelMgr LevelMgr => _levelMgr;
	public AudioMgr AudioMgr => _audioMgr;
	public InputMgr InputMgr => _inputMgr;
	public CameraMgr CameraMgr => _cameraMgr;
	public PlayerMgr PlayerMgr => _playerMgr;
	public ColliderMgr ColliderMgr => _colliderMgr;

	public override void Awake()
	{
		base.Awake();

		// Auto-find managers if not assigned
		if (_gameMgr == null) _gameMgr = FindFirstObjectByType<GameMgr>();
		if (_uiMgr == null) _uiMgr = FindFirstObjectByType<UIMgr>();
		if (_sceneMgr == null) _sceneMgr = FindFirstObjectByType<SceneMgr>();
		if (_levelMgr == null) _levelMgr = FindFirstObjectByType<LevelMgr>();
		if (_audioMgr == null) _audioMgr = FindFirstObjectByType<AudioMgr>();
		if (_inputMgr == null) _inputMgr = FindFirstObjectByType<InputMgr>();
		if (_cameraMgr == null) _cameraMgr = FindFirstObjectByType<CameraMgr>();
		if (_playerMgr == null) _playerMgr = FindFirstObjectByType<PlayerMgr>();
		if (_colliderMgr == null) _colliderMgr = FindFirstObjectByType<ColliderMgr>();
	}
}
