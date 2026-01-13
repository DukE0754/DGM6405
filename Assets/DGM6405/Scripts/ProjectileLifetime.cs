using UnityEngine;

public class ProjectileLifetime : MonoBehaviour
{
    [SerializeField] private float lifeTimeSeconds = 3f;

    private void OnEnable()
    {
        if (lifeTimeSeconds <= 0f) lifeTimeSeconds = 0.1f;
        Destroy(gameObject, lifeTimeSeconds);
    }
}

