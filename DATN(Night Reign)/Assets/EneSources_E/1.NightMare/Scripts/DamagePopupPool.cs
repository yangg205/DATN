using System.Collections.Generic;
using UnityEngine;

public class DamagePopupPool : MonoBehaviour
{
    public static DamagePopupPool Instance;

    public GameObject popupPrefab;
    public int initialPoolSize = 10;

    private Queue<DamagePopup> pool = new Queue<DamagePopup>();

    void Awake()
    {
        Instance = this;
        FillPool();
    }

    void FillPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreatePopup();
        }
    }

    DamagePopup CreatePopup()
    {
        GameObject obj = Instantiate(popupPrefab);
        obj.SetActive(false);
        DamagePopup popup = obj.GetComponent<DamagePopup>();
        pool.Enqueue(popup);
        return popup;
    }

    public DamagePopup GetFromPool()
    {
        if (pool.Count == 0)
        {
            CreatePopup();
        }

        DamagePopup popup = pool.Dequeue();
        popup.gameObject.SetActive(true);
        return popup;
    }

    public void ReturnToPool(DamagePopup popup)
    {
        popup.gameObject.SetActive(false);
        pool.Enqueue(popup);
    }
}
