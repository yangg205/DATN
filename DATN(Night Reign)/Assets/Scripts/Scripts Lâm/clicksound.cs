using UnityEngine;
using UnityEngine.EventSystems;

public class clicksound : MonoBehaviour, IPointerClickHandler
{
    public AudioSource clickSound;

    

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null) clickSound.Play();
    }
}
