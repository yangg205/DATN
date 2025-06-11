using UnityEngine;

public class CharacterSelectionHandler : MonoBehaviour
{
    public GameObject nameInputPanel;

    private void Start()
    {
        // Ẩn panel khi bắt đầu game
        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(false);
        }
    }

    // Hàm gọi khi chọn nhân vật
    public void OnCharacterSelected()
    {
        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(true);
        }
    }
}
