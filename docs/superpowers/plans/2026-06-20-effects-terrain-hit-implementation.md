# Effects Terrain Hit Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add separate player/enemy death effects and one shared bullet hit effect that triggers on enemies and Terrain layer collisions.

**Architecture:** Existing player/enemy/bullet scripts keep owning effect spawning through their serialized prefab fields. A tiny runtime `SimpleEffectAnimator` script gives prefab-only effects a visible scale/fade animation without adding an effect manager or pooling system.

**Tech Stack:** Unity 2020.3.14f1, C#, Unity YAML prefabs/scenes, Physics2D trigger collisions.

---

## Tasks

### Task 1: Bullet Terrain Hit Logic

**Files:**
- Modify `Assets/Scripts/Combat/M18Bullet.cs`
- Modify `Assets/Scripts/Enemies/EnemyBullet.cs`

- [x] Add serialized `LayerMask terrainLayers`.
- [x] Default empty masks to `LayerMask.GetMask("Terrain")`.
- [x] Destroy bullets through `DestroyByHit()` when they enter a Terrain collider.
- [x] Keep existing enemy/player hit behavior.

### Task 2: Effect Prefabs

**Files:**
- Create `Assets/Scripts/Effects/SimpleEffectAnimator.cs`
- Create `Assets/Prefabs/Effects/PlayerDeathEffect.prefab`
- Create `Assets/Prefabs/Effects/EnemyDeathEffect.prefab`
- Create `Assets/Prefabs/Effects/BulletHitEffect.prefab`
- Create `Assets/Prefabs/Ammo/EnemyBullet.prefab`

- [x] Add a minimal animated square effect script.
- [x] Use different colors/scales/durations for player death, enemy death, and bullet hit.
- [x] Configure player and enemy bullets to use `BulletHitEffect`.

### Task 3: Scene And Prefab Wiring

**Files:**
- Modify `ProjectSettings/TagManager.asset`
- Modify `Assets/Scenes/SampleScene.unity`
- Modify `Assets/Prefabs/Ammo/90mmapc.prefab`
- Modify `Assets/Prefabs/Enemies_Prefabs/Enemy_Basic1.prefab`

- [x] Add `Terrain` at layer index 8.
- [x] Assign `GroundBg_01` through `GroundBg_06` to layer 8.
- [x] Add `BoxCollider2D` to `GroundBg_01` through `GroundBg_06`.
- [x] Assign `PlayerDeathEffect` to `Player_M18`.
- [x] Assign `EnemyDeathEffect` and `EnemyBullet.prefab` to `Enemy_Basic1`.

### Task 4: Verification

- [x] Check generated files exist.
- [x] Check references with `rg`.
- [x] Check scene has exactly six Terrain ground objects and six new colliders.
- [x] Run lightweight C# compile check for changed runtime scripts.
- [ ] Run full Unity Play Mode verification in the open editor.

Unity batchmode could not run because the project was already open in GUI Unity. Use the open editor for final play checks: bullet hits enemy, bullet hits ground, enemy death, player death.
