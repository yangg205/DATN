using UnityEngine;

public class ActiveBoss : MonoBehaviour
{
    [SerializeField] private GameObject m_gameObject;
    private void Start()
    {
        m_gameObject.SetActive(false);
    }
    public void OnEnable()
    {
        m_gameObject.SetActive(true);
    }
}
