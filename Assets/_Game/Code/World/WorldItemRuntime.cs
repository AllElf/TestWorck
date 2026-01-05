using Game.Items;
using UnityEngine;

namespace Game.World
{
    [DisallowMultipleComponent]
    public sealed class WorldItemRuntime : MonoBehaviour
    {
        [SerializeField] private WorldItemPickup pickup;

        private void Reset()
        {
            pickup = GetComponent<WorldItemPickup>();
        }

        public void Set(ItemDefinition def, int amount)
        {
            // ƒанные храним в WorldItemPickup (он же IInteractable)
            // ≈сли вы хотите безопасно Ч можно сделать публичный метод в WorldItemPickup,
            // но дл€ простоты здесь через SerializeField + инспектор.
            if (pickup == null) pickup = GetComponent<WorldItemPickup>();
            if (pickup == null) return;

            // ѕр€мо в инспекторные пол€ мы не лезем. ѕоэтому добавим в WorldItemPickup публичный метод ниже.
            pickup.Configure(def, amount);
        }
    }
}
