// 功能：测试场景菜单，快速创建滚动背景对象。
// 技术要点：只创建对象和滚动脚本；背景外观请在 Unity 里直接配置 SpriteRenderer。
// 版本：0.4.0

using UnityEditor;
using UnityEngine;

public static class ScrollingBackgroundSceneSetupTools
{
    private const int DefaultBlockCount = 6;

    [MenuItem("SummerBlue/Test Scene/Create Simple Scrolling Background")]
    public static void CreateSimpleScrollingBackground()
    {
        GameObject root = new GameObject("SimpleScrollingBackground");
        Undo.RegisterCreatedObjectUndo(root, "Create Simple Scrolling Background");

        CreateLayer(root.transform, "FarBg", 0.45f, 1.2f, new Vector2(20f, 8.5f));
        CreateLayer(root.transform, "MidBg", 0.9f, -0.35f, new Vector2(20f, 6f));
        CreateLayer(root.transform, "GroundBg", 1.8f, -3.55f, new Vector2(20f, 2.1f));

        Selection.activeGameObject = root;
        EditorGUIUtility.PingObject(root);
    }

    private static void CreateLayer(Transform root, string layerName, float speed, float y, Vector2 size)
    {
        GameObject layerRoot = new GameObject(layerName);
        Undo.RegisterCreatedObjectUndo(layerRoot, "Create Background Layer");
        layerRoot.transform.SetParent(root);

        for (int i = 0; i < DefaultBlockCount; i++)
        {
            CreateBlock(layerRoot.transform, layerName + "_" + (i + 1).ToString("00"), size.x * i, y, speed, size);
        }
    }

    private static void CreateBlock(Transform parent, string objectName, float x, float y, float speed, Vector2 size)
    {
        GameObject block = new GameObject(objectName);
        Undo.RegisterCreatedObjectUndo(block, "Create Background Block");
        block.transform.SetParent(parent);
        block.transform.position = new Vector3(x, y, 0f);

        Undo.AddComponent<SpriteRenderer>(block);
        SimpleBgScroller scroller = Undo.AddComponent<SimpleBgScroller>(block);
        scroller.Configure(speed, -size.x, size.x * 2f);
    }
}