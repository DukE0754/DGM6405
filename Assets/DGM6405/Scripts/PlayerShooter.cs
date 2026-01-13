using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private ProjectileShooter shooter;
    public void Shoot()
    {
        shooter.ShootForward();
    }
}
