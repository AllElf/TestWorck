using Game.World;
using UnityEngine;

namespace Game.Player
{
    public interface IHintUI
    {
        void SetHint(string text);
    }

    [DisallowMultipleComponent]
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [Header("Interaction")]
        [SerializeField] private float radius = 2.2f;
        [SerializeField] private LayerMask interactableMask = ~0;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private KeyCode inventorytKey = KeyCode.F;
        bool isOpenInventory;

        [Header("UI (Optional)")]
        [SerializeField] private MonoBehaviour hintUiBehaviour; // должен реализовывать IHintUI
        [SerializeField] GameObject canvasInventory;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;

        private readonly Collider[] _hits = new Collider[24];
        private PlayerController _player;
        private IHintUI _hintUI;
        private IInteractable _current;

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
            _hintUI = hintUiBehaviour as IHintUI;
        }

        private void Update()
        {
            _current = FindBestInteractable();

            if (_hintUI != null)
            {
                _hintUI.SetHint(_current != null ? _current.GetHint(_player) : string.Empty);
            }

            if (_current != null && Input.GetKeyDown(interactKey))
            {
                if (_current.CanInteract(_player))
                    _current.Interact(_player);
            }
            if (Input.GetKeyDown(inventorytKey))
            {
                isOpenInventory = !isOpenInventory;
            }
            InventoryOpen();
        }

        private IInteractable FindBestInteractable()
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, radius, _hits, interactableMask);
            if (count <= 0) return null;

            float bestDistSq = float.MaxValue;
            IInteractable best = null;

            for (int i = 0; i < count; i++)
            {
                Collider col = _hits[i];
                if (col == null) continue;

                // »щем IInteractable на объекте или на родителе
                IInteractable interactable = col.GetComponent<IInteractable>();
                if (interactable == null)
                    interactable = col.GetComponentInParent<IInteractable>();

                if (interactable == null) continue;
                if (_player != null && !interactable.CanInteract(_player)) continue;

                Vector3 p = col.ClosestPoint(transform.position);
                float d = (p - transform.position).sqrMagnitude;
                if (d < bestDistSq)
                {
                    bestDistSq = d;
                    best = interactable;
                }
            }

            return best;
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }

        public void InventoryOpen()
        {
            if(isOpenInventory && canvasInventory != null)
            {
                canvasInventory.SetActive(true);
            }
            else
            {
                canvasInventory.SetActive(false);
            }
        }
    }
}
