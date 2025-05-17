using UnityEngine;
using UnityEngine.EventSystems;

public class HoverGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Material glowMat;
    public Material defaultMat;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material = defaultMat;

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        rend.material = glowMat;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rend.material = defaultMat;
    }
}