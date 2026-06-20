using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class SimpleEffectAnimator : MonoBehaviour
{
    [SerializeField] private Color color = Color.white;
    [SerializeField] private Vector2 startScale = new Vector2(0.2f, 0.2f);
    [SerializeField] private Vector2 endScale = new Vector2(1f, 1f);
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private int sortingOrder = 80;

    private static Sprite sharedSprite;
    private SpriteRenderer spriteRenderer;
    private float timer;

    private void Awake()
    {
        CacheRenderer();
        ApplyFrame(0f);
    }

    private void OnEnable()
    {
        timer = 0f;
        CacheRenderer();
        ApplyFrame(0f);
    }

    private void Update()
    {
        float safeDuration = Mathf.Max(0.01f, duration);
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / safeDuration);
        ApplyFrame(t);

        if (rotationSpeed != 0f)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }

        if (timer >= safeDuration)
        {
            Destroy(gameObject);
        }
    }

    private void CacheRenderer()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        if (spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = GetSharedSprite();
        }

        spriteRenderer.sortingOrder = sortingOrder;
    }

    private void ApplyFrame(float t)
    {
        Vector2 scale = Vector2.Lerp(startScale, endScale, 1f - Mathf.Pow(1f - t, 2f));
        transform.localScale = new Vector3(scale.x, scale.y, 1f);

        if (spriteRenderer == null)
        {
            return;
        }

        Color nextColor = color;
        nextColor.a *= 1f - t;
        spriteRenderer.color = nextColor;
    }

    private static Sprite GetSharedSprite()
    {
        if (sharedSprite != null)
        {
            return sharedSprite;
        }

        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        sharedSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        sharedSprite.hideFlags = HideFlags.HideAndDontSave;
        return sharedSprite;
    }
}
