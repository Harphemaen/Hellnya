// 功能：敌人子弹的基础飞行与生命周期控制。
// 技术要点：由敌人脚本生成并初始化方向、速度、生命周期和外观；编辑模式下可直接看到占位图。
// 配置：direction 初始方向；speed 速度；lifeTime 存活时间；bulletSprite 外观；sortingOrder 层级。
// 版本：v0.1.0

using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBullet : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private Vector2 direction = Vector2.left;
    [SerializeField] private float speed = 4f;
    [SerializeField] private float lifeTime = 4f;

    [Header("Visual")]
    [SerializeField] private Sprite bulletSprite;
    [SerializeField] private int sortingOrder = 15;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D hitBox;
    private Rigidbody2D body;
    private float lifeTimer;

    private void Awake()
    {
        NormalizeSettings();
        CacheComponents();
        ApplyVisualDefaults();
        FaceMoveDirection();
    }

    private void Reset()
    {
        NormalizeSettings();
        CacheComponents();
        ApplyVisualDefaults();
    }

    private void OnEnable()
    {
        CacheComponents();
        ApplyVisualDefaults();
    }

    private void OnValidate()
    {
        NormalizeSettings();
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    public void Init(Vector2 newDirection, float newSpeed, float newLifeTime, Sprite newBulletSprite, int newSortingOrder)
    {
        direction = newDirection.sqrMagnitude > 0.0001f ? newDirection.normalized : Vector2.left;
        speed = Mathf.Max(0f, newSpeed);
        lifeTime = Mathf.Max(0.01f, newLifeTime);
        bulletSprite = newBulletSprite;
        sortingOrder = newSortingOrder;
        lifeTimer = 0f;
        ApplyVisualDefaults();
        FaceMoveDirection();
    }

    private void NormalizeSettings()
    {
        direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.left;
        speed = Mathf.Max(0f, speed);
        lifeTime = Mathf.Max(0.01f, lifeTime);
    }

    private void CacheComponents()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        if (hitBox == null)
        {
            hitBox = GetComponent<BoxCollider2D>();
        }

        if (hitBox == null)
        {
            hitBox = gameObject.AddComponent<BoxCollider2D>();
        }

        hitBox.isTrigger = true;

        if (body == null)
        {
            body = GetComponent<Rigidbody2D>();
        }

        if (body == null)
        {
            body = gameObject.AddComponent<Rigidbody2D>();
        }

        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
    }

    private void ApplyVisualDefaults()
    {
        CacheComponents();

        spriteRenderer.sprite = bulletSprite;
        spriteRenderer.color = Color.white;
        spriteRenderer.sortingOrder = sortingOrder;
    }

    private void FaceMoveDirection()
    {
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

}
