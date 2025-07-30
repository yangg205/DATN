using UnityEngine;
using System.Collections.Generic;

public class VFXPoolManager : MonoBehaviour
{
    public static VFXPoolManager Instance;

    [System.Serializable]
    public class VFXPool
    {
        public string name;
        public GameObject prefab;
        public int size = 3;
        [HideInInspector] public Queue<GameObject> pool = new Queue<GameObject>();
    }

    public List<VFXPool> vfxPools;

    private void Awake()
    {
        Instance = this;

        foreach (var vfx in vfxPools)
        {
            for (int i = 0; i < vfx.size; i++)
            {
                GameObject obj = Instantiate(vfx.prefab);
                obj.SetActive(false);
                vfx.pool.Enqueue(obj);
            }
        }
    }

    public GameObject SpawnFromPool(string name, Vector3 position, Quaternion rotation)
    {
        var vfx = vfxPools.Find(p => p.name == name);
        if (vfx == null) return null;

        GameObject obj = vfx.pool.Dequeue();
        obj.SetActive(true);
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        // Optional: tắt sau 1-2 giây nếu là effect ngắn
        StartCoroutine(DisableAfterSeconds(obj, 1.5f));

        vfx.pool.Enqueue(obj);
        return obj;
    }

    private IEnumerator<WaitForSeconds> DisableAfterSeconds(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
    }
}
