using UnityEngine;

public class HomingEnemyBullet : EnemyBullet
{
    [Header("Homing")]
    [SerializeField] private float turnSpeedDegrees = 120f;

    protected override void Awake()
    {
        base.Awake();
        NormalizeHomingSettings();
    }

    protected override void Reset()
    {
        base.Reset();
        NormalizeHomingSettings();
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        NormalizeHomingSettings();
    }

    protected override void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        SteerTowardPlayer();
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    public void Init(Vector2 newDirection, float newSpeed, float newLifeTime, float newTurnSpeedDegrees)
    {
        Init(newDirection, newSpeed, newLifeTime);
        turnSpeedDegrees = Mathf.Max(0f, newTurnSpeedDegrees);
    }

    private void SteerTowardPlayer()
    {
        Transform player = GameWorldContext.CurrentPlayer;
        if (player == null)
        {
            return;
        }

        Vector2 targetDirection = player.position - transform.position;
        if (targetDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        float currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        float nextAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, turnSpeedDegrees * Time.deltaTime);
        direction = AngleToDirection(nextAngle);
        FaceMoveDirection();
    }

    private void NormalizeHomingSettings()
    {
        turnSpeedDegrees = Mathf.Max(0f, turnSpeedDegrees);
    }

    private static Vector2 AngleToDirection(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;
    }
}
