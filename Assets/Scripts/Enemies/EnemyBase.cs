// 功能：敌人基础类别，提供外观、血量、受击碰撞盒、简单左移、基础射击和出生点初始参数覆盖。
// 技术要点：可直接挂在敌人对象上制作 Prefab；后续特化敌人可以继承本类并覆盖移动或射击逻辑；出生点可调用公开方法设置初始移动速度。
// 版本：v0.2.0

using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class EnemyBase : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] protected Sprite enemySprite;
    [SerializeField] protected Vector2 placeholderSize = new Vector2(0.9f, 0.7f);
    [SerializeField] protected Color placeholderColor = new Color(0.9f, 0.25f, 0.25f, 1f);
    [SerializeField] protected int sortingOrder = 8;

    [Header("Health")]
    [SerializeField] protected int maxHealth = 1;
    [SerializeField] protected bool destroyWhenHealthZero = true;

    [Header("Move")]
    [SerializeField] protected float moveSpeed = 1.2f;

    [Header("Hit Box")]
    [SerializeField] protected bool autoConfigureHitBox = true;
    [SerializeField] protected Vector2 hitBoxSize = new Vector2(0.9f, 0.7f);
    [SerializeField] protected Vector2 hitBoxOffset = Vector2.zero;

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

    [Header("Shot Visual")]
    [SerializeField] protected Sprite bulletSprite;
    [SerializeField] protected Vector2 bulletPlaceholderSize = new Vector2(0.18f, 0.08f);
    [SerializeField] protected Color bulletPlaceholderColor = new Color(1f, 0.35f, 0.2f, 1f);
    [SerializeField] protected int bulletSortingOrder = 15;

    private static Sprite sharedPlaceholderSprite;
    protected SpriteRenderer spriteRenderer;
    protected BoxCollider2D hitBox;
    protected int currentHealth;
    protected float nextShotTime;

    protected virtual void Awake()
    {
        NormalizeSettings();
        CacheComponents();
        ApplyEditorVisibleSettings();

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
        ApplyEditorVisibleSettings();
    }

    protected virtual void OnEnable()
    {
        CacheComponents();
        ApplyEditorVisibleSettings();
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
        bullet.Init(
            AngleToDirection(shotAngle),
            bulletSpeed,
            bulletLifeTime,
            bulletSprite,
            bulletPlaceholderSize,
            bulletPlaceholderColor,
            bulletSortingOrder);
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
        M18Bullet playerBullet = other.GetComponent<M18Bullet>();
        if (playerBullet == null)
        {
            return;
        }

        Destroy(playerBullet.gameObject);
        TakeDamage(1);
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

    public virtual void SetMoveSpeed(float newMoveSpeed)
    {
        moveSpeed = Mathf.Max(0f, newMoveSpeed);
    }

    protected virtual void Die()
    {
        if (destroyWhenHealthZero)
        {
            Destroy(gameObject);
        }
    }

    protected void ScheduleNextShot(float delay)
    {
        nextShotTime = Time.time + Mathf.Max(0.01f, delay);
    }

    protected void NormalizeSettings()
    {
        placeholderSize = new Vector2(Mathf.Max(0.01f, placeholderSize.x), Mathf.Max(0.01f, placeholderSize.y));
        maxHealth = Mathf.Max(1, maxHealth);
        moveSpeed = Mathf.Max(0f, moveSpeed);
        hitBoxSize = new Vector2(Mathf.Max(0.01f, hitBoxSize.x), Mathf.Max(0.01f, hitBoxSize.y));
        bulletSpeed = Mathf.Max(0f, bulletSpeed);
        bulletLifeTime = Mathf.Max(0.01f, bulletLifeTime);
        shotInterval = Mathf.Max(0.01f, shotInterval);
        shotIntervalRandom = Mathf.Max(0f, shotIntervalRandom);
        initialShotDelay = Mathf.Max(0f, initialShotDelay);
        bulletPlaceholderSize = new Vector2(Mathf.Max(0.01f, bulletPlaceholderSize.x), Mathf.Max(0.01f, bulletPlaceholderSize.y));
    }

    protected void CacheComponents()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (hitBox == null)
        {
            hitBox = GetComponent<BoxCollider2D>();
        }
    }

    protected void ApplyEditorVisibleSettings()
    {
        CacheComponents();

        if (spriteRenderer != null)
        {
            if (enemySprite != null)
            {
                spriteRenderer.sprite = enemySprite;
                spriteRenderer.color = Color.white;
            }
            else
            {
                spriteRenderer.sprite = GetPlaceholderSprite();
                spriteRenderer.color = placeholderColor;
                transform.localScale = new Vector3(placeholderSize.x, placeholderSize.y, 1f);
            }

            spriteRenderer.sortingOrder = sortingOrder;
        }

        if (hitBox != null)
        {
            hitBox.isTrigger = true;

            if (autoConfigureHitBox)
            {
                hitBox.size = hitBoxSize;
                hitBox.offset = hitBoxOffset;
            }
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

    private static Sprite GetPlaceholderSprite()
    {
        if (sharedPlaceholderSprite != null)
        {
            return sharedPlaceholderSprite;
        }

        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        sharedPlaceholderSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        sharedPlaceholderSprite.hideFlags = HideFlags.HideAndDontSave;
        return sharedPlaceholderSprite;
    }
}
