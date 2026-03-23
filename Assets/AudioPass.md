# Audio Pass Plan

## Instructions for Agent
For EACH audio item:
1. Investigate the live gameplay flow and confirm the mechanic is actually used in the prototype
2. Identify whether audio should be driven by:
   - Global UI/audio events
   - Local entity events
   - Animation events for timing-critical actions
3. Mark current status:
   - `VERIFIED`
   - `MISSING`
   - `NOT IN PROTOTYPE`
   - `NEEDS TIMING REVIEW`
4. Summarize findings and the correct hookup point
5. Ask for approval BEFORE making changes
6. Implement only after approval
7. Verify in-game and update this file

Do NOT add sounds for unused prototype mechanics just because assets or animations exist.

---

## Audio Architecture Notes

### Global UI / Menu Audio
- `ButtonSounds.cs` raises `IAudioEventListener` events on `GlobalEventBus`
- `AudioEventListener.cs` bridges those global events into `AudioMgr`
- `AudioMgr.cs` is the central non-3D audio manager for:
  - menu/UI SFX
  - reusable one-shot global SFX
  - music playback
- Current confirmed global audio usage:
  - button hover
  - button click
  - button error

### Local Character / Enemy Audio
- Character gameplay is built around `LocalEventBus` on each entity via `CharacterContext`
- `PlayerCommandBrain.cs` raises local command events like:
  - `IBlockListener`
  - `IShootListener`
  - `IMeleeListener`
  - `IWeaponSlotListener`
- `BudBrain.cs` also raises local combat events for its own entity
- `CharacterAnimationSystem.cs` and `EnemyAnimatorDriver.cs` consume those local events to drive animator state

### Timing Rule For Gameplay SFX
- Prefer audio at animation keyframes for timing-critical actions
- Do not default to playing sounds directly from command/input scripts if the action has an animation
- Player already follows this pattern for footsteps through `CharacterSoundSystem.PlayFootstep(AnimationEvent)`
- Enemy projectile timing already follows this pattern:
  - `EnemyAnimatorDriver` triggers attack animation from `IShootListener`
  - `AnimatorEventDispatcher` raises `IFireProjectileListener`
  - `ProjectileWeapon` fires in `LateUpdate` after the animation event
- Bud enemy currently has animation event infrastructure, but no dedicated enemy sound system yet

### Important Current Gaps
- `CharacterSoundSystem.cs` contains block / shoot / melee / dodge clip fields, but its local event reactions are currently commented out
- `ShieldProjectileBlocker.cs` raises `IBlockHitListener` on shield projectile collisions
- `CharacterAnimationSystem.cs` responds to `IBlockHitListener` for visuals, but there is no confirmed sound listener for shield impact yet
- No current gameplay-facing bridge exists for 3D positional combat audio other than direct `AudioSource.PlayClipAtPoint`
- No confirmed music hook-up was found calling `AudioMgr.PlayMusic(...)` from scene/game state flow yet

### Asset Wiring Findings
- `RangerV3.prefab` has `CharacterSoundSystem`, `CharacterAnimationSystem`, and `BlockSystem` on the player root
- `RangerV3.prefab` already has clips assigned for:
  - footsteps
  - landing
  - block
  - shoot
  - melee
  - dodge
- Player locomotion and landing animation clips contain animation events for:
  - `PlayFootstep`
  - `PlayLanding`
- `EnemyBud Variant.prefab` contains `AnimatorEventDispatcher`
- Bud attack animation clips contain `FireProjectile` animation events
- `AudioMgr.prefab` has reusable clips assigned for:
  - menu music
  - gameplay music
  - button select / hover / error SFX
- `ButtonSounds` was found wired on the shared button prefab and `SettingsUI.prefab`
- No asset or script evidence was found yet that menu flow or game-state flow actually calls `AudioMgr.PlayMusic(...)`
- The repo currently contains audio files for:
  - menu music
  - gameplay music
  - button select / hover / error
  - player footsteps
  - player landing
- No dedicated audio files were found for:
  - shield raise / lower
  - shield block impact
  - Bud attack / hit / death
  - projectile launch / impact
- `RangerV3.prefab` combat clip assignments currently point to reused clips from:
  - UI select
  - UI hover
  - UI error
  - one footstep clip

---

## Current Baseline Findings

### Confirmed Present
- Menus:
  - shared UI button hover path exists
  - shared UI button click path exists
  - shared UI button error path exists via `AudioMgr`
- Player:
  - footsteps
  - landing animation event path exists
- Bud Enemy:
  - attack animation timing path exists for projectile spawn

### Confirmed Or Likely Missing
- Menus:
  - scene/state-based music hookup is currently missing in code
  - pause/menu transition audio not yet confirmed
- Player:
  - block activated
  - block deactivated
  - shield absorbed a hit
- Bud Enemy:
  - no enemy-specific sound system found yet
  - attack/fire sound missing
  - hit/death sounds not yet confirmed
- Projectiles:
  - launch sound missing for current projectile flow
  - impact sound missing in projectile collision flow

### Out Of Scope Unless Verified In Prototype
- Player melee audio
- Player shooting audio
- Any extra combat actions that exist only as unused assets or unused animations

### Current Blockers Requiring User Input
- If you want unique combat audio, you need to provide or approve source clips for:
  - block activated
  - block deactivated
  - shield absorbed hit
  - Bud attack/fire
  - Bud hit
  - Bud death
  - projectile launch
  - projectile world/player impact
- Without new assets, the only implementation options are:
  - leave those items missing
  - reuse existing UI sounds as placeholders
  - reuse footstep/landing clips, which is likely not acceptable for combat feedback

---

## Priority 1 - Menus & Music

### Task 1 - Menu SFX Validation
**What to validate**
- Main menu button hover/click coverage
- Settings menu button/slider/toggle coverage
- Pause menu button coverage
- Error/invalid action feedback where applicable

**Files / Systems to Investigate**
- `Assets/InstructorFiles/Scripts/UI/ButtonSounds.cs`
- menu prefabs and menu scenes
- any UI scripts bypassing `ButtonSounds`

**Investigation Goals**
- confirm every interactive menu element has expected hover/click behavior
- identify controls missing `ButtonSounds` or equivalent wiring

**Current Status**
- `PARTIALLY VERIFIED`

**Current Findings**
- `ButtonSounds` is wired on the shared button prefab and in `SettingsUI.prefab`
- this confirms the global hover/click path is in active use for at least shared/settings UI
- main menu, pause menu, and any bespoke interactive controls still need scene/prefab validation

### Task 2 - Music Flow Validation
**What to validate**
- main menu music
- in-game music
- pause behavior for music
- game over / level complete music behavior if intended

**Files / Systems to Investigate**
- `Assets/InstructorFiles/Scripts/Managers/AudioMgr.cs`
- `Assets/InstructorFiles/Scripts/Managers/GameMgr.cs`
- scene/menu loading flow

**Investigation Goals**
- find where `AudioMgr.PlayMusic(...)` is actually called
- if no hookup exists, document that clearly before adding anything

**Current Status**
- `IMPLEMENTED, NEEDS IN-GAME VERIFICATION`

**Current Findings**
- no scripts under `Assets` call `AudioMgr.PlayMusic(...)`, `PauseMusic()`, `ResumeMusic()`, or `PlayOneShotMusic(...)`
- `AudioMgr.prefab` has menu and gameplay music clips assigned, but they are currently just data with no scene/game-state caller
- if music is playing anywhere, it is not being started by the current codebase
- implemented music switching in `SceneMgr` for `MainMenu`, `GameOver`, and `Gameplay`
- implemented pause/resume music control in `GameMgr.SetPaused(...)`

---

## Priority 2 - Player Audio

### Task 3 - Movement Audio Validation
**What to validate**
- footsteps on active locomotion animations
- landing timing and frequency

**Files / Systems to Investigate**
- `Assets/DGM6405/Scripts/Character/CharacterSoundSystem.cs`
- player animator clips / animation events

**Investigation Goals**
- confirm footsteps are animation-event driven
- confirm landing is not double-firing or firing on micro ground touches
- decide whether landing should remain event/state driven or move to animation timing

**Current Status**
- `VERIFIED IN ASSETS, NEEDS IN-GAME CHECK`

**Current Findings**
- player movement clips contain `PlayFootstep` animation events
- player landing clip contains `PlayLanding` animation event
- `RangerV3.prefab` has footstep and landing clips assigned on `CharacterSoundSystem`
- landing currently also triggers from `IGroundListener.OnGroundedChanged(true)`, so timing/double-fire still needs play verification

### Task 4 - Shield Raise / Lower Audio
**What to validate**
- block activated
- block deactivated

**Files / Systems to Investigate**
- `Assets/DGM6405/Scripts/Character/PlayerCommandBrain.cs`
- `Assets/DGM6405/Scripts/Character/BlockSystem.cs`
- `Assets/DGM6405/Scripts/Character/CharacterAnimationSystem.cs`
- player block animation clips / animation events

**Investigation Goals**
- determine whether shield raise/lower is represented by actual animation timing
- add sound at animation keyframes if the animation exists and is used
- avoid playing these directly from input if that causes mistiming

**Current Status**
- `IMPLEMENTED, NEEDS IN-GAME VERIFICATION`

**Current Findings**
- the player command layer raises `IBlockListener.OnBlock(bool)` on state changes
- `CharacterAnimationSystem` consumes that event to drive the animator `Block` bool
- `WeaponHandSlots` also consumes it indirectly via `IWeaponSlotListener` to show the shield object
- `CharacterSoundSystem` has a block clip assigned on the player prefab, but its `IBlockListener` reaction is commented out
- there are no dedicated raise/lower animation events or obvious separate raise/lower clips in the current player animation folder
- because block is an animated state, raise/lower sounds should be added from animation events if the active controller state has a clean timing window
- there is currently no dedicated shield raise/lower audio asset in the repo
- implemented block start/end playback on `CharacterSoundSystem` as a state-change fallback because no dedicated raise/lower animation events were found
- live player prefab now has `CharacterSoundSystem` configured with the new block start/end clips

### Task 5 - Shield Impact Audio
**What to validate**
- shield absorbed projectile hit

**Files / Systems to Investigate**
- `Assets/DGM6405/Scripts/Character/ShieldProjectileBlocker.cs`
- `Assets/DGM6405/Scripts/Events/Listeners.cs`
- `Assets/DGM6405/Scripts/Character/CharacterAnimationSystem.cs`

**Investigation Goals**
- confirm `IBlockHitListener` is the correct sound hook for shield impact
- determine whether the sound should be played at:
  - shield collision point
  - player center
- ensure the sound only fires while actively blocking

**Current Status**
- `IMPLEMENTED, NEEDS IN-GAME VERIFICATION`

**Current Findings**
- `ShieldProjectileBlocker` raises `IBlockHitListener` only when `BlockSystem.IsBlocking` is true
- `CharacterAnimationSystem` already listens to `IBlockHitListener` and triggers blocked-hit visuals
- this makes `IBlockHitListener` the correct decoupled sound hook for shield impact
- no current listener plays audio for this event
- the event already carries `hitPoint`, which is the best positional sound location for shield impacts
- there is currently no dedicated shield-impact audio asset in the repo
- implemented shield-impact playback on `CharacterSoundSystem` via `IBlockHitListener`

---

## Priority 3 - Bud Enemy Audio

### Task 6 - Bud Attack Audio
**What to validate**
- attack windup/fire sound in sync with the actual attack animation

**Files / Systems to Investigate**
- `Assets/DGM6405/Scripts/Enemies/Brains/BudBrain.cs`
- `Assets/DGM6405/Scripts/Core/Animation/EnemyAnimatorDriver.cs`
- `Assets/DGM6405/Scripts/Events/AnimatorEventDispatcher.cs`
- bud animator clips / animation events

**Investigation Goals**
- confirm the Bud uses attack animation events in live gameplay
- add enemy sound handling through animation events, not just `OnShoot(true)`
- decide whether Bud needs its own sound system component parallel to `CharacterSoundSystem`

**Current Status**
- `IMPLEMENTED, NEEDS IN-GAME VERIFICATION`

**Current Findings**
- `BudBrain` raises `IShootListener.OnShoot(true)` while the player is detected
- `EnemyAnimatorDriver` converts that into the attack trigger
- Bud attack clips call `AnimatorEventDispatcher.FireProjectile()`
- `ProjectileWeapon` waits for that `IFireProjectileListener` event and fires in `LateUpdate`
- this confirms Bud attack timing is animation-driven already, but there is still no enemy sound component or attack audio listener
- there is currently no dedicated Bud attack audio asset in the repo
- implemented Bud attack audio on `EnemyAnimatorDriver` via `IFireProjectileListener`, so the sound fires on the same animation event that releases the projectile

### Task 7 - Bud Damage / Death Audio
**What to validate**
- hit reaction audio
- death audio

**Files / Systems to Investigate**
- Bud health flow
- `EnemyAnimatorDriver.cs`
- local health event listeners on Bud prefab

**Investigation Goals**
- confirm whether those reactions are present and noticeable enough to warrant SFX
- keep implementation local to enemy events if added

**Current Status**
- `PARTIALLY IMPLEMENTED FOR VISUALS ONLY`

**Current Findings**
- `Health` is the canonical local damage/death event source
- `EnemyAnimatorDriver` implements `IHealthListener.OnDied()` and has a `TriggerHit()` method
- Bud currently does not call `TriggerHit()` on damage the way `TrainingDummyBrain` does
- no Bud-specific audio listener was found for damage or death
- there are currently no dedicated Bud hit/death audio assets in the repo

---

## Priority 4 - Projectile Audio

### Task 8 - Projectile Launch Audio
**What to validate**
- player-used projectile launch audio if player shooting is active in prototype
- Bud projectile launch audio

**Files / Systems to Investigate**
- `Assets/DGM6405/Scripts/Weapons/ProjectileWeapon.cs`
- firing animation events
- weapon prefabs

**Investigation Goals**
- attach launch sound to the same animation-timed event window as projectile release when possible
- avoid duplicating sound if both weapon logic and animation logic try to fire it

**Current Status**
- `PARTIALLY VERIFIED`

**Current Findings**
- Bud projectile spawn is animation-timed, so Bud launch audio should key off the same attack animation window
- `ProjectileWeapon` itself has no sound path
- player shooting remains prototype-scope dependent and should only be included if the ranged player flow is actually used
- there is currently no dedicated projectile-launch audio asset in the repo

### Task 9 - Projectile Impact Audio
**What to validate**
- projectile hitting world
- projectile hitting player
- projectile hitting shield

**Files / Systems to Investigate**
- projectile collision / lifetime scripts
- shield block collision flow
- damage receiver flow

**Investigation Goals**
- separate normal impact audio from shield-block impact audio
- confirm whether projectile prefabs already contain impact VFX/SFX logic

**Current Status**
- `IMPLEMENTED, NEEDS IN-GAME VERIFICATION`

**Current Findings**
- `Projectile.OnCollisionEnter(...)` applies damage and immediately destroys the projectile
- no impact audio is played on world hit, player hit, or normal enemy hit
- shield impact should stay separate and remain on the shield/block event path, not generic projectile impact logic
- there is currently no dedicated projectile-impact audio asset in the repo
- implemented generic projectile impact audio on `Projectile`
- generic projectile impact now skips shield collisions so shield absorb uses the dedicated block-hit sound path

---

## Verification Matrix

### Menus
- `VERIFY` Main menu hover/click
- `VERIFY` Settings hover/click
- `VERIFY` Settings sliders/toggles
- `VERIFY` Pause menu hover/click
- `VERIFY` Button error feedback
- `IMPLEMENTED, VERIFY` Main menu music hookup
- `IMPLEMENTED, VERIFY` In-game music hookup
- `IMPLEMENTED, VERIFY` Pause music behavior hookup

### In Game
- `IMPLEMENTED, VERIFY` ambient/gameplay music presence
- `VERIFY` state transitions do not cut audio unexpectedly

### Player
- `VERIFIED IN ASSETS` footsteps
- `NEEDS TIMING REVIEW` landing
- `IMPLEMENTED, VERIFY` block activated
- `IMPLEMENTED, VERIFY` block deactivated
- `IMPLEMENTED, VERIFY` shield absorbed hit

### Bud Enemy
- `IMPLEMENTED, VERIFY` attack/fire
- `UNVERIFIED` hit reaction
- `UNVERIFIED` death

### Projectiles
- `IMPLEMENTED, VERIFY` launch
- `IMPLEMENTED, VERIFY` impact on world
- `IMPLEMENTED, VERIFY` impact on player
- `MISSING` impact on shield

---

## Implementation Rules
- Prefer local event listeners plus animation events for character/enemy gameplay SFX
- Use `AudioMgr` and global audio events for non-3D menu/UI sounds
- Reuse current prototype mechanics only
- Do not wire sounds to unused combat branches just because clip fields already exist
- Keep changes modular:
  - command brains emit intent
  - animation systems drive timing
  - sound systems respond without owning gameplay logic

---

## Update Log

### 2026-03-22
- Created initial audio validation pass plan
- Confirmed UI button hover/click/error global audio path
- Confirmed player footstep system exists and is animation-event capable
- Confirmed shield block-hit event exists via `ShieldProjectileBlocker`
- Confirmed Bud projectile firing already uses animation-event dispatch for timing
- Found `CharacterSoundSystem` combat event handlers are present but currently commented out
- Found no confirmed caller of `AudioMgr.PlayMusic(...)` yet
- Confirmed `RangerV3.prefab` has `CharacterSoundSystem` and combat clips assigned even though block/shoot/melee listeners are not active
- Confirmed player locomotion and landing clips contain `PlayFootstep` / `PlayLanding` animation events
- Confirmed `EnemyBud Variant.prefab` includes `AnimatorEventDispatcher`
- Confirmed Bud attack animation clips contain `FireProjectile` events
- Confirmed `Projectile` collision flow has no impact audio path
- Found `ButtonSounds` wiring on the shared button prefab and `SettingsUI.prefab`
- Confirmed the repo does not currently contain dedicated combat/enemy/projectile SFX assets beyond UI sounds, footsteps, landing, and music
- Confirmed several player combat clip assignments on `RangerV3.prefab` currently reuse UI sounds or a footstep clip as placeholders
- Added missing `.meta` files for the newly provided audio assets so they can be referenced by prefabs
- Implemented scene-based music playback and pause/resume music control
- Added `CharacterSoundSystem` to the live `Player.prefab` and wired block start/end, shield hit, and death clips
- Wired Bud attack audio to the animation-timed `IFireProjectileListener` event in `EnemyAnimatorDriver`
- Wired Bud projectile impact audio on `EnemySeedProjectileWithGravity Variant.prefab`
- Shield impact remains separate from generic projectile impact and should be validated in play
