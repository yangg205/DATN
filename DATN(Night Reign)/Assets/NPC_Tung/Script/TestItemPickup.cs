using UnityEngine;

public class TestItemPickup : MonoBehaviour
{
    public string itemID = "1";
    public int amount = 1;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            SimpleInventory.Instance.AddItem(itemID, amount);

            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.CheckItemCollectionProgress();
            }
        }
    }
}
