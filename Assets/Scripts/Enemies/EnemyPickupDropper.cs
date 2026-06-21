using UnityEngine;

[DisallowMultipleComponent]
public class EnemyPickupDropper : MonoBehaviour
{
    [SerializeField] private PickupItem pickupPrefab = null;
    [SerializeField] private Vector2 dropOffset = Vector2.zero;

    public void Drop()
    {
        if (!isActiveAndEnabled || pickupPrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = transform.position + new Vector3(dropOffset.x, dropOffset.y, 0f);
        Instantiate(pickupPrefab, spawnPosition, Quaternion.identity);
    }
}
