using System;
using UnityEngine;

#region Global Listeners

public interface IGameStateListener
{
	void OnPauseStateChanged(bool isPaused)
	{
	}

	void OnGameStarted()
	{
	}

	void OnGameOver()
	{
	}

	void OnNextLevel(int timeMs)
	{
	}
}

public interface IUIEventListener
{
	void OnShowMenu(GameMenus menu, Action onComplete = null, bool fadeIn = true)
	{
	}

	void OnCloseAllMenus()
	{
	}

	void OnHideMenu(GameMenus menu)
	{
	}
}

public interface ISceneEventListener
{
	void OnLoadScene(GameScenes scene, GameMenus menu, Action onComplete = null)
	{
	}
}

public interface IAudioEventListener
{
	void OnPlaySound(AudioMgr.SoundTypes sound)
	{
	}

	void OnPlaySoundClip(AudioClip clip)
	{
	}

	void OnSetMasterVolume(float volume, bool save = true)
	{
	}

	void OnSetMusicVolume(float volume, bool save = true)
	{
	}

	void OnSetSfxVolume(float volume, bool save = true)
	{
	}
}

public interface ILevelListener
{
	void OnLevelReady()
	{
	}

	void OnLevelComplete(int timeMs)
	{
	}
}

public interface ILevelSpawnListener
{
	void OnSpawnPointReady(LevelSpawnPoint spawnPoint)
	{
	}
}

public interface IPlayerGlobalListener
{
	void OnPlayerSpawned(GameObject player)
	{
	}

	void OnPlayerDespawned()
	{
	}
}

#endregion

#region Local Entity Listeners

public interface IMovementListener : IEntityListener
{
	void OnMove(Vector2 moveInput, bool isSprinting)
	{
	}
}

public interface ILookListener : IEntityListener
{
	void OnLook(Vector2 lookInput, bool isMouse)
	{
	}
}

public interface IRotationListener : IEntityListener
{
	void OnRotate(Vector3 direction)
	{
	}

	/// <summary>
	///     Sets whether the character should automatically rotate to face its movement direction.
	/// </summary>
	void SetRotateToMovement(bool enable)
	{
	}

	/// <summary>
	///     Sets whether the character should automatically rotate to face the camera forward.
	/// </summary>
	void SetRotateToCamera(bool enable)
	{
	}
}

public interface IJumpListener : IEntityListener
{
	void OnJump(bool jumpInput)
	{
	}

	void OnJumpPerformed()
	{
	}
}

public interface IShootListener : IEntityListener
{
	void OnShoot(bool shootInput)
	{
	}
}

public interface IFireProjectileListener : IEntityListener
{
	void OnFireProjectile()
	{
	}
}

public interface IMeleeListener : IEntityListener
{
	void OnMelee(bool meleeInput)
	{
	}
}

public interface IBlockListener : IEntityListener
{
	void OnBlock(bool blockInput)
	{
	}
}

public interface IBlockHitListener : IEntityListener
{
	void OnBlockHit(Vector3 hitPoint, Vector3 hitNormal, GameObject source)
	{
	}
}

public interface IAimListener : IEntityListener
{
	void OnAimUpdate(Vector3 aimPoint, bool isTargeting)
	{
	}
}

public interface IAimTargetListener : IEntityListener
{
	void OnSetAimTarget(Vector3 worldPosition)
	{
	}
}

public interface IHealthListener : IEntityListener
{
	void OnHealthChanged(float current, float max)
	{
	}

	void OnDamageTaken(int amount, Vector3 direction)
	{
	}

	void OnDied()
	{
	}
}

public interface IGroundListener : IEntityListener
{
	void OnGroundedChanged(bool isGrounded)
	{
	}

	void OnFall()
	{
	}
}

public interface IMovementSpeedListener : IEntityListener
{
	void OnSpeedChanged(
		float speed, float animationBlend, float walkSpeed, float sprintSpeed, float velocityX, float velocityZ)
	{
	}
}

public interface IWeaponSlotListener : IEntityListener
{
	void OnWeaponSlotChanged(WeaponHandSlots.WeaponSlotType slotType)
	{
	}
}

public interface IWaterVolumeListener : IEntityListener
{
	void OnEnteredWater(float surfaceHeight)
	{
	}

	void OnExitedWater()
	{
	}
}

#endregion
