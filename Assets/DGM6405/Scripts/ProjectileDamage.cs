using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private bool destroyOnHit = true;

    [Header("Target")]
    [SerializeField] private LayerMask damageLayers;

    private void OnCollisionEnter(Collision collision)
    {
        // Check layer
        if ((damageLayers.value & (1 << collision.gameObject.layer)) == 0)
            return;

        // Try get CombatStats
        CombatStats stats = collision.gameObject.GetComponentInParent<CombatStats>();
        if (stats != null)
        {
            stats.TakeDamage(damage, gameObject);
        }

        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
}

