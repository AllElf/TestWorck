using Game.InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    public sealed class InventorySlotUI : MonoBehaviour,
        IPointerClickHandler,
        IPointerEnterHandler, IPointerExitHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler,
        IDropHandler
    {
        [Header("UI")]
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text amountText;

        [Header("Highlights")]
        [SerializeField] private Image selectionFrame; // выбранный слот
        [SerializeField] private Image hoverFrame;     // подсветка цели (включая пустые)

        [Header("Hover Colors")]
        [Tooltip("Цвет подсветки, если слот пустой (должен быть зелёный).")]
        [SerializeField] private Color emptySlotHoverColor = Color.green;

        [Tooltip("Цвет подсветки, если слот занят (например жёлтый/оранжевый).")]
        [SerializeField] private Color filledSlotHoverColor = new Color(1f, 0.8f, 0.2f, 1f);

        [Header("Ghost Drag (Optional)")]
        [SerializeField] private Canvas rootCanvas; // можно не задавать — найдём автоматически
        [SerializeField] private float ghostAlpha = 0.9f;

        private InventoryUI _owner;
        private Inventory _inventory;
        private int _index;

        // Глобальное состояние drag
        private static bool s_isDragging;
        private static InventorySlotUI s_dragSource;
        private static GameObject s_ghostGO;
        private static RectTransform s_ghostRT;
        private static Image s_ghostImage;

        public void Bind(InventoryUI owner, Inventory inventory, int index)
        {
            _owner = owner;
            _inventory = inventory;
            _index = index;

            if (hoverFrame != null) hoverFrame.enabled = false;
            SetSelected(false);
            Render(default);
        }

        public void Render(InventorySlot slot)
        {
            bool empty = slot.IsEmpty;

            if (icon != null)
            {
                icon.enabled = !empty && slot.def != null && slot.def.Icon != null;
                icon.sprite = (!empty && slot.def != null) ? slot.def.Icon : null;
            }

            if (amountText != null)
            {
                if (empty || slot.def == null) amountText.text = "";
                else amountText.text = slot.def.Stackable ? slot.amount.ToString() : "";
            }
        }

        public void SetSelected(bool selected)
        {
            if (selectionFrame != null)
                selectionFrame.enabled = selected;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _owner?.SelectSlot(_index);
        }

        // --- Подсветка цели (пустой = зелёный) ---
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!s_isDragging) return;
            if (s_dragSource == null) return;
            if (s_dragSource == this) return;

            SetHover(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetHover(false);
        }

        private void SetHover(bool enabled)
        {
            if (hoverFrame == null) return;

            if (!enabled)
            {
                hoverFrame.enabled = false;
                return;
            }

            // Определяем, пустой ли слот-цель
            bool isEmpty = true;
            if (_inventory != null && _inventory.TryGetSlot(_index, out var slot))
                isEmpty = slot.IsEmpty;

            hoverFrame.color = isEmpty ? emptySlotHoverColor : filledSlotHoverColor;
            hoverFrame.enabled = true;
        }

        // --- Drag ---
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_inventory == null) return;
            if (!_inventory.TryGetSlot(_index, out var slot) || slot.IsEmpty) return;

            s_isDragging = true;
            s_dragSource = this;

            _owner?.SelectSlot(_index);

            EnsureRootCanvas();
            CreateGhost(slot);
            UpdateGhostPosition(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!s_isDragging) return;
            UpdateGhostPosition(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // убрать подсветку текущего слота (если курсор остался на нём)
            SetHover(false);

            DestroyGhost();

            s_isDragging = false;
            s_dragSource = null;
        }

        // --- Drop на слот-цель => SWAP ---
        public void OnDrop(PointerEventData eventData)
        {
            if (!s_isDragging) return;
            if (s_dragSource == null) return;
            if (s_dragSource._owner != _owner) return;

            int fromIndex = s_dragSource._index;
            int toIndex = _index;

            if (fromIndex == toIndex) return;

            // Подсветку цели выключаем
            SetHover(false);

            // SWAP: источник <-> цель
            _owner?.RequestSwap(fromIndex, toIndex);
        }

        // -------------------------
        // Ghost helpers
        // -------------------------
        private void EnsureRootCanvas()
        {
            if (rootCanvas != null) return;
            rootCanvas = GetComponentInParent<Canvas>();
        }

        private void CreateGhost(InventorySlot slot)
        {
            DestroyGhost();

            if (rootCanvas == null) return;
            if (slot.def == null || slot.def.Icon == null) return;

            s_ghostGO = new GameObject("DragGhost", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            s_ghostGO.transform.SetParent(rootCanvas.transform, false);

            s_ghostRT = s_ghostGO.GetComponent<RectTransform>();
            s_ghostImage = s_ghostGO.GetComponent<Image>();

            s_ghostImage.raycastTarget = false;
            s_ghostImage.sprite = slot.def.Icon;
            s_ghostImage.preserveAspect = true;

            var cg = s_ghostGO.GetComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.alpha = ghostAlpha;

            if (icon != null && icon.rectTransform != null)
                s_ghostRT.sizeDelta = icon.rectTransform.rect.size;
            else
                s_ghostRT.sizeDelta = new Vector2(64, 64);
        }

        private void UpdateGhostPosition(PointerEventData eventData)
        {
            if (s_ghostRT == null || rootCanvas == null) return;

            RectTransform canvasRT = rootCanvas.transform as RectTransform;
            if (canvasRT == null) return;

            Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, eventData.position, cam, out Vector2 localPos))
            {
                s_ghostRT.anchoredPosition = localPos;
            }
        }

        private static void DestroyGhost()
        {
            if (s_ghostGO != null)
            {
                Destroy(s_ghostGO);
                s_ghostGO = null;
                s_ghostRT = null;
                s_ghostImage = null;
            }
        }
    }
}
