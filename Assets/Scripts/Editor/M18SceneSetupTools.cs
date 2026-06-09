// 功能：提供编辑器菜单，快速在当前场景创建 M18 地面玩家测试对象。
// 技术要点：只挂载 M18Player 一个核心脚本，减少测试阶段的组件依赖。
// 版本：v0.2.0

using UnityEditor;
using UnityEngine;

public static class M18SceneSetupTools
{
    [MenuItem("SummerBlue/Test Scene/Create M18 Player")]
    public static void CreateM18Player()
    {
        GameObject player = new GameObject("Player_M18");
        Undo.RegisterCreatedObjectUndo(player, "Create M18 Player");

        player.transform.position = GetDefaultSpawnPosition();
        Undo.AddComponent<SpriteRenderer>(player);
        Undo.AddComponent<BoxCollider2D>(player).isTrigger = true;
        Undo.AddComponent<M18Player>(player);

        Selection.activeGameObject = player;
        EditorGUIUtility.PingObject(player);
    }

    private static Vector3 GetDefaultSpawnPosition()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return new Vector3(-3.5f, -2f, 0f);
        }

        float distanceFromCamera = Mathf.Abs(0f - mainCamera.transform.position.z);
        Vector3 worldPosition = mainCamera.ViewportToWorldPoint(new Vector3(0.2f, 0.2f, distanceFromCamera));
        worldPosition.z = 0f;
        return worldPosition;
    }
}
