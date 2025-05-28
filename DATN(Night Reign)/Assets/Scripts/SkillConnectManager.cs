using UnityEngine;

public class SkillConnectManager : MonoBehaviour
{
    public GameObject[] skillConnects;
    void Start()
    {
        foreach (GameObject skillConnect in skillConnects)
        {
            skillConnect.SetActive(true);
        }
    }
}
