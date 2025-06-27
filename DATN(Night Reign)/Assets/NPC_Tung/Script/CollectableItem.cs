using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    public string itemID;
    public int amount = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SimpleInventory.Instance.AddItem(itemID, amount);
            Destroy(gameObject);
        }
    }
}
