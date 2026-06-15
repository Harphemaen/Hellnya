// 功能：M18 地面玩家的移动、跳跃、屏幕限制和可调角度射击控制。
// 技术要点：一个玩家对象挂一个脚本；输入使用新版 Input System 直接读取键盘；射击支持主路角度调节和道具增加额外弹路；外观和参数通过 Inspector 配置并在编辑器中实时显示。
// 版本：v0.4.0

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class M18Player : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private Key leftKey = Key.LeftArrow;
    [SerializeField] private Key rightKey = Key.RightArrow;
    [SerializeField] private Key aimUpKey = Key.UpArrow;
    [SerializeField] private Key aimDownKey = Key.DownArrow;
    [SerializeField] private Key jumpButton = Key.X;
    [SerializeField] private Key attackKey = Key.Z;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Vector2 viewportMin = new Vector2(0.05f, 0.1f);
    [SerializeField] private Vector2 viewportMax = new Vector2(0.95f, 0.6f);

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 1.8f;
    [SerializeField] private float airTime = 0.28f;
    [SerializeField] private float fallSpeed = 7.5f;
    [SerializeField] private bool lockGroundYOnStart = true;
    [SerializeField] private float groundY;

    [Header("Shot")]
    [SerializeField] private M18Bullet bulletPrefab;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private Vector2 muzzleOffset = new Vector2(0.75f, 0.1f);
    [SerializeField] private float shotsPerSecond = 6f;
    [SerializeField] private float bulletSpeed = 8.5f;
    [SerializeField] private float bulletLifeTime = 1.2f;
    [SerializeField] private float baseShotAngle = 0f;
    [SerializeField] private float aimAngleStep = 30f;
    [SerializeField] private float primaryMaxAngle = 60f;
    [SerializeField] private int extraBulletWays;
    [SerializeField] private int maxExtraBulletWays = 1;
    [SerializeField] private float extraWayAngleOffset = 30f;
    [SerializeField] private float extraMaxAngle = 90f;

    [Header("Visual")]
    [SerializeField] private Sprite playerSprite;
    [SerializeField] private bool createPlaceholderWhenSpriteMissing = true;
    [SerializeField] private Vector2 placeholderSize = new Vector2(1.25f, 0.75f);
    [SerializeField] private Color placeholderColor = new Color(0.15f, 0.8f, 0.95f, 1f);
    [SerializeField] private int sortingOrder = 10;

    private static Sprite sharedPlaceholderSprite;
    private SpriteRenderer spriteRenderer;
    private float verticalSpeed;
    private float nextFireTime;
    private float currentShotAngle;
    private bool isGrounded = true;
    private bool keyboardMissingWarningShown;
#if UNITY_EDITOR
    private bool editorRefreshQueued;
#endif

    private void Awake()
    {
        NormalizeSettings();
        CacheComponents();
        ApplyVisualDefaults();

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (Application.isPlaying && lockGroundYOnStart)
        {
            groundY = transform.position.y;
        }
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

        Keyboard keyboard = Keyboard.current ?? InputSystem.GetDevice<Keyboard>();
        if (keyboard == null)
        {
            LogMissingKeyboardWarningOnce();
            return;
        }

        float horizontal = ReadHorizontalInput(keyboard);
        bool jumpPressed = WasPressedThisFrame(keyboard, jumpButton);
        bool attackHeld = IsPressed(keyboard, attackKey);

        UpdateShotAngle(keyboard);
        Move(horizontal, jumpPressed);

        if (attackHeld)
        {
            TryFire();
        }
    }

    private float ReadHorizontalInput(Keyboard keyboard)
    {
        float horizontal = 0f;

        if (IsPressed(keyboard, leftKey))
        {
            horizontal -= 1f;
        }

        if (IsPressed(keyboard, rightKey))
        {
            horizontal += 1f;
        }

        return Mathf.Clamp(horizontal, -1f, 1f);
    }

    private void UpdateShotAngle(Keyboard keyboard)
    {
        if (WasPressedThisFrame(keyboard, aimUpKey))
        {
            currentShotAngle = Mathf.Min(currentShotAngle + aimAngleStep, GetPrimaryMaxAngle());
        }

        if (WasPressedThisFrame(keyboard, aimDownKey))
        {
            currentShotAngle = Mathf.Max(currentShotAngle - aimAngleStep, baseShotAngle);
        }
    }

    private void Move(float horizontal, bool jumpPressed)
    {
        if (jumpPressed && isGrounded)
        {
            isGrounded = false;
            verticalSpeed = GetJumpStartSpeed();
        }

        Vector3 nextPosition = transform.position;
        nextPosition.x += horizontal * moveSpeed * Time.deltaTime;

        if (isGrounded)
        {
            nextPosition.y = groundY;
        }
        else
        {
            if (verticalSpeed > 0f)
            {
                verticalSpeed += GetJumpGravity() * Time.deltaTime;
            }

            if (verticalSpeed <= 0f)
            {
                verticalSpeed = -Mathf.Max(0.01f, fallSpeed);
            }

            nextPosition.y += verticalSpeed * Time.deltaTime;
            if (nextPosition.y <= groundY)
            {
                nextPosition.y = groundY;
                verticalSpeed = 0f;
                isGrounded = true;
            }
        }

        ClampInsideCamera(ref nextPosition);
        transform.position = nextPosition;
    }

    private void ClampInsideCamera(ref Vector3 position)
    {
        Camera cameraToUse = targetCamera != null ? targetCamera : Camera.main;
        if (cameraToUse == null)
        {
            return;
        }

        float distanceFromCamera = Mathf.Abs(position.z - cameraToUse.transform.position.z);
        Vector3 min = cameraToUse.ViewportToWorldPoint(new Vector3(viewportMin.x, viewportMin.y, distanceFromCamera));
        Vector3 max = cameraToUse.ViewportToWorldPoint(new Vector3(viewportMax.x, viewportMax.y, distanceFromCamera));
        Vector3 extents = spriteRenderer != null ? spriteRenderer.bounds.extents : Vector3.zero;

        float minX = Mathf.Min(min.x, max.x) + extents.x;
        float maxX = Mathf.Max(min.x, max.x) - extents.x;
        float minY = Mathf.Min(min.y, max.y) + extents.y;
        float maxY = Mathf.Max(min.y, max.y) - extents.y;

        position.x = ClampAxis(position.x, minX, maxX);
        float unclampedY = position.y;
        position.y = ClampAxis(position.y, minY, maxY);

        if (position.y < unclampedY && verticalSpeed > 0f)
        {
            verticalSpeed = 0f;
        }
    }

    private void TryFire()
    {
        if (Time.time < nextFireTime)
        {
            return;
        }

        Fire();
        nextFireTime = Time.time + 1f / Mathf.Max(0.01f, shotsPerSecond);
    }

    private void Fire()
    {
        Vector3 spawnPosition = muzzlePoint != null
            ? muzzlePoint.position
            : transform.position + new Vector3(muzzleOffset.x, muzzleOffset.y, 0f);

        FireBullet(spawnPosition, currentShotAngle);

        for (int i = 1; i <= extraBulletWays; i++)
        {
            float extraAngle = Mathf.Min(currentShotAngle + extraWayAngleOffset * i, GetExtraMaxAngle());
            FireBullet(spawnPosition, extraAngle);
        }
    }

    private void FireBullet(Vector3 spawnPosition, float angle)
    {
        Vector2 direction = AngleToDirection(angle);
        M18Bullet bullet = CreateBullet(spawnPosition);
        bullet.Init(direction, bulletSpeed, bulletLifeTime);
    }

    private M18Bullet CreateBullet(Vector3 spawnPosition)
    {
        if (bulletPrefab != null)
        {
            return Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        }

        GameObject bulletObject = new GameObject("M18_Bullet");
        bulletObject.transform.position = spawnPosition;
        return bulletObject.AddComponent<M18Bullet>();
    }

    public void AddBulletWay(int amount = 1)
    {
        SetExtraBulletWays(extraBulletWays + amount);
    }

    public void SetExtraBulletWays(int count)
    {
        extraBulletWays = Mathf.Clamp(count, 0, maxExtraBulletWays);
    }

    private float GetJumpStartSpeed()
    {
        float safeAirTime = Mathf.Max(0.01f, airTime);
        return 2f * Mathf.Max(0.01f, jumpHeight) / safeAirTime;
    }

    private float GetJumpGravity()
    {
        float safeAirTime = Mathf.Max(0.01f, airTime);
        return -(2f * Mathf.Max(0.01f, jumpHeight)) / (safeAirTime * safeAirTime);
    }

    private void NormalizeSettings()
    {
        moveSpeed = Mathf.Max(0f, moveSpeed);
        jumpHeight = Mathf.Max(0.01f, jumpHeight);
        airTime = Mathf.Max(0.01f, airTime);
        fallSpeed = Mathf.Max(0.01f, fallSpeed);
        shotsPerSecond = Mathf.Max(0.01f, shotsPerSecond);
        bulletSpeed = Mathf.Max(0f, bulletSpeed);
        bulletLifeTime = Mathf.Max(0.01f, bulletLifeTime);
        aimAngleStep = Mathf.Max(0.01f, aimAngleStep);
        primaryMaxAngle = Mathf.Max(0f, primaryMaxAngle);
        maxExtraBulletWays = Mathf.Max(0, maxExtraBulletWays);
        extraBulletWays = Mathf.Clamp(extraBulletWays, 0, maxExtraBulletWays);
        extraWayAngleOffset = Mathf.Max(0f, extraWayAngleOffset);
        extraMaxAngle = Mathf.Max(primaryMaxAngle, extraMaxAngle);
        currentShotAngle = Mathf.Clamp(currentShotAngle, baseShotAngle, GetPrimaryMaxAngle());
        placeholderSize = new Vector2(Mathf.Max(0.01f, placeholderSize.x), Mathf.Max(0.01f, placeholderSize.y));
    }

    private float GetPrimaryMaxAngle()
    {
        return baseShotAngle + primaryMaxAngle;
    }

    private float GetExtraMaxAngle()
    {
        return baseShotAngle + extraMaxAngle;
    }

    private void CacheComponents()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void ApplyVisualDefaults()
    {
        CacheComponents();

        if (spriteRenderer == null)
        {
            return;
        }

        if (playerSprite != null)
        {
            spriteRenderer.sprite = playerSprite;
            spriteRenderer.color = Color.white;
        }
        else if (createPlaceholderWhenSpriteMissing)
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

    private void LogMissingKeyboardWarningOnce()
    {
        if (keyboardMissingWarningShown)
        {
            return;
        }

        keyboardMissingWarningShown = true;
        Debug.LogWarning("M18Player could not find a Keyboard device. Click the Game view once, or restart Unity after changing Input System settings.", this);
    }

    private static bool IsPressed(Keyboard keyboard, Key key)
    {
        KeyControl control = GetKeyControl(keyboard, key);
        return control != null && control.isPressed;
    }

    private static bool WasPressedThisFrame(Keyboard keyboard, Key key)
    {
        KeyControl control = GetKeyControl(keyboard, key);
        return control != null && control.wasPressedThisFrame;
    }

    private static KeyControl GetKeyControl(Keyboard keyboard, Key key)
    {
        if (keyboard == null || key == Key.None)
        {
            return null;
        }

        return keyboard[key];
    }

    private static Vector2 AngleToDirection(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;
    }

    private static float ClampAxis(float value, float min, float max)
    {
        if (min > max)
        {
            return (min + max) * 0.5f;
        }

        return Mathf.Clamp(value, min, max);
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
