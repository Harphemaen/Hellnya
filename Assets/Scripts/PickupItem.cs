using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class PickupItem : MonoBehaviour
{
    public enum PickupType
    {
        ExtraBulletWay,
        FireRateUp,
        ScreenDamage
    }

    [Header("Effect")]
    [SerializeField] private PickupType itemType = PickupType.ExtraBulletWay;
    [SerializeField] private float fireRateBonus = 2f;
    [SerializeField] private int screenDamage = 1;

    [Header("Move")]
    [SerializeField] private float flyUpSpeed = 2f;
    [SerializeField] private float flyUpSeconds = 0.35f;
    [SerializeField] private float fallSpeed = 1.5f;
    [SerializeField] private float destroyOutsideScreenPadding = 0.2f;

    [Header("Feedback")]
    [SerializeField] private AudioClip pickupSound = null;
    [SerializeField] [Range(0f, 1f)] private float pickupVolume = 1f;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] [Range(0f, 1f)] private float flashAlpha = 0.65f;
    [SerializeField] private float flashDuration = 0.12f;

    private BoxCollider2D hitBox;
    private Rigidbody2D body;
    private float timer;
    private bool pickedUp;

    private void Awake()
    {
        NormalizeSettings();
        CacheComponents();
    }

    private void Reset()
    {
        NormalizeSettings();
        CacheComponents();
    }

    private void OnValidate()
    {
        NormalizeSettings();
    }

    private void Update()
    {
        if (!Application.isPlaying || pickedUp)
        {
            return;
        }

        float verticalSpeed = timer < flyUpSeconds ? flyUpSpeed : -fallSpeed;
        transform.position += Vector3.up * verticalSpeed * Time.deltaTime;
        timer += Time.deltaTime;

        if (IsOutsideScreen())
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (pickedUp)
        {
            return;
        }

        M18Player player = other.GetComponent<M18Player>();
        if (player == null)
        {
            return;
        }

        pickedUp = true;
        ApplyTo(player);
        PlayPickupSound();
        Destroy(gameObject);
    }

    private void ApplyTo(M18Player player)
    {
        switch (itemType)
        {
            case PickupType.ExtraBulletWay:
                player.AddBulletWay(1);
                break;
            case PickupType.FireRateUp:
                player.AddShotsPerSecond(fireRateBonus);
                break;
            case PickupType.ScreenDamage:
                ScreenFlash.Play(flashColor, flashAlpha, flashDuration);
                DamageVisibleEnemies();
                break;
        }
    }

    private void DamageVisibleEnemies()
    {
        Camera camera = GameWorldContext.CurrentCamera;
        EnemyBase[] enemies = FindObjectsOfType<EnemyBase>();
        for (int i = 0; i < enemies.Length; i++)
        {
            EnemyBase enemy = enemies[i];
            if (enemy != null && IsInsideScreen(camera, enemy.transform.position))
            {
                enemy.TakeDamage(screenDamage);
            }
        }
    }

    private void PlayPickupSound()
    {
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupVolume);
        }
    }

    private bool IsOutsideScreen()
    {
        Camera camera = GameWorldContext.CurrentCamera;
        if (camera == null)
        {
            return false;
        }

        Vector3 viewportPosition = camera.WorldToViewportPoint(transform.position);
        float padding = Mathf.Max(0f, destroyOutsideScreenPadding);
        return viewportPosition.z < 0f
            || viewportPosition.x < -padding
            || viewportPosition.x > 1f + padding
            || viewportPosition.y < -padding
            || viewportPosition.y > 1f + padding;
    }

    private static bool IsInsideScreen(Camera camera, Vector3 worldPosition)
    {
        if (camera == null)
        {
            return true;
        }

        Vector3 viewportPosition = camera.WorldToViewportPoint(worldPosition);
        return viewportPosition.z >= 0f
            && viewportPosition.x >= 0f
            && viewportPosition.x <= 1f
            && viewportPosition.y >= 0f
            && viewportPosition.y <= 1f;
    }

    private void NormalizeSettings()
    {
        fireRateBonus = Mathf.Max(0f, fireRateBonus);
        screenDamage = Mathf.Max(1, screenDamage);
        flyUpSpeed = Mathf.Max(0f, flyUpSpeed);
        flyUpSeconds = Mathf.Max(0f, flyUpSeconds);
        fallSpeed = Mathf.Max(0f, fallSpeed);
        destroyOutsideScreenPadding = Mathf.Max(0f, destroyOutsideScreenPadding);
        pickupVolume = Mathf.Clamp01(pickupVolume);
        flashAlpha = Mathf.Clamp01(flashAlpha);
        flashDuration = Mathf.Max(0.01f, flashDuration);
    }

    private void CacheComponents()
    {
        if (hitBox == null)
        {
            hitBox = GetComponent<BoxCollider2D>();
        }

        if (hitBox != null)
        {
            hitBox.isTrigger = true;
        }

        if (body == null)
        {
            body = GetComponent<Rigidbody2D>();
        }

        if (body != null)
        {
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
        }
    }

    private sealed class ScreenFlash : MonoBehaviour
    {
        private const int SortingOrder = 32000;
        private static ScreenFlash runner;
        private static Sprite sharedSprite;

        public static void Play(Color color, float alpha, float duration)
        {
            if (runner == null)
            {
                GameObject runnerObject = new GameObject("ScreenFlashRunner");
                runnerObject.hideFlags = HideFlags.HideAndDontSave;
                runner = runnerObject.AddComponent<ScreenFlash>();
            }

            runner.StartCoroutine(runner.FlashRoutine(color, alpha, duration));
        }

        private IEnumerator FlashRoutine(Color color, float alpha, float duration)
        {
            Camera camera = GameWorldContext.CurrentCamera;
            if (camera == null)
            {
                yield break;
            }

            GameObject overlay = new GameObject("ScreenFlashOverlay");
            overlay.hideFlags = HideFlags.HideAndDontSave;
            overlay.transform.SetParent(camera.transform, false);
            overlay.transform.localPosition = new Vector3(0f, 0f, 1f);

            SpriteRenderer renderer = overlay.AddComponent<SpriteRenderer>();
            renderer.sprite = GetSharedSprite();
            renderer.sortingOrder = SortingOrder;

            float safeDuration = Mathf.Max(0.01f, duration);
            float timer = 0f;
            while (timer < safeDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / safeDuration);
                Color nextColor = color;
                nextColor.a = alpha * (1f - t);
                renderer.color = nextColor;
                FitToCamera(overlay.transform, camera);
                yield return null;
            }

            Destroy(overlay);
        }

        private static void FitToCamera(Transform target, Camera camera)
        {
            if (!camera.orthographic)
            {
                target.localScale = new Vector3(100f, 100f, 1f);
                return;
            }

            float height = camera.orthographicSize * 2f;
            target.localScale = new Vector3(height * camera.aspect, height, 1f);
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
}
