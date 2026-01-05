using System;
using Game.Items;

namespace Game.InventorySystem
{
    [Serializable]
    public struct InventorySlot
    {
        public ItemDefinition def;
        public int amount;

        public bool IsEmpty => def == null || amount <= 0;

        public void Clear()
        {
            def = null;
            amount = 0;
        }

        public int SpaceLeft()
        {
            if (IsEmpty) return 0;
            return Math.Max(0, def.MaxStack - amount);
        }
    }
}