# AI Task List — Investigation & Fix Plan (Sequential, Agent-Friendly)

## Instructions for Agent
For EACH task:
1. Investigate the listed systems/files
2. Summarize findings and root cause
3. Propose a minimal, safe solution
4. Ask for approval BEFORE making changes
5. Implement only after approval
6. Verify behavior and report results

Do NOT batch tasks. Complete one at a time.

---

## PRIORITY 1 — Critical Gameplay Feel & Reliability

### Task 1 — Killbox Reliability (COMPLETE)
**What to change**
- Ensure player death triggers consistently when falling or clipping edges

**Files / Systems to Investigate**
- Player death / damage handling scripts
- Killbox trigger colliders in level prefabs
- Any physics-based death checks (OnTriggerEnter, OnCollision)
- Level scenes with reported issues

**Investigation Goals**
- Determine why edge cases fail (collider size, layer filtering, missed triggers)
- Check if fast movement skips trigger detection

**Expected Fix Direction**
- Expand killbox bounds OR
- Add fallback Y-position death check OR
- Improve collision detection method

---

### Task 2 — Jump Timing Forgiveness (COMPLETE)
**What to change**
- Make jump input feel responsive even if slightly late

**Files / Systems to Investigate**
- Player movement controller
- Jump input handling
- Ground detection logic

**Investigation Goals**
- Identify how grounded state is determined
- Check if jump only allowed strictly while grounded

**Summary of Changes**
- Added Coyote Time (0.15s) to allow jumping shortly after leaving ground.
- Added Jump Buffering (0.15s) to queue jump input shortly before landing.
- Refactored JumpGravitySystem.cs to use timer-based jump logic.

**Expected Fix Direction**
- Add coyote time (short grace period after leaving ground)
- Optionally add jump input buffering

---

### Task 3 — Movement Responsiveness (Acceleration & Momentum)
**What to change**
- Smooth acceleration and prevent unexpected mid-air slowdown

**Files / Systems to Investigate**
- Player movement script
- Sprint / run logic
- Velocity handling (Rigidbody or CharacterController)

**Investigation Goals**
- Identify where acceleration is applied (instant vs interpolated)
- Check how velocity is modified when input changes mid-air

**Expected Fix Direction**
- Smooth acceleration curve (lerp or acceleration value)
- Preserve horizontal velocity in air
- Separate ground vs air control logic

---

## PRIORITY 2 — Player Experience & Control Clarity

### Task 4 — Input Feedback for Incorrect Actions (COMPLETE)
**What to change**
- Provide feedback when player presses wrong input

**Files / Systems to Investigate**
- Input system bindings
- UI feedback systems (instruction text, prompts)
- Localization input display system

**Investigation Goals**
- Determine how inputs are validated
- Identify where incorrect input is ignored silently

**Expected Fix Direction**
- Add feedback trigger on invalid input
- Hook into UI (text flash, message, or indicator)

---

### Task 5 — Mouse Sensitivity Control (COMPLETE)
**What to change**
- Allow adjustable mouse sensitivity for camera control

**Files / Systems to Investigate**
- Camera controller script
- Input scaling logic
- Settings menu UI
- Save system (SavedValues)

**Investigation Goals**
- Identify where mouse delta is applied
- Check if sensitivity is hardcoded

**Summary of Changes**
- Added `LookSensitivity` to `SavedValues.cs`.
- Added `_sensitivitySlider` and logic to `Settings.cs` to update sensitivity.
- Applied sensitivity multiplier in `CameraRotationSystem.cs` for both mouse and gamepad/other look input.

**Expected Fix Direction**
- Add sensitivity multiplier
- Expose setting in UI
- Save/load via existing save system

---

### Task 6 — Death Flow Optimization (COMPLETE)
**What to change**
- Allow skipping or shortening death animation
- Prevent pausing during death sequence (redundant)

**Files / Systems to Investigate**
- GameOver / death handling flow
- Animation triggers
- Input handling during death state

**Investigation Goals**
- Identify where death animation blocks input
- Check transition timing to restart

**Summary of Changes**
- Implemented `ISkipDeathListener` in `Listeners.cs` and `PlayerDeathHandler.cs`.
- Modified `PlayerCommandBrain.cs` to raise `OnSkipDeathAnimation` when any skip input (Jump, Fire, Sprint, Block) is detected during death, simplifying the interface requirements for `PlayerDeathHandler`.
- Modified `DeathSequence` to break early if skip is requested, immediately triggering `GameOver`.
- Added `IsDead` property to `PlayerDeathHandler`.
- Updated `PlayerMgr.PauseInput()` to ignore pause requests if the player is dead/dying.
- Fixed collision between `EventBus` listener methods and Unity's `PlayerInput` message system by using explicit interface implementations for input-related listeners in `CharacterAnimationSystem.cs`, `CharacterSoundSystem.cs`, and `EnemyAnimatorDriver.cs`.
- Verified that `JumpGravitySystem.cs` was already using explicit interface implementation for `OnJump`, preventing double-firing.
- Fixed an issue where `PlayerCommandBrain` was blocking all input processing when the player was dead, preventing the death skip functionality from working. Implemented a restricted `ProcessDeathSkipInputs` to allow skip triggers during the death sequence.

**Expected Fix Direction**
- Allow input to interrupt animation
- Reduce delay before retry

---

## PRIORITY 3 — Level Design & Spatial Clarity

### Task 7 — Sign Visibility at Level Start (COMPLETE)
**What to change**
- Ensure instructions are visible immediately

**Files / Systems to Investigate**
- Camera spawn position and rotation
- Level start layout
- Instruction sign placement

**Investigation Goals**
- Determine if issue is camera or level design
- Check if player control starts before camera settles

**Expected Fix Direction**
- Adjust camera framing OR
- Reposition signs OR
- Delay player input briefly

---

### Task 8 — Edge Sliding / Platform Friction
**What to change**
- Reduce unintended sliding off narrow platforms

**Files / Systems to Investigate**
- Player physics material or movement logic
- Platform colliders
- Rigidbody / CharacterController settings

**Investigation Goals**
- Identify cause (low friction, slope handling, velocity decay)
- Check collision vs visual mesh alignment

**Expected Fix Direction**
- Increase friction OR
- Adjust movement damping OR
- Slightly expand collision bounds

---

### Task 9 — Enemy Height & Projectile Alignment (Complete)
**What to change**
- Ensure enemy projectiles align correctly with player

**Files / Systems to Investigate**
- Enemy prefab
- Projectile spawn (muzzle transform)
- Player hitbox

**Investigation Goals**
- Verify projectile origin height vs player center
- Check for recent muzzle transform changes

**Expected Fix Direction**
- Adjust enemy height or muzzle position
- Validate projectile trajectory

---

## PRIORITY 4 — Polish & System Improvements

### Task 10 — Camera as Guidance Tool
**What to change**
- Improve camera framing for readability and navigation

**Files / Systems to Investigate**
- Camera controller
- Camera offsets and follow logic
- Level start camera state

**Investigation Goals**
- Determine if camera contributes to jump misjudgment
- Check field of view and angle

**Expected Fix Direction**
- Adjust camera angle or distance
- Improve visibility of landing zones

---

### Task 11 — Platform Collision Tuning
**What to change**
- Improve fairness of platform interactions

**Files / Systems to Investigate**
- Platform colliders vs visual meshes
- Level geometry

**Investigation Goals**
- Identify mismatch between visuals and collision
- Check narrow platform tolerances

**Expected Fix Direction**
- Slightly enlarge colliders
- Adjust collision shapes for consistency

---

### Task 12 — Future: Leaderboard Data Separation
**What to change**
- Separate progression data from leaderboard data

**Files / Systems to Investigate**
- SaveUtil
- SavedValues
- LevelMgr completion logic

**Investigation Goals**
- Understand current save structure
- Identify coupling between progression and best times

**Expected Fix Direction**
- Split save data into separate structures
- Maintain backward compatibility

---

## Execution Order
1. Killbox Reliability (COMPLETE)
2. Jump Forgiveness (COMPLETE)
3. Movement Responsiveness (WONT DO)
4. Input Feedback (COMPLETE)
5. Mouse Sensitivity (COMPLETE)
6. Death Flow (COMPLETE)
7. Sign Visibility (COMPLETE)
8. Edge Sliding (WONT DO)
9. Enemy Alignment (COMPLETE)
10. Camera Improvements (WONT DO)
11. Platform Collision (WONT DO)
12. Leaderboards (optional)

---

## Completion Criteria
- Each task verified in-game
- No regressions introduced
- Changes remain modular and maintainable
- Agent documents all modifications

---