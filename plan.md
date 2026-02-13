# Implementation Plan - Movement and Rotation Decoupling

## 1. Fix Input Reflection Conflicts
- [x] Use explicit interface implementation for input-related methods to avoid `PlayerInput` reflection issues.
    - Affected systems: `CameraRotationSystem`, `CharacterMovementSystem`, `BlockSystem`, `CharacterAnimationSystem`, `CharacterSoundSystem`.
    - Methods hidden from reflection: `OnLook`, `OnMove`, `OnBlock`, `OnMelee`, `OnShoot`, `OnJump`.
- [x] Ensure `PlayerInputHandler` is the primary receiver of `PlayerInput` messages.

## 2. Decouple Character Rotation
- [x] Define `IRotationListener` in `Listeners.cs`.
- [x] Remove rotation logic from `CharacterMovementSystem.cs`.
- [x] Create `RotateWithMoveDirectionSystem.cs`.
- [x] Create `RotateToFaceCameraForwardSystem.cs`.

## 3. Block System Integration
- [x] Update `BlockSystem.cs` to toggle rotation modes.

## 4. AI / Enemy Brain Refactor
- [x] Update `EnemyCommandBrain.cs` to use the new rotation systems.
- [x] Support different AI rotation behaviors via `IRotationListener`.

## 5. Event Bus Centralization
- [x] Add `LocalEventBus` reference to `CharacterContext.cs`.
- [x] Refactor systems (`AimSystem`, `BlockSystem`, `CharacterMovementSystem`, `EnemyCommandBrain`, `PlayerCommandBrain`) to use `LocalEventBus` from `CharacterContext`.
- [x] Remove redundant `LocalEventBus` fields in individual systems to reduce `GetComponent` calls.

## 6. Verification
- [x] Verify no `PlayerInput` reflection errors (by using explicit interface implementation).
- [x] Verify Player movement and rotation decoupling (logic separated into systems).
- [x] Verify Enemy rotation behaviors (AI now raises `IRotationListener.OnRotate`).
- [x] Verify all systems correctly access the centralized event bus via `CharacterContext`.
