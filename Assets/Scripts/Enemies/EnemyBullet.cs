// 功能：敌人子弹的飞行、命中销毁动画和自然生命周期。
// 技术要点：外观交给对象自己的 SpriteRenderer；自然超时销毁不播放动画，命中玩家时才播放。
// 配置：direction 初始方向；speed 速度；lifeTime 存活时间；deathAnimationPrefab 命中动画；deathAnimationLifeTime 动画对象自动销毁时间。
// 版本：0.2.0

using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBullet : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] protected Vector2 direction = Vector2.left;
    [SerializeField] protected float speed = 4f;
    [SerializeField] protected float lifeTime = 4f;

    [Header("Death Animation")]
    [SerializeField] private GameObject deathAnimationPrefab;
    [SerializeField] private float deathAnimationLifeTime = 1f;
    [SerializeField] private LayerMask terrainLayers;

    private BoxCollider2D hitBox;
    private Rigidbody2D body;
    protected float lifeTimer;
    private bool hasHit;

    protected virtual void Awake()
    {
        NormalizeSettings();
        CacheComponents();
        FaceMoveDirection();
    }

    protected virtual void Reset()
    {
        NormalizeSettings();
        CacheComponents();
    }

    protected virtual void OnEnable()
    {
        CacheComponents();
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

        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsTerrain(other))
        {
            DestroyByHit();
            return;
        }

        M18Player player = other.GetComponent<M18Player>();
        if (player == null)
        {
            return;
        }

        if (DestroyByHit())
        {
            player.TakeDamage(1);
        }
    }

    public void Init(Vector2 newDirection, float newSpeed, float newLifeTime)
    {
        direction = newDirection.sqrMagnitude > 0.0001f ? newDirection.normalized : Vector2.left;
        speed = Mathf.Max(0f, newSpeed);
        lifeTime = Mathf.Max(0.01f, newLifeTime);
        lifeTimer = 0f;
        hasHit = false;
        FaceMoveDirection();
    }

    public bool DestroyByHit()
    {
        if (hasHit)
        {
            return false;
        }

        hasHit = true;
        SpawnDeathAnimation();
        Destroy(gameObject);
        return true;
    }

    protected void NormalizeSettings()
    {
        direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.left;
        speed = Mathf.Max(0f, speed);
        lifeTime = Mathf.Max(0.01f, lifeTime);
        deathAnimationLifeTime = Mathf.Max(0f, deathAnimationLifeTime);

        if (terrainLayers.value == 0)
        {
            terrainLayers = LayerMask.GetMask("Terrain");
        }
    }

    protected void CacheComponents()
    {
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

    protected void FaceMoveDirection()
    {
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void SpawnDeathAnimation()
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

    private bool IsTerrain(Collider2D other)
    {
        return other != null && (terrainLayers.value & (1 << other.gameObject.layer)) != 0;
    }
}
