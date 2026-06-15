// 功能：单个背景块的可替换外观和可选横向演出移动。
// 技术要点：默认背景固定在关卡世界坐标中，由摄像机右移产生卷轴效果；如需独立演出移动，可在 Inspector 开启对象自移动。
// 版本：v0.4.0

using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class SimpleBgScroller : MonoBehaviour
{
    [Header("Scroll")]
    [SerializeField] private bool moveObjectInPlay;
    [SerializeField] private float scrollSpeed = 1f;
    [SerializeField] private float recycleLeftX = -20f;
    [SerializeField] private float recycleDistance = 40f;

    [Header("Visual")]
    [SerializeField] private Sprite backgroundSprite;
    [SerializeField] private Vector2 placeholderSize = new Vector2(20f, 5f);
    [SerializeField] private Color placeholderColor = Color.white;
    [SerializeField] private int sortingOrder = -20;

    private static Sprite sharedPlaceholderSprite;
    private SpriteRenderer spriteRenderer;
#if UNITY_EDITOR
    private bool editorRefreshQueued;
#endif

    private void Awake()
    {
        NormalizeSettings();
        CacheComponents();
        ApplyVisualDefaults();
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

        if (!moveObjectInPlay)
        {
            return;
        }

        transform.position += Vector3.left * scrollSpeed * Time.deltaTime;
        if (transform.position.x <= recycleLeftX)
        {
            transform.position += Vector3.right * recycleDistance;
        }
    }

    public void Configure(float newScrollSpeed, float newRecycleLeftX, float newRecycleDistance, Vector2 newSize, Color newColor, int newSortingOrder)
    {
        scrollSpeed = Mathf.Max(0f, newScrollSpeed);
        recycleLeftX = newRecycleLeftX;
        recycleDistance = Mathf.Max(0.01f, newRecycleDistance);
        placeholderSize = new Vector2(Mathf.Max(0.01f, newSize.x), Mathf.Max(0.01f, newSize.y));
        placeholderColor = newColor;
        sortingOrder = newSortingOrder;
        ApplyVisualDefaults();
    }

    private void NormalizeSettings()
    {
        scrollSpeed = Mathf.Max(0f, scrollSpeed);
        recycleDistance = Mathf.Max(0.01f, recycleDistance);
        placeholderSize = new Vector2(Mathf.Max(0.01f, placeholderSize.x), Mathf.Max(0.01f, placeholderSize.y));
    }

    private void CacheComponents()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    public void ApplyVisualDefaults()
    {
        CacheComponents();

        if (spriteRenderer == null)
        {
            return;
        }

        if (backgroundSprite != null)
        {
            spriteRenderer.sprite = backgroundSprite;
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
