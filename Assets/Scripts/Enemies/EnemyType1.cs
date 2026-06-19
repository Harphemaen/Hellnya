// 功能：敌人类型 1，飞到当前屏幕的指定位置，短暂停留并可射击一次，然后向左飞出。
// 技术要点：继承 EnemyBase；停止点使用 viewport 坐标，每帧换算成当前摄像机下的世界坐标，避免摄像机移动导致追旧坐标。
// 配置：flySpeed 飞行速度；stopScreenPosition 停止点 viewport 坐标(0-1)；useRandomStopScreenPosition/randomStopScreenMin/Max 随机停止区域；stopSeconds 停留时间；shootOnStop 停住时是否左斜下射击一次。
// 版本：0.2.0

using UnityEngine;

public class EnemyType1 : EnemyBase
{
    private const float StopShotAngle = 210f;

    [Header("Type 1 Flight")]
    [SerializeField] private float flySpeed = 3f;
    [SerializeField] private Vector2 stopScreenPosition = new Vector2(0.75f, 0.5f);
    [SerializeField] private bool useRandomStopScreenPosition;
    [SerializeField] private Vector2 randomStopScreenMin = new Vector2(0.65f, 0.35f);
    [SerializeField] private Vector2 randomStopScreenMax = new Vector2(0.85f, 0.65f);
    [SerializeField] private float stopSeconds = 0.5f;
    [SerializeField] private bool shootOnStop = true;

    private Vector2 resolvedStopScreenPosition;
    private float stopTimer;
    private State state;

    private enum State
    {
        Entering,
        Stopping,
        Leaving
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            ResetFlight();
        }
    }

    protected override void Move()
    {
        switch (state)
        {
            case State.Entering:
                MoveIntoStopPosition();
                break;
            case State.Stopping:
                WaitAtStopPosition();
                break;
            case State.Leaving:
                transform.position += Vector3.left * GetFlySpeed() * Time.deltaTime;
                break;
        }
    }

    protected override void TryShoot()
    {
    }

    private void ResetFlight()
    {
        resolvedStopScreenPosition = ResolveStopScreenPosition();
        stopTimer = 0f;
        state = State.Entering;
    }

    private Vector2 ResolveStopScreenPosition()
    {
        if (!useRandomStopScreenPosition)
        {
            return ClampViewport(stopScreenPosition);
        }

        float minX = Mathf.Min(randomStopScreenMin.x, randomStopScreenMax.x);
        float maxX = Mathf.Max(randomStopScreenMin.x, randomStopScreenMax.x);
        float minY = Mathf.Min(randomStopScreenMin.y, randomStopScreenMax.y);
        float maxY = Mathf.Max(randomStopScreenMin.y, randomStopScreenMax.y);
        return ClampViewport(new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY)));
    }

    private void MoveIntoStopPosition()
    {
        Vector3 target = GetCurrentStopWorldPosition();
        transform.position = Vector3.MoveTowards(transform.position, target, GetFlySpeed() * Time.deltaTime);
        if ((transform.position - target).sqrMagnitude <= 0.0001f)
        {
            transform.position = target;
            StartStopping();
        }
    }

    private void StartStopping()
    {
        state = State.Stopping;
        stopTimer = 0f;
        transform.position = GetCurrentStopWorldPosition();

        if (shootOnStop)
        {
            ShootDownLeftOnce();
        }

        if (GetStopSeconds() <= 0f)
        {
            state = State.Leaving;
        }
    }

    private void WaitAtStopPosition()
    {
        transform.position = GetCurrentStopWorldPosition();
        stopTimer += Time.deltaTime;
        if (stopTimer >= GetStopSeconds())
        {
            state = State.Leaving;
        }
    }

    private void ShootDownLeftOnce()
    {
        Vector3 spawnPosition = muzzlePoint != null
            ? muzzlePoint.position
            : transform.position + new Vector3(muzzleOffset.x, muzzleOffset.y, 0f);

        EnemyBullet bullet = CreateBullet(spawnPosition);
        bullet.Init(
            AngleToDirection(StopShotAngle),
            bulletSpeed,
            bulletLifeTime,
            bulletSprite,
            bulletSortingOrder);
    }

    private Vector3 GetCurrentStopWorldPosition()
    {
        return GameWorldContext.ViewportToWorld(resolvedStopScreenPosition, transform.position.z);
    }

    private float GetFlySpeed()
    {
        return Mathf.Max(0.01f, flySpeed);
    }

    private float GetStopSeconds()
    {
        return Mathf.Max(0f, stopSeconds);
    }

    private static Vector2 ClampViewport(Vector2 position)
    {
        return new Vector2(Mathf.Clamp01(position.x), Mathf.Clamp01(position.y));
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 viewportPosition = useRandomStopScreenPosition
            ? (randomStopScreenMin + randomStopScreenMax) * 0.5f
            : stopScreenPosition;
        Vector3 target = GameWorldContext.ViewportToWorld(ClampViewport(viewportPosition), transform.position.z);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, target);
        Gizmos.DrawWireSphere(target, 0.2f);
    }
}