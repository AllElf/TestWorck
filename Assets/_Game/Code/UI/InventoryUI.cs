using Game.InventorySystem;
using Game.Player;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    [DisallowMultipleComponent]
    public sealed class InventoryUI : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Inventory inventory;
        [SerializeField] private InventoryDropper dropper;

        [Header("UI")]
        [SerializeField] private Transform slotsRoot;         // GridLayoutGroup root
        [SerializeField] private InventorySlotUI slotPrefab; // prefab слота
        [SerializeField] private TMP_Text selectedInfoText;  // опционально

        [Header("Hotkeys")]
        [SerializeField] private KeyCode dropKey = KeyCode.Q;

        private InventorySlotUI[] _slotUIs;
        private int _selectedIndex = -1;

        // Важно: Start (а не Awake), чтобы Inventory успел создать _slots
        private void Start()
        {
            if (inventory != null)
                inventory.OnChanged += Refresh;

            Build();
            Refresh();
        }

        private void OnDestroy()
        {
            if (inventory != null)
                inventory.OnChanged -= Refresh;
        }

        private void Update()
        {
            if (inventory == null || dropper == null) return;

            if (Input.GetKeyDown(dropKey) && _selectedIndex >= 0)
            {
                bool dropAll = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

                if (!inventory.TryGetSlot(_selectedIndex, out var slot) || slot.IsEmpty)
                    return;

                int amount = dropAll ? slot.amount : 1;
                dropper.DropFromSlot(_selectedIndex, amount);
                // UI обновится через OnChanged из Inventory
            }
        }

        private void Build()
        {
            if (inventory == null || slotsRoot == null || slotPrefab == null) return;

            // Очистка
            for (int i = slotsRoot.childCount - 1; i >= 0; i--)
                Destroy(slotsRoot.GetChild(i).gameObject);

            _slotUIs = new InventorySlotUI[inventory.Capacity];

            for (int i = 0; i < inventory.Capacity; i++)
            {
                var ui = Instantiate(slotPrefab, slotsRoot);
                ui.Bind(this, inventory, i);
                _slotUIs[i] = ui;
            }
        }

        public void SelectSlot(int index)
        {
            if (_slotUIs == null || _slotUIs.Length == 0) return;

            _selectedIndex = Mathf.Clamp(index, 0, _slotUIs.Length - 1);

            for (int i = 0; i < _slotUIs.Length; i++)
                _slotUIs[i].SetSelected(i == _selectedIndex);

            UpdateSelectedInfo();
        }

        public void RequestSwap(int fromIndex, int toIndex)
        {
            if (inventory == null) return;

            bool swapped = inventory.SwapSlots(fromIndex, toIndex);

            // На всякий случай сразу обновляем визуал (если подписка/ивент где-то отвалилась)
            if (swapped) Refresh();

            SelectSlot(toIndex);
        }

        private void Refresh()
        {
            if (inventory == null || _slotUIs == null) return;

            // 1) Рендерим слоты
            for (int i = 0; i < _slotUIs.Length; i++)
            {
                if (inventory.TryGetSlot(i, out var slot))
                    _slotUIs[i].Render(slot);
            }

            // 2) Автовыбор: чтобы Q работал без клика
            if (_selectedIndex < 0)
            {
                int firstNonEmpty = -1;
                for (int i = 0; i < _slotUIs.Length; i++)
                {
                    if (inventory.TryGetSlot(i, out var s) && !s.IsEmpty)
                    {
                        firstNonEmpty = i;
                        break;
                    }
                }

                SelectSlot(firstNonEmpty >= 0 ? firstNonEmpty : 0);
                return;
            }

            // 3) Нормализация индекса
            if (_selectedIndex >= _slotUIs.Length)
                _selectedIndex = _slotUIs.Length - 1;

            // 4) Если выбранный слот пуст — переключимся на первый непустой
            if (!inventory.TryGetSlot(_selectedIndex, out var selectedSlot) || selectedSlot.IsEmpty)
            {
                int firstNonEmpty = -1;
                for (int i = 0; i < _slotUIs.Length; i++)
                {
                    if (inventory.TryGetSlot(i, out var s) && !s.IsEmpty)
                    {
                        firstNonEmpty = i;
                        break;
                    }
                }

                SelectSlot(firstNonEmpty >= 0 ? firstNonEmpty : 0);
                return;
            }

            // 5) Инфо
            UpdateSelectedInfo();
        }

        private void UpdateSelectedInfo()
        {
            if (selectedInfoText == null) return;

            if (_selectedIndex < 0 || inventory == null ||
                !inventory.TryGetSlot(_selectedIndex, out var slot) || slot.IsEmpty)
            {
                selectedInfoText.text = "Selected: (none)";
                return;
            }

            string name = slot.def != null ? slot.def.DisplayName : "(null)";
            selectedInfoText.text = $"Selected: #{_selectedIndex} — {name} x{slot.amount} (Q drop 1, Shift+Q drop all)";
        }
    }
}
