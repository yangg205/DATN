using UnityEngine;

public class UIBringToFront : MonoBehaviour
{
    public void BringToFront(GameObject panel)
    {
        panel.transform.SetAsLastSibling();
    }
}