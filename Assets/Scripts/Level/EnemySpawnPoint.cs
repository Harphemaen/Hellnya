// 功能：敌人出生点，当摄像机右边缘到达触发 X 坐标时生成配置好的敌人或敌人组。
// 技术要点：挂在场景空对象上作为关卡开关；生成器只负责延迟、数量和队列，敌人行为由 Prefab 自己的脚本决定。
// 配置：triggerSource 用摄像机右缘或玩家 X 触发；useTransformXAsTrigger/triggerX 触发坐标；spawnDelay 触发后的生成延迟；spawnInterval 队列中每个敌人的间隔；spawnParent 生成父节点；enemies 配置 Prefab 和数量。
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

        public EnemyBase EnemyPrefab
        {
            get { return enemyPrefab; }
        }

        public int Count
        {
            get { return Mathf.Max(1, count); }
        }

    }

    [Header("Trigger")]
    [SerializeField] private TriggerSource triggerSource;
    [SerializeField] private bool useTransformXAsTrigger = true;
    [SerializeField] private float triggerX;

    [Header("Spawn")]
    [SerializeField] private float spawnDelay;
    [SerializeField] private float spawnInterval = 0.25f;
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
        float safeSpawnDelay = Mathf.Max(0f, spawnDelay);
        if (safeSpawnDelay > 0f)
        {
            yield return new WaitForSeconds(safeSpawnDelay);
        }

        if (enemies == null)
        {
            yield break;
        }

        bool spawnedAny = false;
        float safeSpawnInterval = Mathf.Max(0f, spawnInterval);
        for (int entryIndex = 0; entryIndex < enemies.Length; entryIndex++)
        {
            EnemySpawnEntry entry = enemies[entryIndex];
            if (entry == null)
            {
                continue;
            }

            for (int enemyIndex = 0; enemyIndex < entry.Count; enemyIndex++)
            {
                if (spawnedAny && safeSpawnInterval > 0f)
                {
                    yield return new WaitForSeconds(safeSpawnInterval);
                }

                SpawnOne(entry);
                spawnedAny = true;
            }
        }

        spawnRoutine = null;
    }

    private void SpawnOne(EnemySpawnEntry entry)
    {
        Transform parent = spawnParent != null ? spawnParent : null;
        CreateEnemy(entry.EnemyPrefab, transform.position, parent);
    }

    private void CreateEnemy(EnemyBase prefab, Vector3 spawnPosition, Transform parent)
    {
        if (prefab != null)
        {
            Instantiate(prefab, spawnPosition, Quaternion.identity, parent);
            return;
        }

        GameObject enemyObject = new GameObject("Enemy_Spawned");
        enemyObject.transform.position = spawnPosition;

        if (parent != null)
        {
            enemyObject.transform.SetParent(parent);
        }

        enemyObject.AddComponent<SpriteRenderer>();
        enemyObject.AddComponent<BoxCollider2D>();
        enemyObject.AddComponent<EnemyBase>();
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
