# Pickups And Mines Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add configurable pickup items, enemy death drops, and mine-style player touch damage without creating prefabs.

**Architecture:** Add one `PickupItem` script for all pickup effects and movement. Extend existing `EnemyBase` for death drops and mine touch damage, and extend `M18Player` with a fire-rate bonus method.

**Tech Stack:** Unity 2020.3 C#.

---

### Task 1: Player Upgrade Hook

**Files:**
- Modify: `Assets/Scripts/Player/M18Player.cs`

- [x] Add `AddShotsPerSecond(float amount)` so pickup type 2 can raise fire rate.

### Task 2: Pickup Item

**Files:**
- Create: `Assets/Scripts/PickupItem.cs`
- Create: `Assets/Scripts/PickupItem.cs.meta`

- [x] Add one configurable pickup script with three item types.
- [x] Move up for `flyUpSeconds`, then fall at `fallSpeed`.
- [x] Apply effect and optional pickup sound on player trigger.
- [x] For screen damage, flash the screen and damage visible `EnemyBase` instances.
- [x] Destroy after leaving the camera view.

### Task 3: Enemy Drops And Mines

**Files:**
- Modify: `Assets/Scripts/Enemies/EnemyBase.cs`

- [x] Add optional pickup drop fields used by specific enemies.
- [x] Add optional player touch damage fields so a ground mine can be configured with `EnemyBase`.

### Task 4: Verify

**Files:**
- Read changed files only.

- [x] Compile changed runtime scripts with Unity assemblies.
- [x] Commit only code and plan files, no prefab or scene changes.
