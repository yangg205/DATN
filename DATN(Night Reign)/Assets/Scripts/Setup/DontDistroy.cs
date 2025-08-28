using UnityEngine;

public class DontDestroyManager : MonoBehaviour
{
    public static DontDestroyManager Instance;

    [Tooltip("GameObject này sẽ không bị Destroy khi load scene mới")]
    public GameObject objectToKeep;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ manager này luôn tồn tại

            // Giữ object duy nhất nếu có
            if (objectToKeep != null)
            {
                // Kiểm tra xem object persistent cùng tên đã tồn tại chưa
                var existing = FindExistingPersistent(objectToKeep.name);
                if (existing == null)
                {
                    DontDestroyOnLoad(objectToKeep);
                }
                else
                {
                    // Nếu đã tồn tại, hủy object mới để tránh conflict
                    Destroy(objectToKeep);
                }
            }
        }
        else
        {
            Destroy(gameObject); // Nếu đã có manager, destroy thừa
        }
    }

    // Tìm object persistent cùng tên trong scene
    private GameObject FindExistingPersistent(string name)
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (var obj in allObjects)
        {
            if (obj.name == name && obj != objectToKeep)
                return obj;
        }
        return null;
    }
}
