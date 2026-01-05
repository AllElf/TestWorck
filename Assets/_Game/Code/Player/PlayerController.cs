using Game.InventorySystem;
using UnityEngine;

namespace Game.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private Inventory inventory;

        public Inventory Inventory => inventory;

        private void Reset()
        {
            inventory = GetComponent<Inventory>();
        }
    }
}
