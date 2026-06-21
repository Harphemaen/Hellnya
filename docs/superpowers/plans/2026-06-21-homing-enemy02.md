# Homing Enemy02 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a homing enemy bullet and a stationary enemy02 that fires it on a configurable interval.

**Architecture:** Keep existing `EnemyBase` and `EnemyBullet` behavior intact. Add one homing bullet component for finite-turn pursuit and one enemy type that stays fixed while using existing shot fields plus `muzzleOffset`/`muzzlePoint`.

**Tech Stack:** Unity 2020.3 C#, prefab YAML.

---

### Task 1: Homing Bullet

**Files:**
- Create: `Assets/Scripts/Enemies/HomingEnemyBullet.cs`
- Create: `Assets/Scripts/Enemies/HomingEnemyBullet.cs.meta`
- Create: `Assets/Prefabs/Ammo/HomingEnemyBullet.prefab`
- Create: `Assets/Prefabs/Ammo/HomingEnemyBullet.prefab.meta`

- [x] Add `HomingEnemyBullet : EnemyBullet` with `turnSpeedDegrees`, steering toward `GameWorldContext.CurrentPlayer`.
- [x] Reuse `EnemyBullet.Init(...)`, terrain hit, player hit, and death animation behavior.
- [x] Create a prefab using the existing enemy bullet sprite and bullet hit effect.

### Task 2: Enemy02

**Files:**
- Create: `Assets/Scripts/Enemies/EnemyType2.cs`
- Create: `Assets/Scripts/Enemies/EnemyType2.cs.meta`
- Create: `Assets/Prefabs/Enemies_Prefabs/Enemy02.prefab`
- Create: `Assets/Prefabs/Enemies_Prefabs/Enemy02.prefab.meta`

- [x] Add `EnemyType2 : EnemyBase` with no movement.
- [x] Use configurable `maxHealth`, `shotInterval`, `shotAngle`, `muzzleOffset`, and optional `muzzlePoint`.
- [x] Create an enemy02 prefab wired to `HomingEnemyBullet`.

### Task 3: Verify

**Files:**
- Read changed files only.

- [x] Compile changed runtime scripts with Unity assemblies.
- [x] Verify prefab script GUIDs and effect references exist.
- [x] Commit only files created for this feature.
