using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileData", menuName = "DGM6405/Weapons/ProjectileData")]
public class ProjectileData : ScriptableObject
{
	[SerializeField] private int _damage = 10;
	[SerializeField] private float _speed = 20f;
	[SerializeField] private float _lifetime = 5f;
	[SerializeField] private bool _useGravity;

	public int Damage => _damage;
	public float Speed => _speed;
	public float Lifetime => _lifetime;
	public bool UseGravity => _useGravity;
}
