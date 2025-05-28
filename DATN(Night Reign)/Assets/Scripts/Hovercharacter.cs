using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Hovercharacter : MonoBehaviour
{
    private Material originalMaterial;
    public Material hoverMaterial; // vật liệu sáng hơn hoặc có hiệu ứng glow
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        originalMaterial = rend.material;
    }

    public void OnHoverEnter()
    {
        rend.material = hoverMaterial;
        transform.localScale = Vector3.one * 1.05f; // Phóng to nhẹ
    }

    public void OnHoverExit()
    {
        rend.material = originalMaterial;
        transform.localScale = Vector3.one;
    }
}