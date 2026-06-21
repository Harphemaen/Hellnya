using UnityEngine;

public class EnemyType2 : EnemyBase
{
    [Header("Type 2 Homing Shot")]
    [SerializeField] private float bulletTurnSpeedDegrees = 120f;

    protected override void Awake()
    {
        base.Awake();
        NormalizeType2Settings();
    }

    protected override void Reset()
    {
        base.Reset();
        maxHealth = 3;
        canShoot = true;
        shotInterval = 2f;
        shotIntervalRandom = 0f;
        shotAngle = 180f;
        muzzleOffset = new Vector2(-0.7f, 0.1f);
        NormalizeType2Settings();
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        NormalizeType2Settings();
    }

    protected override void Move()
    {
    }

    protected override void Shoot()
    {
        Vector3 spawnPosition = muzzlePoint != null
            ? muzzlePoint.position
            : transform.position + new Vector3(muzzleOffset.x, muzzleOffset.y, 0f);

        EnemyBullet bullet = CreateBullet(spawnPosition);
        Vector2 shotDirection = AngleToDirection(shotAngle);
        HomingEnemyBullet homingBullet = bullet as HomingEnemyBullet;
        if (homingBullet != null)
        {
            homingBullet.Init(shotDirection, bulletSpeed, bulletLifeTime, bulletTurnSpeedDegrees);
            return;
        }

        bullet.Init(shotDirection, bulletSpeed, bulletLifeTime);
    }

    private void NormalizeType2Settings()
    {
        bulletTurnSpeedDegrees = Mathf.Max(0f, bulletTurnSpeedDegrees);
    }
}
