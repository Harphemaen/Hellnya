// 功能：关卡卷轴管理器，控制摄像机沿 X 正方向匀速移动，并按摄像机右边缘触发敌人出生点。
// 技术要点：挂在独立 LevelManager 对象上；摄像机移动早于玩家 Update，玩家继续使用摄像机视口限制活动范围；出生点可自动查找或手动配置。
// 配置：targetCamera 摄像机；moveCamera 是否推进；setCameraToLevelStartOnPlay 是否开局放到起点；levelStartX/EndX 关卡范围；scrollSpeed 卷轴速度；stopAtLevelEnd 是否停在终点；spawnPoints 出生点；player 玩家引用。
// 版本：v0.1.0

using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
[DisallowMultipleComponent]
public class LevelScrollManager : MonoBehaviour
{
    [Header("Camera Scroll")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private bool moveCamera = true;
    [SerializeField] private bool setCameraToLevelStartOnPlay = true;
    [SerializeField] private float levelStartX = 0f;
    [SerializeField] private float levelEndX = 120f;
    [SerializeField] private float scrollSpeed = 1.2f;
    [SerializeField] private bool stopAtLevelEnd = true;

    [Header("Spawn Points")]
    [SerializeField] private bool autoFindSpawnPoints = true;
    [SerializeField] private EnemySpawnPoint[] spawnPoints = new EnemySpawnPoint[0];
    [SerializeField] private Transform player;

    public float CameraRightEdgeX
    {
        get { return GameWorldContext.CameraRightEdgeX; }
    }

    private void Awake()
    {
        NormalizeSettings();
        ResolveCamera();
        GameWorldContext.SetCamera(targetCamera);
    }

    private void Start()
    {
        ResolveCamera();

        if (Application.isPlaying && targetCamera != null && setCameraToLevelStartOnPlay)
        {
            Vector3 position = targetCamera.transform.position;
            position.x = levelStartX;
            targetCamera.transform.position = position;
        }

        if (autoFindSpawnPoints)
        {
            RefreshSpawnPoints();
        }

        ResolvePlayer();
        GameWorldContext.SetPlayer(player);
        ResetSpawnPoints();
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

        MoveCameraForward();
        TryTriggerSpawnPoints();
    }

    public void Configure(Camera newTargetCamera, float newScrollSpeed, float newLevelStartX, float newLevelEndX)
    {
        targetCamera = newTargetCamera;
        scrollSpeed = newScrollSpeed;
        levelStartX = newLevelStartX;
        levelEndX = newLevelEndX;
        NormalizeSettings();
    }

    public void RefreshSpawnPoints()
    {
        spawnPoints = FindObjectsOfType<EnemySpawnPoint>();
        Array.Sort(spawnPoints, CompareSpawnPointByTriggerX);
    }

    private void MoveCameraForward()
    {
        if (!moveCamera || targetCamera == null)
        {
            return;
        }

        Vector3 position = targetCamera.transform.position;
        position.x += scrollSpeed * Time.deltaTime;

        if (stopAtLevelEnd)
        {
            position.x = Mathf.Min(position.x, levelEndX);
        }

        targetCamera.transform.position = position;
    }

    private void TryTriggerSpawnPoints()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return;
        }

        ResolvePlayer();
        GameWorldContext.SetCamera(targetCamera);
        GameWorldContext.SetPlayer(player);
        float rightEdgeX = GameWorldContext.CameraRightEdgeX;
        float playerX = GameWorldContext.PlayerX;
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
            {
                spawnPoints[i].TryTrigger(rightEdgeX, playerX);
            }
        }
    }

    private void ResetSpawnPoints()
    {
        if (spawnPoints == null)
        {
            return;
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
            {
                spawnPoints[i].ResetTrigger();
            }
        }
    }

    private void ResolveCamera()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        GameWorldContext.SetCamera(targetCamera);
    }

    private void ResolvePlayer()
    {
        if (player != null)
        {
            GameWorldContext.SetPlayer(player);
            return;
        }

        M18Player foundPlayer = FindObjectOfType<M18Player>();
        if (foundPlayer != null)
        {
            player = foundPlayer.transform;
        }

        GameWorldContext.SetPlayer(player);
    }

    private void NormalizeSettings()
    {
        scrollSpeed = Mathf.Max(0f, scrollSpeed);
        if (levelEndX < levelStartX)
        {
            levelEndX = levelStartX;
        }
    }

    private static int CompareSpawnPointByTriggerX(EnemySpawnPoint left, EnemySpawnPoint right)
    {
        if (left == right)
        {
            return 0;
        }

        if (left == null)
        {
            return 1;
        }

        if (right == null)
        {
            return -1;
        }

        return left.TriggerX.CompareTo(right.TriggerX);
    }
}
