// 功能：提供编辑器菜单，快速创建关卡卷轴管理器和敌人出生点测试对象。
// 技术要点：保持测试对象创建流程简单，生成后直接在 Inspector 调整参数并制作 Prefab 或关卡配置。
// 版本：v0.1.0

using UnityEditor;
using UnityEngine;

public static class LevelSceneSetupTools
{
    [MenuItem("SummerBlue/Test Scene/Create Level Scroll Manager")]
    public static void CreateLevelScrollManager()
    {
        GameObject managerObject = new GameObject("LevelScrollManager");
        Undo.RegisterCreatedObjectUndo(managerObject, "Create Level Scroll Manager");

        LevelScrollManager manager = Undo.AddComponent<LevelScrollManager>(managerObject);
        Camera mainCamera = Camera.main;
        float startX = mainCamera != null ? mainCamera.transform.position.x : 0f;
        manager.Configure(mainCamera, 1.2f, startX, startX + 120f);

        Selection.activeGameObject = managerObject;
        EditorGUIUtility.PingObject(managerObject);
    }

    [MenuItem("SummerBlue/Test Scene/Create Enemy Spawn Point")]
    public static void CreateEnemySpawnPoint()
    {
        GameObject spawnPoint = new GameObject("EnemySpawnPoint");
        Undo.RegisterCreatedObjectUndo(spawnPoint, "Create Enemy Spawn Point");

        spawnPoint.transform.position = GetDefaultSpawnPointPosition();
        Undo.AddComponent<EnemySpawnPoint>(spawnPoint);

        Selection.activeGameObject = spawnPoint;
        EditorGUIUtility.PingObject(spawnPoint);
    }

    private static Vector3 GetDefaultSpawnPointPosition()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return new Vector3(8f, 0f, 0f);
        }

        float distanceFromCamera = Mathf.Abs(0f - mainCamera.transform.position.z);
        Vector3 worldPosition = mainCamera.ViewportToWorldPoint(new Vector3(1.15f, 0.45f, distanceFromCamera));
        worldPosition.z = 0f;
        return worldPosition;
    }
}
