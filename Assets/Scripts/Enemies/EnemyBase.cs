// 功能：敌人基础类别，提供血量、受击、碰撞盒、简单左移、基础射击、死亡动画和左屏外销毁。
// 技术要点：外观交给对象自己的 SpriteRenderer；特化敌人继承本类并覆盖移动或射击逻辑。
// 配置：maxHealth 血量；destroyWhenHealthZero 是否死亡销毁；deathAnimationPrefab 死亡动画；moveSpeed 默认左移速度；hitBoxSize/Offset 碰撞盒；canShoot/shotInterval/shotAngle 射击；bulletPrefab/bulletSpeed/bulletLifeTime 子弹。
// 版本：0.3.0

using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class EnemyBase : MonoBehaviour
{
    private const float DestroyAfterLeftScreenSeconds = 2f;

    [Header("Health")]
    [SerializeField] protected int maxHealth = 1;
    [SerializeField] protected bool destroyWhenHealthZero = true;
    [SerializeField] protected GameObject deathAnimationPrefab;
    [SerializeField] protected float deathAnimationLifeTime = 1f;

    [Header("Move")]
    [SerializeField] protected float moveSpeed = 1.2f;

    [Header("Hit Box")]
    [SerializeField] protected bool autoConfigureHitBox = true;
    [SerializeField] protected Vector2 hitBoxSize = new Vector2(0.9f, 0.7f);
    [SerializeField] protected Vector2 hitBoxOffset = Vector2.zero;

    [Header("Touch Damage")]
    [SerializeField] protected bool damagePlayerOnTouch;
    [SerializeField] protected int touchDamage = 1;
    [SerializeField] protected bool destroySelfOnTouchPlayer;

    [Header("Shot")]
    [SerializeField] protected bool canShoot = true;
    [SerializeField] protected EnemyBullet bulletPrefab;
    [SerializeField] protected Transform muzzlePoint;
    [SerializeField] protected Vector2 muzzleOffset = new Vector2(-0.6f, 0f);
    [SerializeField] protected float bulletSpeed = 4f;
    [SerializeField] protected float bulletLifeTime = 4f;
    [SerializeField] protected float shotInterval = 1.5f;
    [SerializeField] protected float shotIntervalRandom = 0.25f;
    [SerializeField] protected float initialShotDelay = 0.5f;
    [SerializeField] protected float shotAngle = 180f;

    protected BoxCollider2D hitBox;
    protected int currentHealth;
    protected float nextShotTime;
    private float leftScreenTimer;

    protected virtual void Awake()
    {
        NormalizeSettings();
        CacheComponents();
        ApplyColliderSettings();

        if (Application.isPlaying)
        {
            currentHealth = maxHealth;
            ScheduleNextShot(initialShotDelay);
        }
    }

    protected virtual void Reset()
    {
        NormalizeSettings();
        CacheComponents();
        ApplyColliderSettings();
    }

    protected virtual void OnEnable()
    {
        CacheComponents();
        ApplyColliderSettings();
    }

    protected virtual void OnValidate()
    {
        NormalizeSettings();
    }

    protected virtual void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        Move();
        TryShoot();
    }

    protected virtual void LateUpdate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        DestroyAfterLeftScreen();
    }

    protected virtual void Move()
    {
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;
    }

    protected virtual void TryShoot()
    {
        if (!canShoot || Time.time < nextShotTime)
        {
            return;
        }

        Shoot();
        ScheduleNextShot(shotInterval + Random.Range(-shotIntervalRandom, shotIntervalRandom));
    }

    protected virtual void Shoot()
    {
        Vector3 spawnPosition = muzzlePoint != null
            ? muzzlePoint.position
            : transform.position + new Vector3(muzzleOffset.x, muzzleOffset.y, 0f);

        EnemyBullet bullet = CreateBullet(spawnPosition);
        bullet.Init(AngleToDirection(shotAngle), bulletSpeed, bulletLifeTime);
    }

    protected virtual EnemyBullet CreateBullet(Vector3 spawnPosition)
    {
        if (bulletPrefab != null)
        {
            return Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        }

        GameObject bulletObject = new GameObject("Enemy_Bullet");
        bulletObject.transform.position = spawnPosition;
        return bulletObject.AddComponent<EnemyBullet>();
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        M18Player player = other.GetComponent<M18Player>();
        if (player != null)
        {
            TouchPlayer(player);
            return;
        }

        M18Bullet playerBullet = other.GetComponent<M18Bullet>();
        if (playerBullet == null)
        {
            return;
        }

        if (playerBullet.DestroyByHit())
        {
            TakeDamage(1);
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if (!Application.isPlaying)
        {
            return;
        }

        currentHealth -= Mathf.Max(1, damage);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (destroyWhenHealthZero)
        {
            DropPickupIfConfigured();
            SpawnDeathAnimation();
            Destroy(gameObject);
        }
    }

    protected virtual void TouchPlayer(M18Player player)
    {
        if (!damagePlayerOnTouch || player == null)
        {
            return;
        }

        player.TakeDamage(touchDamage);
        if (destroySelfOnTouchPlayer)
        {
            Die();
        }
    }

    protected void DropPickupIfConfigured()
    {
        EnemyPickupDropper dropper = GetComponent<EnemyPickupDropper>();
        if (dropper == null)
        {
            return;
        }

        dropper.Drop();
    }

    protected void ScheduleNextShot(float delay)
    {
        nextShotTime = Time.time + Mathf.Max(0.01f, delay);
    }

    private void DestroyAfterLeftScreen()
    {
        if (GameWorldContext.CurrentCamera == null)
        {
            leftScreenTimer = 0f;
            return;
        }

        float leftEdgeX = GameWorldContext.CameraLeftEdgeX;
        if (transform.position.x >= leftEdgeX)
        {
            leftScreenTimer = 0f;
            return;
        }

        leftScreenTimer += Time.deltaTime;
        if (leftScreenTimer >= DestroyAfterLeftScreenSeconds)
        {
            Destroy(gameObject);
        }
    }

    protected void NormalizeSettings()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        deathAnimationLifeTime = Mathf.Max(0f, deathAnimationLifeTime);
        moveSpeed = Mathf.Max(0f, moveSpeed);
        hitBoxSize = new Vector2(Mathf.Max(0.01f, hitBoxSize.x), Mathf.Max(0.01f, hitBoxSize.y));
        touchDamage = Mathf.Max(1, touchDamage);
        bulletSpeed = Mathf.Max(0f, bulletSpeed);
        bulletLifeTime = Mathf.Max(0.01f, bulletLifeTime);
        shotInterval = Mathf.Max(0.01f, shotInterval);
        shotIntervalRandom = Mathf.Max(0f, shotIntervalRandom);
        initialShotDelay = Mathf.Max(0f, initialShotDelay);
    }

    protected void CacheComponents()
    {
        if (hitBox == null)
        {
            hitBox = GetComponent<BoxCollider2D>();
        }
    }

    protected void ApplyColliderSettings()
    {
        CacheComponents();

        if (hitBox == null)
        {
            return;
        }

        hitBox.isTrigger = true;
        if (autoConfigureHitBox)
        {
            hitBox.size = hitBoxSize;
            hitBox.offset = hitBoxOffset;
        }
    }

    protected void SpawnDeathAnimation()
    {
        if (deathAnimationPrefab == null)
        {
            return;
        }

        GameObject effect = Instantiate(deathAnimationPrefab, transform.position, transform.rotation);
        if (deathAnimationLifeTime > 0f)
        {
            Destroy(effect, deathAnimationLifeTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = muzzlePoint != null
            ? muzzlePoint.position
            : transform.position + new Vector3(muzzleOffset.x, muzzleOffset.y, 0f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + (Vector3)(AngleToDirection(shotAngle) * 1.2f));
    }

    protected static Vector2 AngleToDirection(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;
    }
}

// 功能：集中提供当前摄像机、玩家位置和屏幕坐标换算。
// 技术要点：LevelScrollManager 写入当前场景引用，其他脚本只读取；缺引用时退回 Camera.main / M18Player。
// 配置：无。
// 版本：0.1.0
public static class GameWorldContext
{
    private static Camera currentCamera;
    private static Transform currentPlayer;

    public static Camera CurrentCamera
    {
        get
        {
            if (currentCamera == null)
            {
                currentCamera = Camera.main;
            }

            return currentCamera;
        }
    }

    public static Transform CurrentPlayer
    {
        get
        {
            if (currentPlayer == null)
            {
                M18Player foundPlayer = Object.FindObjectOfType<M18Player>();
                if (foundPlayer != null)
                {
                    currentPlayer = foundPlayer.transform;
                }
            }

            return currentPlayer;
        }
    }

    public static float CameraLeftEdgeX
    {
        get { return ViewportToWorld(new Vector2(0f, 0.5f), 0f).x; }
    }

    public static float CameraRightEdgeX
    {
        get { return ViewportToWorld(new Vector2(1f, 0.5f), 0f).x; }
    }

    public static float PlayerX
    {
        get
        {
            Transform player = CurrentPlayer;
            return player != null ? player.position.x : float.NegativeInfinity;
        }
    }

    public static void SetCamera(Camera camera)
    {
        if (camera != null)
        {
            currentCamera = camera;
        }
    }

    public static void SetPlayer(Transform player)
    {
        if (player != null)
        {
            currentPlayer = player;
        }
    }

    public static Vector3 ViewportToWorld(Vector2 viewportPosition, float worldZ)
    {
        Camera camera = CurrentCamera;
        if (camera == null)
        {
            return new Vector3(viewportPosition.x, viewportPosition.y, worldZ);
        }

        float distanceFromCamera = Mathf.Abs(worldZ - camera.transform.position.z);
        Vector3 worldPosition = camera.ViewportToWorldPoint(new Vector3(viewportPosition.x, viewportPosition.y, distanceFromCamera));
        worldPosition.z = worldZ;
        return worldPosition;
    }
}
