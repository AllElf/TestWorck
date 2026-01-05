using UnityEngine;

namespace Game.Items
{
    public enum ItemCategory
    {
        Loot,
        Resource,
        Quest,
        Consumable
    }

    [CreateAssetMenu(menuName = "Game/Items/Item Definition", fileName = "Item_")]
    public class ItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string itemId = "item_id";
        [SerializeField] private string displayName = "New Item";

        [Header("UI")]
        [SerializeField] private Sprite icon;

        [Header("World")]
        [SerializeField] private GameObject worldPrefab;

        [Header("Rules")]
        [SerializeField] private ItemCategory category = ItemCategory.Loot;
        [SerializeField] private bool stackable = true;
        [Min(1)]
        [SerializeField] private int maxStack = 99;

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public GameObject WorldPrefab => worldPrefab;
        public ItemCategory Category => category;
        public bool Stackable => stackable;
        public int MaxStack => stackable ? maxStack : 1;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!stackable) maxStack = 1;
            if (string.IsNullOrWhiteSpace(itemId))
                itemId = name.Trim().ToLowerInvariant().Replace(" ", "_");
        }
#endif
    }
}
