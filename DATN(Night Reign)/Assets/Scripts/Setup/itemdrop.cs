using TMPro;
using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    private bool isPlayerInRange = false; // kiểm tra player có trong vùng item không
    [SerializeField] private string itemName; // Tên item
    [SerializeField] private GameObject itempanel;
    [SerializeField] private TextMeshProUGUI itemnametext;
    void Start()
    {
        if (itempanel != null)
        {
            itempanel.SetActive(false); // Ẩn panel khi bắt đầu
            itemnametext.text = itemName; // Hiện tên item trên panel

        }
    }

    void Update()
    {
        // Nếu player đang trong vùng và bấm phím C thì nhặt item
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.C))
        {
            PickupItem();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Player phải có tag "Player"
        {
            isPlayerInRange = true;
            itempanel.SetActive(true); // Hiện panel khi player vào vùng
            Debug.Log("Nhấn C để nhặt item!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            itempanel.SetActive(false); // Hiện panel khi player vào vùng

        }
    }

    private void PickupItem()
    {
        Debug.Log("Đã nhặt item: " + gameObject.name);
        itempanel.SetActive(false); // Ẩn panel khi nhặt item
        Destroy(gameObject);
        SimpleInventory.Instance?.AddItem($"{itemName}", 1);

    }
}
