using Game.Items;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.InventorySystem
{
    public sealed class Inventory : MonoBehaviour
    {
        [SerializeField, Min(1)] private int capacity = 24;

        public InventorySlot[] _slots;

        public event Action OnChanged;

        public int Capacity => _slots?.Length ?? 0;

        public IReadOnlyList<InventorySlot> Slots => _slots;

        private void Awake()
        {
            EnsureInitialized();
        }
        private void EnsureInitialized()
        {
            if (_slots == null || _slots.Length != capacity)
                _slots = new InventorySlot[capacity];
        }
        public bool TryAdd(ItemDefinition def, int amount, out int added)
        {
            added = 0;

            if (def == null || amount <= 0)
                return false;

            // Гарантируем инициализацию
            if (_slots == null || _slots.Length != capacity)
                _slots = new InventorySlot[capacity];

            int toAdd = amount;

            // 1) Добиваем существующие стаки
            if (def.Stackable)
            {
                for (int i = 0; i < _slots.Length && toAdd > 0; i++)
                {
                    if (_slots[i].IsEmpty) continue;
                    if (_slots[i].def != def) continue;

                    int space = def.MaxStack - _slots[i].amount;
                    if (space <= 0) continue;

                    int take = Mathf.Min(space, toAdd);
                    _slots[i].amount += take;

                    toAdd -= take;
                    added += take;
                }
            }

            // 2) Кладём в пустые слоты
            for (int i = 0; i < _slots.Length && toAdd > 0; i++)
            {
                if (!_slots[i].IsEmpty) continue;

                int putAmount = def.Stackable
                    ? Mathf.Min(def.MaxStack, toAdd)
                    : 1;

                _slots[i].def = def;
                _slots[i].amount = putAmount;

                toAdd -= putAmount;
                added += putAmount;

                // Если предмет не стакается — кладём только один
                if (!def.Stackable)
                    break;
            }

            if (added > 0)
            {
                Debug.Log($"Inventory added: {def.DisplayName} +{added}");
                OnChanged?.Invoke();
                return true;
            }

            return false;
        }



        public bool TryRemove(ItemDefinition def, int amount, out int removed)
        {
            EnsureInitialized();
            removed = 0;
            if (def == null || amount <= 0) return false;

            int toRemove = amount;

            for (int i = 0; i < _slots.Length && toRemove > 0; i++)
            {
                if (_slots[i].IsEmpty) continue;
                if (_slots[i].def != def) continue;

                int take = Mathf.Min(_slots[i].amount, toRemove);
                _slots[i].amount -= take;
                toRemove -= take;
                removed += take;

                if (_slots[i].amount <= 0)
                    _slots[i].Clear();
            }

            if (removed > 0) OnChanged?.Invoke();
            return removed > 0;
        }

        public bool TryRemoveAt(int slotIndex, int amount, out ItemStack removedStack)
        {
            EnsureInitialized();

            removedStack = default;

            if (_slots == null) return false;
            if (slotIndex < 0 || slotIndex >= _slots.Length) return false;
            if (amount <= 0) return false;

            ref InventorySlot slot = ref _slots[slotIndex];
            if (slot.IsEmpty) return false;

            int take = Mathf.Min(slot.amount, amount);
            removedStack = new ItemStack(slot.def, take);

            slot.amount -= take;
            if (slot.amount <= 0)
                slot.Clear();

            OnChanged?.Invoke();
            return true;
        }

        public int CountOf(ItemDefinition def)
        {
            EnsureInitialized();

            if (def == null || _slots == null) return 0;

            int total = 0;
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].IsEmpty) continue;
                if (_slots[i].def == def)
                    total += _slots[i].amount;
            }
            return total;
        }

        public void ClearAll()
        {
            EnsureInitialized();

            if (_slots == null) return;
            for (int i = 0; i < _slots.Length; i++)
                _slots[i].Clear();

            OnChanged?.Invoke();
        }
    public bool TryGetSlot(int index, out InventorySlot slot)
        {
            EnsureInitialized();

            slot = default;
            if (_slots == null) return false;
            if (index < 0 || index >= _slots.Length) return false;
            slot = _slots[index];
            return true;
        }

        public bool TrySetSlot(int index, InventorySlot slot)
        {
            if (_slots == null) return false;
            if (index < 0 || index >= _slots.Length) return false;
            _slots[index] = slot;
            OnChanged?.Invoke();
            return true;
        }

        public bool SwapSlots(int a, int b)
        {
            EnsureInitialized();

            if (_slots == null) return false;
            if (a < 0 || a >= _slots.Length) return false;
            if (b < 0 || b >= _slots.Length) return false;
            if (a == b) return false;

            var tmp = _slots[a];
            _slots[a] = _slots[b];
            _slots[b] = tmp;

            OnChanged?.Invoke();
            return true;
        }
        

    }
}