/// <summary>
///     Interface for objects that can receive damage.
///     This acts as the bridge between a hit
///     (collision/raycast) and the health system.
/// </summary>
public interface IDamageReceiver
{
	void ApplyDamage(DamageInfo info);
}
