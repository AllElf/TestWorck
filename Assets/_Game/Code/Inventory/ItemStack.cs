using System;
using Game.Items;

namespace Game.InventorySystem
{
    [Serializable]
    public struct ItemStack
    {
        public ItemDefinition def;
        public int amount;

        public ItemStack(ItemDefinition def, int amount)
        {
            this.def = def;
            this.amount = amount;
        }

        public bool IsValid => def != null && amount > 0;
    }
}
