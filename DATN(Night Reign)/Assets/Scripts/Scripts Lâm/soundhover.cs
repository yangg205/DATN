using UnityEngine;
using UnityEngine.EventSystems;

public class soundhover : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public AudioSource hoverSound;
    public AudioSource clickSound;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null) hoverSound.Play();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null) clickSound.Play();
    }
}
