using Game.InventorySystem;
using Game.Items;
using UnityEngine;

namespace Game.World
{
    [DisallowMultipleComponent]
    public sealed class WorldItemPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemDefinition item;
        [SerializeField, Min(1)] private int amount = 1;

        [Header("Pickup Settings")]
        [SerializeField] private bool destroyOnPickup = true;

        public ItemDefinition Item => item;
        public int Amount => amount;

        public bool CanInteract(Game.Player.PlayerController player)
        {
            return player != null && player.Inventory != null && item != null && amount > 0;
        }

        public string GetHint(Game.Player.PlayerController player)
        {
            if (item == null) return "Поднять";
            if (amount <= 1) return $"E — Поднять: {item.DisplayName}";
            return $"E — Поднять: {item.DisplayName} x{amount}";
        }

        public void Interact(Game.Player.PlayerController player)
        {
            if (!CanInteract(player)) return;

            bool ok = player.Inventory.TryAdd(item, amount, out int added);
            if (!ok || added <= 0) return;

            // Если добавили не всё — оставляем остаток на земле
            int left = amount - added;
            if (left <= 0)
            {
                if (destroyOnPickup) Destroy(gameObject);
                else gameObject.SetActive(false);
            }
            else
            {
                amount = left;
            }
            Debug.Log($"Pickup interact: {item?.DisplayName} x{amount}");
        }
        public void Configure(ItemDefinition newItem, int newAmount)
        {
            item = newItem;
            amount = Mathf.Max(1, newAmount);
        }
    }
}
