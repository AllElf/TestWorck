using Game.InventorySystem;
using Game.Items;
using UnityEngine;

public class InventoryDebugTester : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private ItemDefinition testItemA;
    [SerializeField] private ItemDefinition testItemB;

    private void Reset()
    {
        inventory = GetComponent<Inventory>();
    }

    private void Update()
    {
        if (inventory == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            inventory.TryAdd(testItemA, 15, out var added);
            Debug.Log($"Add A: {added}, total A = {inventory.CountOf(testItemA)}");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            inventory.TryAdd(testItemB, 3, out var added);
            Debug.Log($"Add B: {added}, total B = {inventory.CountOf(testItemB)}");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            inventory.TryRemove(testItemA, 7, out var removed);
            Debug.Log($"Remove A: {removed}, total A = {inventory.CountOf(testItemA)}");
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            inventory.ClearAll();
            Debug.Log("Inventory cleared");
        }
    }
}