// 功能：提供编辑器菜单，快速创建可配置的基础敌人测试对象。
// 技术要点：只挂载 EnemyBase 一个核心行为脚本，生成后可直接在 Inspector 调整并制作 Prefab。
// 版本：v0.1.0

using UnityEditor;
using UnityEngine;

public static class EnemySceneSetupTools
{
    [MenuItem("SummerBlue/Test Scene/Create Basic Enemy")]
    public static void CreateBasicEnemy()
    {
        GameObject enemy = new GameObject("Enemy_Basic");
        Undo.RegisterCreatedObjectUndo(enemy, "Create Basic Enemy");

        enemy.transform.position = GetDefaultSpawnPosition();
        Undo.AddComponent<SpriteRenderer>(enemy);
        Undo.AddComponent<BoxCollider2D>(enemy);
        Undo.AddComponent<EnemyBase>(enemy);

        Selection.activeGameObject = enemy;
        EditorGUIUtility.PingObject(enemy);
    }

    private static Vector3 GetDefaultSpawnPosition()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return new Vector3(4.5f, -1f, 0f);
        }

        float distanceFromCamera = Mathf.Abs(0f - mainCamera.transform.position.z);
        Vector3 worldPosition = mainCamera.ViewportToWorldPoint(new Vector3(0.85f, 0.35f, distanceFromCamera));
        worldPosition.z = 0f;
        return worldPosition;
    }
}
