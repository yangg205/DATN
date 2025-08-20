using System.Collections.Generic;
using UnityEngine;

public class MinimapManager : MonoBehaviour
{
    public static MinimapManager Instance;
    public Transform player;
    public GameObject checkpointButtonPrefab;
    public Transform checkpointButtonParent; // chỗ chứa button trên UI

    private List<Checkpoint> unlockedCheckpoints = new List<Checkpoint>();

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterCheckpoint(Checkpoint cp)
    {
        if (!unlockedCheckpoints.Contains(cp))
        {
            unlockedCheckpoints.Add(cp);
            CreateCheckpointButton(cp);
        }
    }

    private void CreateCheckpointButton(Checkpoint cp)
    {
        GameObject btnObj = Instantiate(checkpointButtonPrefab, checkpointButtonParent);
        UnityEngine.UI.Button btn = btnObj.GetComponent<UnityEngine.UI.Button>();
        btnObj.GetComponentInChildren<UnityEngine.UI.Text>().text = cp.checkpointName;
        btn.onClick.AddListener(() => TeleportToCheckpoint(cp));
    }

    public void TeleportToCheckpoint(Checkpoint cp)
    {
        player.position = cp.teleportPosition;
    }
}
