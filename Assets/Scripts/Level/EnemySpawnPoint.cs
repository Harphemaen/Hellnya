// 功能：敌人出生点，当摄像机右边缘到达触发 X 坐标时生成配置好的敌人或敌人组。
// 技术要点：挂在场景空对象上作为关卡开关；每个条目可配置敌人 Prefab、数量、队形偏移、生成间隔和初始移动速度覆盖。
// 版本：v0.1.0

using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemySpawnPoint : MonoBehaviour
{
    public enum TriggerSource
    {
        CameraRightEdge,
        PlayerX
    }

    [System.Serializable]
    public sealed class EnemySpawnEntry
    {
        [SerializeField] private EnemyBase enemyPrefab;
        [SerializeField] private int count = 1;
        [SerializeField] private Vector2 firstSpawnOffset = Vector2.zero;
        [SerializeField] private Vector2 spacing = new Vector2(0f, -0.6f);
        [SerializeField] private float spawnInterval = 0f;
        [SerializeField] private bool overrideMoveSpeed;
        [SerializeField] private float moveSpeed = 1.2f;

        public EnemyBase EnemyPrefab
        {
            get { return enemyPrefab; }
        }

        public int Count
        {
            get { return Mathf.Max(1, count); }
        }

        public Vector2 FirstSpawnOffset
        {
            get { return firstSpawnOffset; }
        }

        public Vector2 Spacing
        {
            get { return spacing; }
        }

        public float SpawnInterval
        {
            get { return Mathf.Max(0f, spawnInterval); }
        }

        public bool OverrideMoveSpeed
        {
            get { return overrideMoveSpeed; }
        }

        public float MoveSpeed
        {
            get { return Mathf.Max(0f, moveSpeed); }
        }

    }

    [Header("Trigger")]
    [SerializeField] private TriggerSource triggerSource;
    [SerializeField] private bool useTransformXAsTrigger = true;
    [SerializeField] private float triggerX;

    [Header("Spawn")]
    [SerializeField] private Transform spawnParent;
    [SerializeField] private EnemySpawnEntry[] enemies = new EnemySpawnEntry[] { new EnemySpawnEntry() };

    private bool hasTriggered;
    private Coroutine spawnRoutine;

    public float TriggerX
    {
        get { return useTransformXAsTrigger ? transform.position.x : triggerX; }
    }

    public bool HasTriggered
    {
        get { return hasTriggered; }
    }

    private void Reset()
    {
        triggerX = transform.position.x;
        enemies = new EnemySpawnEntry[] { new EnemySpawnEntry() };
    }

    private void OnValidate()
    {
        if (useTransformXAsTrigger)
        {
            triggerX = transform.position.x;
        }
    }

    public void ResetTrigger()
    {
        hasTriggered = false;

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    public bool TryTrigger(float cameraRightEdgeX)
    {
        return TryTrigger(cameraRightEdgeX, float.NegativeInfinity);
    }

    public bool TryTrigger(float cameraRightEdgeX, float playerX)
    {
        if (hasTriggered)
        {
            return false;
        }

        if (GetTriggerValue(cameraRightEdgeX, playerX) < TriggerX)
        {
            return false;
        }

        Trigger();
        return true;
    }

    private float GetTriggerValue(float cameraRightEdgeX, float playerX)
    {
        return triggerSource == TriggerSource.PlayerX ? playerX : cameraRightEdgeX;
    }

    public void Trigger()
    {
        if (hasTriggered)
        {
            return;
        }

        hasTriggered = true;

        if (!Application.isPlaying)
        {
            return;
        }

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }

        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        if (enemies == null)
        {
            yield break;
        }

        for (int entryIndex = 0; entryIndex < enemies.Length; entryIndex++)
        {
            EnemySpawnEntry entry = enemies[entryIndex];
            if (entry == null)
            {
                continue;
            }

            for (int enemyIndex = 0; enemyIndex < entry.Count; enemyIndex++)
            {
                SpawnOne(entry, enemyIndex);

                if (entry.SpawnInterval > 0f)
                {
                    yield return new WaitForSeconds(entry.SpawnInterval);
                }
            }
        }

        spawnRoutine = null;
    }

    private void SpawnOne(EnemySpawnEntry entry, int enemyIndex)
    {
        Vector2 localOffset = entry.FirstSpawnOffset + entry.Spacing * enemyIndex;
        Vector3 spawnPosition = transform.position + new Vector3(localOffset.x, localOffset.y, 0f);
        Transform parent = spawnParent != null ? spawnParent : null;
        EnemyBase enemy = CreateEnemy(entry.EnemyPrefab, spawnPosition, parent);

        if (entry.OverrideMoveSpeed)
        {
            enemy.SetMoveSpeed(entry.MoveSpeed);
        }
    }

    private EnemyBase CreateEnemy(EnemyBase prefab, Vector3 spawnPosition, Transform parent)
    {
        if (prefab != null)
        {
            return Instantiate(prefab, spawnPosition, Quaternion.identity, parent);
        }

        GameObject enemyObject = new GameObject("Enemy_Spawned");
        enemyObject.transform.position = spawnPosition;

        if (parent != null)
        {
            enemyObject.transform.SetParent(parent);
        }

        enemyObject.AddComponent<SpriteRenderer>();
        enemyObject.AddComponent<BoxCollider2D>();
        return enemyObject.AddComponent<EnemyBase>();
    }

    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position;
        Vector3 triggerTop = new Vector3(TriggerX, origin.y + 1.5f, origin.z);
        Vector3 triggerBottom = new Vector3(TriggerX, origin.y - 1.5f, origin.z);

        Gizmos.color = hasTriggered ? Color.gray : Color.yellow;
        Gizmos.DrawLine(triggerBottom, triggerTop);
        Gizmos.DrawWireSphere(origin, 0.18f);
    }
}
