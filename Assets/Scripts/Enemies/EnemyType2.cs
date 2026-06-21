using UnityEngine;

public class EnemyType2 : EnemyBase
{
    [Header("Type 2 Homing Shot")]
    [SerializeField] private int shotWays = 1;
    [SerializeField] private float shotSpreadAngle = 15f;
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
        shotWays = 1;
        shotSpreadAngle = 15f;
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

        int safeShotWays = Mathf.Max(1, shotWays);
        float startAngle = shotAngle - shotSpreadAngle * (safeShotWays - 1) * 0.5f;
        for (int i = 0; i < safeShotWays; i++)
        {
            FireOne(spawnPosition, startAngle + shotSpreadAngle * i);
        }
    }

    private void FireOne(Vector3 spawnPosition, float angle)
    {
        EnemyBullet bullet = CreateBullet(spawnPosition);
        Vector2 shotDirection = AngleToDirection(angle);
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
        shotWays = Mathf.Max(1, shotWays);
        shotSpreadAngle = Mathf.Max(0f, shotSpreadAngle);
        bulletTurnSpeedDegrees = Mathf.Max(0f, bulletTurnSpeedDegrees);
    }
}
