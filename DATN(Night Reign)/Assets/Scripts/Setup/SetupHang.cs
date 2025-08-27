using UnityEngine;

public class SetupHang : MonoBehaviour
{
    public QuestData QuestData;
    public GameObject hang;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hang.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (QuestData.isQuestCompleted)
        {
            hang.SetActive(true);
        }
    }
}
