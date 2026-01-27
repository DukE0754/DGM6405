# Character Systems Integration Guide

This guide provides step-by-step instructions for integrating the modular character systems into your GameObjects.

## Table of Contents

1. [GameObject Hierarchy Setup](#gameobject-hierarchy-setup)
2. [Component Configuration Checklist](#component-configuration-checklist)
3. [Serialized Reference Wiring](#serialized-reference-wiring)
4. [System Dependencies Map](#system-dependencies-map)
5. [Testing Checklist](#testing-checklist)
6. [Example Prefab Setup](#example-prefab-setup)
7. [Troubleshooting](#troubleshooting)

## GameObject Hierarchy Setup

### Recommended Player Character Hierarchy

```
PlayerRoot (GameObject)
├── CharacterController (Component)
├── CharacterContext (Component)
├── CharacterMovementSystem (Component)
├── JumpGravitySystem (Component)
├── CameraRotationSystem (Component)
├── CharacterAnimationSystem (Component)
├── CharacterSoundSystem (Component)
├── BlockSystem (Component)
├── ShootSystem (Component)
├── MeleeSystem (Component)
├── AimSystem (Component)
├── PlayerCommandBrain (Component)
├── PlayerInputHandler (Component)
├── WeaponHandSlots (Component)
├── ProjectileShooter (Component) [if using shooting]
└── Model (GameObject)
    ├── Animator (Component)
    ├── [Character Mesh Renderer]
    └── Rig_Medium (or your rig)
        └── [Bone Hierarchy]
            ├── root
            │   └── hips
            │       └── spine
            │           └── chest
            │               ├── upperarm.l
            │               │   └── lowerarm.l
            │               │       └── wrist.l
            │               │           └── hand.l
            │               │               └── handslot.l (Weapon Slot GameObject)
            │               └── upperarm.r
            │                   └── lowerarm.r
            │                       └── wrist.r
            │                           └── hand.r
            │                               └── handslot.r (Weapon Slot GameObject)
            └── [Other bones...]
```

### Component Placement Guidelines

- **Root GameObject (PlayerRoot)**: Core systems that need direct access to CharacterController
  - `CharacterContext`
  - `CharacterMovementSystem`
  - `JumpGravitySystem`
  - `PlayerCommandBrain` (or `EnemyCommandBrain` for AI)
  - `WeaponHandSlots`

- **Model GameObject**: Animation and visual systems
  - `CharacterAnimationSystem`
  - `Animator` component
  - `AimSystem` (can be on root or model)

- **Combat Systems**: Can be on root or model
  - `BlockSystem`
  - `ShootSystem`
  - `MeleeSystem`
  - `CharacterSoundSystem`

- **Camera System**: Usually on root
  - `CameraRotationSystem`

## Component Configuration Checklist

### CharacterContext

**Required Components:**
- `CharacterController` (must be on same GameObject or assigned)

**Required Serialized Fields:**
- `_controller` - CharacterController reference
- `_animator` - Animator reference (optional but recommended)
- `_cameraTarget` - Cinemachine camera target Transform
- `_weaponHandSlots` - WeaponHandSlots component reference

**Optional Serialized Fields:**
- `_footstepAudioClips` - Array of footstep audio clips
- `_landingAudioClip` - Landing sound clip
- `_footstepAudioVolume` - Volume for footstep/landing sounds (default: 0.5)

**Default Values:**
- `_footstepAudioVolume`: 0.5

### WeaponHandSlots

**Required Serialized Fields:**
- `_slots` - Array of weapon slot GameObjects
  - Index 0: Shield
  - Index 1: Melee weapon
  - Index 2: Ranged weapon (bow, etc.)

**Setup:**
1. Create empty GameObjects as children of hand bones (e.g., `handslot.l`, `handslot.r`)
2. Assign these GameObjects to the `_slots` array in order
3. Attach weapon prefabs as children of these slot GameObjects

### CharacterAnimationSystem

**Required Components:**
- `Animator` (on same GameObject or via CharacterContext)

**Required Serialized Fields:**
- `_animator` - Animator reference (or `_context` with animator)

**Optional Serialized Fields:**
- `_context` - CharacterContext reference

### CharacterMovementSystem

**Required Components:**
- `CharacterController` (via CharacterContext or direct reference)

**Required Serialized Fields:**
- `_controller` - CharacterController reference (or `_context`)

**Optional Serialized Fields:**
- `_context` - CharacterContext reference
- `_mainCamera` - Main camera Transform (auto-finds Camera.main if null)
- `_animationSystem` - CharacterAnimationSystem reference
- `_jumpGravitySystem` - JumpGravitySystem reference

**Configurable Parameters:**
- `_moveSpeed`: 2.0 m/s (default)
- `_sprintSpeed`: 5.335 m/s (default, must be >= moveSpeed)
- `_rotationSmoothTime`: 0.12 (default, range 0-0.3)
- `_speedChangeRate`: 10.0 (default)

### JumpGravitySystem

**Required Components:**
- `CharacterController` (via CharacterContext or direct reference)

**Required Serialized Fields:**
- `_controller` - CharacterController reference (or `_context`)

**Optional Serialized Fields:**
- `_context` - CharacterContext reference
- `_animationSystem` - CharacterAnimationSystem reference

**Configurable Parameters:**
- `_jumpHeight`: 1.2 (default)
- `_gravity`: -15.0 (default)
- `_jumpTimeout`: 0.50 seconds (default)
- `_fallTimeout`: 0.15 seconds (default)
- `_groundedOffset`: -0.14 (default)
- `_groundedRadius`: 0.28 (default, should match CharacterController radius)
- `_groundLayers` - LayerMask for ground detection

### CameraRotationSystem

**Required Serialized Fields:**
- `_cameraTarget` - Cinemachine camera target Transform (or via `_context`)

**Optional Serialized Fields:**
- `_context` - CharacterContext reference

**Configurable Parameters:**
- `_topClamp`: 70.0 degrees (default)
- `_bottomClamp`: -30.0 degrees (default)
- `_cameraAngleOverride`: 0.0 (default)
- `_lockCameraPosition`: false (default)

### CharacterSoundSystem

**Optional Serialized Fields:**
- `_footstepAudioClips` - Array of footstep audio clips
- `_landingAudioClip` - Landing sound clip
- `_blockAudioClip` - Block sound clip
- `_shootAudioClip` - Shoot sound clip
- `_meleeAudioClip` - Melee attack sound clip
- `_dodgeAudioClip` - Dodge sound clip
- `_footstepAudioVolume`: 0.5 (default, range 0-1)
- `_combatAudioVolume`: 0.7 (default, range 0-1)
- `_context` - CharacterContext reference
- `_controller` - CharacterController reference (for sound position)

**Animation Event Setup:**
1. In your animation clips, add Animation Events
2. Call `CharacterSoundSystem.PlayFootstep()` on footstep events
3. Call `CharacterSoundSystem.PlayLanding()` on landing events

### BlockSystem

**Required Serialized Fields:**
- `_animationSystem` - CharacterAnimationSystem reference

**Optional Serialized Fields:**
- `_context` - CharacterContext reference
- `_weaponHandSlots` - WeaponHandSlots reference (or via `_context`)

**Configurable Parameters:**
- `_blockArcAngle`: 180 degrees (default, for gizmo visualization)

### ShootSystem

**Required Serialized Fields:**
- `_projectileShooter` - ProjectileShooter component reference
- `_animationSystem` - CharacterAnimationSystem reference

**Optional Serialized Fields:**
- `_context` - CharacterContext reference
- `_weaponHandSlots` - WeaponHandSlots reference
- `_aimSystem` - AimSystem reference (recommended for accurate aiming)
- `_soundSystem` - CharacterSoundSystem reference

### MeleeSystem

**Required Serialized Fields:**
- `_animationSystem` - CharacterAnimationSystem reference

**Optional Serialized Fields:**
- `_context` - CharacterContext reference
- `_weaponHandSlots` - WeaponHandSlots reference
- `_soundSystem` - CharacterSoundSystem reference

**Configurable Parameters:**
- `_meleeRange`: 2.0 (default, for gizmo visualization)
- `_meleeArcAngle`: 90 degrees (default, for gizmo visualization)

### AimSystem

**Required Components:**
- `Animator` (for IK, via CharacterContext or direct reference)

**Optional Serialized Fields:**
- `_context` - CharacterContext reference
- `_animator` - Animator reference
- `_headBone` - Head bone Transform (auto-finds if null)
- `_weaponHandBone` - Weapon hand bone Transform (auto-finds if null)
- `_mainCamera` - Main camera Transform (for player, auto-finds Camera.main)
- `_targetTransform` - Target Transform (for AI)

**Configurable Parameters:**
- `_maxAimDistance`: 100.0 (default)
- `_aimSmoothing`: 0.1 (default)
- `_headIKWeight`: 1.0 (default, range 0-1)
- `_handIKWeight`: 1.0 (default, range 0-1)

### PlayerCommandBrain

**Required Serialized Fields:**
- `_inputHandler` - PlayerInputHandler component reference
- `_movementSystem` - CharacterMovementSystem reference
- `_jumpGravitySystem` - JumpGravitySystem reference
- `_cameraRotationSystem` - CameraRotationSystem reference

**Optional Serialized Fields:**
- `_playerInput` - PlayerInput component reference (for control scheme detection)
- `_blockSystem` - BlockSystem reference
- `_shootSystem` - ShootSystem reference
- `_meleeSystem` - MeleeSystem reference
- `_aimSystem` - AimSystem reference

### EnemyCommandBrain

**Optional Serialized Fields:**
- `_movementSystem` - CharacterMovementSystem reference
- `_jumpGravitySystem` - JumpGravitySystem reference
- `_cameraRotationSystem` - CameraRotationSystem reference
- `_blockSystem` - BlockSystem reference
- `_shootSystem` - ShootSystem reference
- `_meleeSystem` - MeleeSystem reference
- `_aimSystem` - AimSystem reference
- `_targetTransform` - Target Transform for AI to track

**Configurable Parameters:**
- `_isActive`: true (default)

## Serialized Reference Wiring

### Step-by-Step Wiring Instructions

1. **Start with CharacterContext**
   - Add `CharacterContext` component to root GameObject
   - Assign `CharacterController` reference (drag from same GameObject)
   - Assign `Animator` reference (drag from Model GameObject)
   - Assign `CinemachineCameraTarget` Transform (drag from scene hierarchy)
   - Assign `WeaponHandSlots` reference (drag from same GameObject)
   - Assign audio clips and set volume

2. **Add WeaponHandSlots**
   - Add `WeaponHandSlots` component to root GameObject
   - Create weapon slot GameObjects as children of hand bones
   - Assign weapon slot GameObjects to `_slots` array in order:
     - Element 0: Shield slot
     - Element 1: Melee weapon slot
     - Element 2: Ranged weapon slot

3. **Add Core Systems**
   - Add `CharacterMovementSystem` to root
     - Assign `_context` reference (drag CharacterContext)
     - Assign `_controller` reference (or leave null to use from context)
     - Assign `_animationSystem` reference (drag CharacterAnimationSystem)
     - Assign `_jumpGravitySystem` reference (drag JumpGravitySystem)
   - Add `JumpGravitySystem` to root
     - Assign `_context` reference
     - Assign `_controller` reference (or leave null)
     - Assign `_animationSystem` reference
   - Add `CameraRotationSystem` to root
     - Assign `_context` reference
     - Assign `_cameraTarget` reference (or leave null to use from context)

4. **Add Animation System**
   - Add `CharacterAnimationSystem` to Model GameObject (or root)
     - Assign `_context` reference (or leave null to find on same GameObject)
     - Assign `_animator` reference (or leave null to use from context)

5. **Add Combat Systems**
   - Add `BlockSystem` to root
     - Assign `_animationSystem` reference
     - Assign `_weaponHandSlots` reference (or leave null to use from context)
   - Add `ShootSystem` to root
     - Assign `_projectileShooter` reference (drag ProjectileShooter component)
     - Assign `_animationSystem` reference
     - Assign `_aimSystem` reference (recommended)
     - Assign `_weaponHandSlots` reference
   - Add `MeleeSystem` to root
     - Assign `_animationSystem` reference
     - Assign `_weaponHandSlots` reference

6. **Add Sound System**
   - Add `CharacterSoundSystem` to root
     - Assign audio clips for footsteps, landing, combat sounds
     - Assign `_context` reference (optional, for audio clips)
     - Assign `_controller` reference (optional, for sound position)

7. **Add Aim System**
   - Add `AimSystem` to root or Model GameObject
     - Assign `_context` reference
     - Assign `_animator` reference (or leave null to use from context)
     - Assign `_headBone` Transform (or leave null to auto-find)
     - Assign `_weaponHandBone` Transform (or leave null to auto-find)
     - Assign `_mainCamera` Transform (for player, or leave null to auto-find)

8. **Add Command Brain**
   - Add `PlayerCommandBrain` to root
     - Assign `_inputHandler` reference (drag PlayerInputHandler)
     - Assign `_movementSystem` reference
     - Assign `_jumpGravitySystem` reference
     - Assign `_cameraRotationSystem` reference
     - Assign combat system references (BlockSystem, ShootSystem, MeleeSystem)
     - Assign `_aimSystem` reference

### Common Mistakes to Avoid

1. **Forgetting to assign CharacterController**
   - Error: "CharacterController is required!"
   - Solution: Add CharacterController component to root GameObject and assign to CharacterContext

2. **Missing Animator reference**
   - Error: "Animator not found"
   - Solution: Add Animator component to Model GameObject and assign to CharacterContext

3. **Weapon slots array not set up**
   - Error: "WeaponHandSlots: _slots array is empty"
   - Solution: Create weapon slot GameObjects and assign to WeaponHandSlots `_slots` array

4. **Missing ProjectileShooter for ShootSystem**
   - Error: "ProjectileShooter is required!"
   - Solution: Add ProjectileShooter component and assign to ShootSystem

5. **Camera target not assigned**
   - Error: "Camera target transform is required!"
   - Solution: Assign CinemachineCameraTarget Transform to CharacterContext or CameraRotationSystem

6. **Circular dependencies**
   - Problem: Systems trying to find each other in Awake()
   - Solution: Use serialized field assignments in Inspector instead of relying on GetComponent

## System Dependencies Map

```
PlayerCommandBrain
├── Requires: PlayerInputHandler
├── Requires: CharacterMovementSystem
├── Requires: JumpGravitySystem
├── Requires: CameraRotationSystem
└── Optional: BlockSystem, ShootSystem, MeleeSystem, AimSystem

CharacterMovementSystem
├── Requires: CharacterController (via CharacterContext or direct)
├── Optional: CharacterAnimationSystem
├── Optional: JumpGravitySystem (for vertical velocity)
└── Optional: Transform mainCamera

JumpGravitySystem
├── Requires: CharacterController (via CharacterContext or direct)
└── Optional: CharacterAnimationSystem

CameraRotationSystem
└── Requires: Transform cameraTarget (via CharacterContext or direct)

CharacterAnimationSystem
└── Requires: Animator (via CharacterContext or direct)

BlockSystem
├── Requires: CharacterAnimationSystem
└── Optional: WeaponHandSlots

ShootSystem
├── Requires: ProjectileShooter
├── Requires: CharacterAnimationSystem
├── Optional: AimSystem (recommended)
├── Optional: WeaponHandSlots
└── Optional: CharacterSoundSystem

MeleeSystem
├── Requires: CharacterAnimationSystem
├── Optional: WeaponHandSlots
└── Optional: CharacterSoundSystem

AimSystem
├── Requires: Animator (for IK)
├── Optional: Transform headBone
├── Optional: Transform weaponHandBone
└── Optional: Transform mainCamera (for player) or targetTransform (for AI)

CharacterSoundSystem
└── Optional: CharacterController (for sound position)

WeaponHandSlots
└── Requires: GameObject[] slots array

CharacterContext
├── Requires: CharacterController
├── Optional: Animator
├── Optional: Transform cameraTarget
├── Optional: WeaponHandSlots
└── Optional: Audio clips
```

### Initialization Order

Systems initialize in this order (based on Unity's execution order):

1. `CharacterContext.Awake()` - Sets up shared references
2. `WeaponHandSlots.Awake()` - Validates weapon slots
3. `CharacterAnimationSystem.Awake()` - Caches animator and parameter IDs
4. `CharacterMovementSystem.Awake()` - Validates controller and finds camera
5. `JumpGravitySystem.Awake()` - Validates controller
6. `CameraRotationSystem.Awake()` - Validates camera target
7. Combat systems (`BlockSystem`, `ShootSystem`, `MeleeSystem`) - Validate dependencies
8. `AimSystem.Awake()` - Finds bones and sets up IK
9. `CharacterSoundSystem.Awake()` - Validates audio setup
10. `PlayerCommandBrain.Awake()` - Validates all system references

**Note**: If systems depend on each other, assign references in Inspector rather than using GetComponent to avoid order issues.

## Testing Checklist

### Basic Functionality Tests

- [ ] **Movement Test**
  - [ ] Character moves forward/backward with W/S keys
  - [ ] Character moves left/right with A/D keys
  - [ ] Character sprints when holding Shift
  - [ ] Character rotates to face movement direction
  - [ ] Movement gizmos show velocity line (green/yellow/red)

- [ ] **Jump Test**
  - [ ] Character jumps when pressing Space
  - [ ] Character falls with gravity when not grounded
  - [ ] Ground check gizmo shows green when grounded, red when not
  - [ ] Vertical velocity gizmo shows blue when jumping, red when falling

- [ ] **Camera Test**
  - [ ] Camera rotates horizontally with mouse X movement
  - [ ] Camera rotates vertically with mouse Y movement
  - [ ] Camera clamps at top/bottom limits
  - [ ] Camera gizmos show look direction line

- [ ] **Animation Test**
  - [ ] Movement animations play when moving
  - [ ] Jump animation plays when jumping
  - [ ] Fall animation plays when falling
  - [ ] Animation parameters update correctly in Animator window

- [ ] **Combat Test**
  - [ ] Block animation plays when blocking
  - [ ] Shield weapon slot activates when blocking
  - [ ] Shoot animation plays when shooting
  - [ ] Ranged weapon slot activates when shooting
  - [ ] Projectile spawns and moves correctly
  - [ ] Melee animation plays when melee attacking
  - [ ] Melee weapon slot activates when attacking
  - [ ] Combat gizmos show block arc, aim point, melee arc

- [ ] **Sound Test**
  - [ ] Footstep sounds play during movement
  - [ ] Landing sound plays when landing
  - [ ] Combat sounds play (block, shoot, melee)

- [ ] **Aim Test**
  - [ ] Aim point updates based on camera direction
  - [ ] Head IK points toward aim target
  - [ ] Projectiles shoot toward aim point (not just forward)
  - [ ] Aim gizmos show aim line and target sphere

### Game State Tests

- [ ] **Pause Test**
  - [ ] Press Escape to pause
  - [ ] All movement stops
  - [ ] All animations freeze
  - [ ] Input is cleared
  - [ ] Press Escape again to unpause
  - [ ] Systems resume correctly

- [ ] **Game Over Test**
  - [ ] Trigger game over (death, time limit, etc.)
  - [ ] All systems stop processing
  - [ ] Character stops moving/attacking
  - [ ] Return to menu works correctly

- [ ] **Level Complete Test**
  - [ ] Complete level
  - [ ] Systems stop processing
  - [ ] Load next level works correctly

### Inspector Validation Tests

- [ ] Check all serialized fields are assigned (no null references)
- [ ] Check all required components exist
- [ ] Check parameter values are in valid ranges
- [ ] Check weapon slots array is populated
- [ ] Check audio clips are assigned
- [ ] Check camera target is assigned

### Error Message Tests

- [ ] Missing CharacterController shows error in console
- [ ] Missing Animator shows warning in console
- [ ] Missing ProjectileShooter shows error in console
- [ ] Missing required system references show warnings

## Example Prefab Setup

### Complete Player Prefab Configuration

1. **Create PlayerRoot GameObject**
   - Add `CharacterController` component
     - Center: (0, 1, 0)
     - Radius: 0.28
     - Height: 1.8
   - Add `CharacterContext` component
     - Assign CharacterController reference
     - Assign Animator reference (from Model)
     - Assign CinemachineCameraTarget Transform
     - Assign WeaponHandSlots reference
     - Assign footstep and landing audio clips
   - Add `WeaponHandSlots` component
     - Create 3 empty GameObjects as weapon slots
     - Assign to `_slots` array: [ShieldSlot, MeleeSlot, RangedSlot]
   - Add `CharacterMovementSystem` component
     - Move Speed: 2.0
     - Sprint Speed: 5.335
     - Rotation Smooth Time: 0.12
     - Speed Change Rate: 10.0
     - Assign CharacterContext reference
     - Assign CharacterAnimationSystem reference
     - Assign JumpGravitySystem reference
   - Add `JumpGravitySystem` component
     - Jump Height: 1.2
     - Gravity: -15.0
     - Jump Timeout: 0.50
     - Fall Timeout: 0.15
     - Grounded Offset: -0.14
     - Grounded Radius: 0.28
     - Assign Ground Layers mask
     - Assign CharacterContext reference
     - Assign CharacterAnimationSystem reference
   - Add `CameraRotationSystem` component
     - Top Clamp: 70.0
     - Bottom Clamp: -30.0
     - Assign CharacterContext reference
   - Add `BlockSystem` component
     - Assign CharacterAnimationSystem reference
     - Assign WeaponHandSlots reference
   - Add `ShootSystem` component
     - Assign ProjectileShooter reference
     - Assign CharacterAnimationSystem reference
     - Assign AimSystem reference
     - Assign WeaponHandSlots reference
     - Assign CharacterSoundSystem reference
   - Add `MeleeSystem` component
     - Assign CharacterAnimationSystem reference
     - Assign WeaponHandSlots reference
     - Assign CharacterSoundSystem reference
   - Add `CharacterSoundSystem` component
     - Assign footstep audio clips array
     - Assign landing audio clip
     - Assign combat audio clips (block, shoot, melee)
     - Footstep Volume: 0.5
     - Combat Volume: 0.7
   - Add `AimSystem` component
     - Max Aim Distance: 100.0
     - Aim Smoothing: 0.1
     - Head IK Weight: 1.0
     - Hand IK Weight: 1.0
     - Assign CharacterContext reference
     - Assign head bone Transform (or leave null to auto-find)
     - Assign weapon hand bone Transform (or leave null to auto-find)
   - Add `PlayerCommandBrain` component
     - Assign PlayerInputHandler reference
     - Assign CharacterMovementSystem reference
     - Assign JumpGravitySystem reference
     - Assign CameraRotationSystem reference
     - Assign BlockSystem reference
     - Assign ShootSystem reference
     - Assign MeleeSystem reference
     - Assign AimSystem reference
   - Add `PlayerInputHandler` component
   - Add `ProjectileShooter` component
     - Assign projectile prefab
     - Assign fire point Transform

2. **Create Model GameObject (child of PlayerRoot)**
   - Add `Animator` component
     - Assign Animator Controller
     - Avatar: Humanoid (or your avatar)
   - Add `CharacterAnimationSystem` component
     - Assign CharacterContext reference (from parent)
     - Assign Animator reference
   - Add character mesh/model as child
   - Add rig hierarchy as child

3. **Create Weapon Slot GameObjects**
   - Create `ShieldSlot` GameObject (child of left hand bone)
   - Create `MeleeSlot` GameObject (child of right hand bone)
   - Create `RangedSlot` GameObject (child of right hand bone)
   - Assign weapon prefabs as children of these slots

## Troubleshooting

### Common Issues and Solutions

#### Issue: "CharacterController is required!" error

**Symptoms:**
- Console error on startup
- Character cannot move

**Solutions:**
1. Add `CharacterController` component to root GameObject
2. Assign CharacterController reference to CharacterContext `_controller` field
3. Verify CharacterController is not null in Inspector

#### Issue: Character doesn't move

**Symptoms:**
- No movement when pressing WASD
- No errors in console

**Solutions:**
1. Check `PlayerCommandBrain` is assigned and enabled
2. Check `PlayerInputHandler` is assigned to PlayerCommandBrain
3. Check `CharacterMovementSystem` is assigned to PlayerCommandBrain
4. Check `GameMgr.Instance.IsGameRunning` is true
5. Verify input actions are set up in Input System
6. Check movement gizmos to see if velocity is being calculated

#### Issue: Character doesn't jump

**Symptoms:**
- No jump when pressing Space
- Character falls but doesn't jump

**Solutions:**
1. Check `JumpGravitySystem` is assigned to PlayerCommandBrain
2. Check `JumpGravitySystem` is assigned to CharacterMovementSystem
3. Verify ground check is working (check gizmo color)
4. Check jump timeout is not blocking jumps
5. Verify `_groundLayers` mask includes ground layers

#### Issue: Camera doesn't rotate

**Symptoms:**
- Mouse movement doesn't rotate camera
- Camera stays fixed

**Solutions:**
1. Check `CameraRotationSystem` is assigned to PlayerCommandBrain
2. Check `_cameraTarget` Transform is assigned to CameraRotationSystem
3. Verify CinemachineCameraTarget exists in scene
4. Check `_lockCameraPosition` is false
5. Verify look input is being received (check PlayerInputHandler)

#### Issue: Animations don't play

**Symptoms:**
- Character moves but no animations
- Animator parameters don't update

**Solutions:**
1. Check `CharacterAnimationSystem` is assigned
2. Check `Animator` component exists and is assigned
3. Verify Animator Controller is assigned to Animator
4. Check animation parameter names match (Speed, Grounded, Jump, etc.)
5. Verify CharacterAnimationSystem is being called by other systems

#### Issue: Weapons don't show/hide

**Symptoms:**
- Weapon slots don't activate during combat
- Weapons always visible or never visible

**Solutions:**
1. Check `WeaponHandSlots` component exists
2. Verify `_slots` array is populated with weapon GameObjects
3. Check weapon slot GameObjects are children of hand bones
4. Verify combat systems (BlockSystem, ShootSystem, MeleeSystem) are calling `SetActiveSlot()`
5. Check weapon slot GameObjects are not disabled elsewhere

#### Issue: Shooting doesn't work

**Symptoms:**
- Shoot animation plays but no projectile spawns
- Projectile spawns in wrong direction

**Solutions:**
1. Check `ProjectileShooter` component exists and is assigned to ShootSystem
2. Verify projectile prefab is assigned to ProjectileShooter
3. Check `AimSystem` is assigned to ShootSystem (for accurate aiming)
4. Verify `AimSystem.GetAimPoint()` returns correct position
5. Check ProjectileShooter fire point Transform is assigned

#### Issue: IK doesn't work (head/hands don't aim)

**Symptoms:**
- Head doesn't look at target
- Weapon hand doesn't point at aim point

**Solutions:**
1. Check `Animator` has IK Pass enabled on animation layers
2. Verify `AimSystem` has `_headBone` and `_weaponHandBone` assigned
3. Check bone names match your rig hierarchy
4. Verify `_headIKWeight` and `_handIKWeight` are > 0
5. Check `OnAnimatorIK` callback is being called (add debug logs)

#### Issue: Sounds don't play

**Symptoms:**
- No footstep or combat sounds

**Solutions:**
1. Check `CharacterSoundSystem` component exists
2. Verify audio clips are assigned
3. Check audio clips are not null
4. Verify animation events are calling sound methods
5. Check `GameMgr.Instance.IsGameRunning` is true (sounds won't play if game paused)

#### Issue: Systems don't pause correctly

**Symptoms:**
- Character still moves when game is paused
- Systems continue processing during pause

**Solutions:**
1. Verify all systems inherit from `PausableBehaviour`
2. Check `OnPaused()` methods are clearing state
3. Verify `GameMgr.Instance.PauseStateChanged` event is firing
4. Check systems are checking `GameMgr.Instance.IsGameRunning`

#### Issue: Null reference exceptions

**Symptoms:**
- Console errors about null references
- Systems fail to initialize

**Solutions:**
1. Check all required serialized fields are assigned in Inspector
2. Verify components exist on GameObjects
3. Check system initialization order (use serialized fields, not GetComponent)
4. Review error messages for specific missing references
5. Use `OnValidate()` to catch missing references in Editor

### Debugging Tips

1. **Enable Gizmos**: Select character GameObject and check gizmo visualizations
   - Movement: Velocity lines, rotation indicators
   - Jump: Ground check sphere, vertical velocity
   - Camera: Look direction, rotation limits
   - Combat: Block arc, aim point, melee arc

2. **Check Console**: Look for error/warning messages with component names
   - All systems log errors with `[{name}]` prefix for easy identification

3. **Use Inspector**: Verify all serialized fields are assigned
   - Null references will show warnings in `OnValidate()`

4. **Test Systems Individually**: Disable other systems to isolate issues
   - Systems are designed to work independently

5. **Check Game State**: Verify `GameMgr.Instance.IsGameRunning` is true
   - Systems won't process if game is not running

6. **Animation Events**: Verify animation events are set up correctly
   - Check event function names match exactly
   - Verify events are on correct animation clips

### Getting Help

If you encounter issues not covered in this guide:

1. Check console for specific error messages
2. Review system code comments for usage notes
3. Verify all dependencies are set up correctly
4. Test with minimal setup (one system at a time)
5. Check that Unity 6 compatibility is maintained
6. Verify Input System is set up correctly

---

**Last Updated**: [Current Date]
**Version**: 1.0
