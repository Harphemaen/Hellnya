// 功能：敌人子弹的基础飞行与生命周期控制。
// 技术要点：由敌人脚本生成并初始化方向、速度、生命周期和外观；编辑模式下可直接看到占位图。
// 版本：v0.1.0

using UnityEngine;

[ExecuteAlways]
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
    [SerializeField] private Vector2 placeholderSize = new Vector2(0.18f, 0.08f);
    [SerializeField] private Color placeholderColor = new Color(1f, 0.35f, 0.2f, 1f);
    [SerializeField] private int sortingOrder = 15;

    private static Sprite sharedPlaceholderSprite;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D hitBox;
    private Rigidbody2D body;
    private float lifeTimer;
#if UNITY_EDITOR
    private bool editorRefreshQueued;
#endif

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
        QueueEditorVisualRefresh();
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

    public void Init(Vector2 newDirection, float newSpeed, float newLifeTime, Sprite newBulletSprite, Vector2 newPlaceholderSize, Color newPlaceholderColor, int newSortingOrder)
    {
        direction = newDirection.sqrMagnitude > 0.0001f ? newDirection.normalized : Vector2.left;
        speed = Mathf.Max(0f, newSpeed);
        lifeTime = Mathf.Max(0.01f, newLifeTime);
        bulletSprite = newBulletSprite;
        placeholderSize = new Vector2(Mathf.Max(0.01f, newPlaceholderSize.x), Mathf.Max(0.01f, newPlaceholderSize.y));
        placeholderColor = newPlaceholderColor;
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
        placeholderSize = new Vector2(Mathf.Max(0.01f, placeholderSize.x), Mathf.Max(0.01f, placeholderSize.y));
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

        if (bulletSprite != null)
        {
            spriteRenderer.sprite = bulletSprite;
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

    private void FaceMoveDirection()
    {
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void QueueEditorVisualRefresh()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            ApplyVisualDefaults();
            return;
        }

        if (editorRefreshQueued)
        {
            return;
        }

        editorRefreshQueued = true;
        UnityEditor.EditorApplication.delayCall += ApplyEditorVisualRefresh;
#endif
    }

#if UNITY_EDITOR
    private void ApplyEditorVisualRefresh()
    {
        editorRefreshQueued = false;

        if (this == null)
        {
            return;
        }

        ApplyVisualDefaults();
    }
#endif

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
