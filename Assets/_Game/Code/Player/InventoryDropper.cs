using Game.InventorySystem;
using Game.World;
using UnityEngine;

namespace Game.Player
{
    [DisallowMultipleComponent]
    public sealed class InventoryDropper : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private PlayerController player;
        [SerializeField] private Transform dropPoint;

        [Header("Drop Settings")]
        [SerializeField] private GameObject genericWorldItemPrefab; // prefab с WorldItemPickup + Collider
        [SerializeField] private float dropForward = 1.2f;
        [SerializeField] private float dropRadius = 0.4f;

        private void Reset()
        {
            player = GetComponent<PlayerController>();
        }

        public bool DropFromSlot(int slotIndex, int amount)
        {
            if (player == null || player.Inventory == null) return false;

            if (!player.Inventory.TryRemoveAt(slotIndex, amount, out var removed))
                return false;

            Vector3 basePos = dropPoint != null ? dropPoint.position : transform.position + transform.forward * dropForward;
            Vector3 spawnPos = basePos + new Vector3(Random.Range(-dropRadius, dropRadius), 0f, Random.Range(-dropRadius, dropRadius));

            // Если у ItemDefinition есть worldPrefab — используем его, иначе generic
            GameObject prefab = removed.def != null && removed.def.WorldPrefab != null
                ? removed.def.WorldPrefab
                : genericWorldItemPrefab;

            if (prefab == null)
            {
                Debug.LogWarning("No world prefab to spawn for dropped item.");
                return true; // предмет уже удалён из инвентаря
            }

            GameObject go = Instantiate(prefab, spawnPos, Quaternion.identity);

            // Настроим данные (если это наш универсальный prefab)
            var runtime = go.GetComponent<WorldItemRuntime>();
            if (runtime != null && removed.def != null)
                runtime.Set(removed.def, removed.amount);

            return true;
        }
    }
}
