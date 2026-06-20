// 功能：让一个背景对象可选地横向自移动，并在越过左边界后循环到右侧。
// 技术要点：外观交给对象自己的 SpriteRenderer；本脚本只负责位移和循环。
// 配置：moveObjectInPlay 是否自移动；scrollSpeed 自移动速度；recycleLeftX/recycleDistance 循环位置。
// 版本：0.5.0

using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class SimpleBgScroller : MonoBehaviour
{
    [Header("Scroll")]
    [SerializeField] private bool moveObjectInPlay;
    [SerializeField] private float scrollSpeed = 1f;
    [SerializeField] private float recycleLeftX = -20f;
    [SerializeField] private float recycleDistance = 40f;

    private void Awake()
    {
        NormalizeSettings();
    }

    private void Reset()
    {
        NormalizeSettings();
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

        if (!moveObjectInPlay)
        {
            return;
        }

        transform.position += Vector3.left * scrollSpeed * Time.deltaTime;
        if (transform.position.x <= recycleLeftX)
        {
            transform.position += Vector3.right * recycleDistance;
        }
    }

    public void Configure(float newScrollSpeed, float newRecycleLeftX, float newRecycleDistance)
    {
        scrollSpeed = Mathf.Max(0f, newScrollSpeed);
        recycleLeftX = newRecycleLeftX;
        recycleDistance = Mathf.Max(0.01f, newRecycleDistance);
    }

    private void NormalizeSettings()
    {
        scrollSpeed = Mathf.Max(0f, scrollSpeed);
        recycleDistance = Mathf.Max(0.01f, recycleDistance);
    }
}