# Effect Prefabs And Terrain Hit Design

Date: 2026-06-20

## Goal

Add separate death effects for the player and enemies, and one shared bullet hit effect. A bullet hit means the bullet collided with an enemy or with a terrain object.

## Scope

- Add PlayerDeathEffect prefab for M18 player death.
- Add EnemyDeathEffect prefab for enemy death.
- Add BulletHitEffect prefab for all bullet impacts.
- Add Terrain layer and use it to mark current test ground objects.
- Make player bullets spawn BulletHitEffect when they hit enemies or Terrain.
- Keep existing deathAnimationPrefab fields; wire the scene/prefabs to the new effect prefabs.

## Design

Player and enemy death remain owned by their existing scripts. M18Player spawns its configured deathAnimationPrefab when health reaches zero. EnemyBase spawns its configured deathAnimationPrefab when health reaches zero.

Bullet hit effects remain owned by each bullet script. M18Bullet already has a deathAnimationPrefab field and DestroyByHit() path for enemy hits. Add a terrain layer mask to M18Bullet so OnTriggerEnter2D can call DestroyByHit() when the other collider is on Terrain. EnemyBullet keeps the same hit effect path when it hits the player.

The current test scene has background ground sprites but no terrain identity. Add a Terrain layer in TagManager and assign it to GroundBg_* objects. Add BoxCollider2D to those ground objects so bullets can collide with them.

## Testing

- Compile scripts in Unity.
- In Play Mode, confirm player bullets spawn BulletHitEffect on Enemy_Basic1.
- In Play Mode, confirm player bullets spawn BulletHitEffect on Terrain ground.
- Confirm player death and enemy death use different effect prefabs.

## Deferred

No effect manager, pooling, per-weapon hit effects, or final art pipeline yet. Replace temporary effect prefabs when final art exists.

