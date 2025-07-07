using UnityEngine;
using UnityEngine.EventSystems;

public class TutorialPopupClickHandler : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.CloseCurrentPopup();
        }
    }
}
