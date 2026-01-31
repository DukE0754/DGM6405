/// <summary>
///     Interface for character command brains (player or AI).
///     Defines the contract that all brains must follow for issuing commands to systems.
/// </summary>
public interface ICharacterBrain
{
	/// <summary>
	///     Whether this brain is currently active and processing commands.
	/// </summary>
	bool IsActive { get; }
}
