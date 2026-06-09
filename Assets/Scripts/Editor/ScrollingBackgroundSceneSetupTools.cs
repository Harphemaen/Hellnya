// 功能：提供编辑器菜单，快速创建远景、中景、地面三层简易卷轴背景。
// 技术要点：每层由两个背景块组成，各自挂 SimpleBgScroller；后续直接替换 SpriteRenderer 的 Sprite 即可换图。
// 版本：v0.2.0

using UnityEditor;
using UnityEngine;

public static class ScrollingBackgroundSceneSetupTools
{
    [MenuItem("SummerBlue/Test Scene/Create Simple Scrolling Background")]
    public static void CreateSimpleScrollingBackground()
    {
        GameObject root = new GameObject("SimpleScrollingBackground");
        Undo.RegisterCreatedObjectUndo(root, "Create Simple Scrolling Background");

        CreateLayer(root.transform, "FarBg", 0.45f, 1.2f, -40, new Vector2(20f, 8.5f), new Color(0.16f, 0.18f, 0.32f, 1f));
        CreateLayer(root.transform, "MidBg", 0.9f, -0.35f, -30, new Vector2(20f, 6f), new Color(0.18f, 0.42f, 0.45f, 1f));
        CreateLayer(root.transform, "GroundBg", 1.8f, -3.55f, -20, new Vector2(20f, 2.1f), new Color(0.25f, 0.22f, 0.18f, 1f));

        Selection.activeGameObject = root;
        EditorGUIUtility.PingObject(root);
    }

    private static void CreateLayer(Transform root, string layerName, float speed, float y, int sortingOrder, Vector2 size, Color color)
    {
        GameObject layerRoot = new GameObject(layerName);
        Undo.RegisterCreatedObjectUndo(layerRoot, "Create Background Layer");
        layerRoot.transform.SetParent(root);

        CreateBlock(layerRoot.transform, layerName + "_A", 0f, y, speed, sortingOrder, size, color);
        CreateBlock(layerRoot.transform, layerName + "_B", size.x, y, speed, sortingOrder, size, color);
    }

    private static void CreateBlock(Transform parent, string objectName, float x, float y, float speed, int sortingOrder, Vector2 size, Color color)
    {
        GameObject block = new GameObject(objectName);
        Undo.RegisterCreatedObjectUndo(block, "Create Background Block");
        block.transform.SetParent(parent);
        block.transform.position = new Vector3(x, y, 0f);

        Undo.AddComponent<SpriteRenderer>(block);
        SimpleBgScroller scroller = Undo.AddComponent<SimpleBgScroller>(block);
        scroller.Configure(speed, -size.x, size.x * 2f, size, color, sortingOrder);
    }
}
