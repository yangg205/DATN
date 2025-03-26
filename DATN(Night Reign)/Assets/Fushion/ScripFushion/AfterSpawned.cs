using Fusion;
using UnityEngine;

public class AfterSpawned : NetworkBehaviour
{
    private Transform spawn1; // Gắn GameObject Spawn1
    private Transform spawn2; // Gắn GameObject Spawn2

    public override void Spawned()
    {
        spawn1 = GameObject.Find("Spawn1").transform;
        spawn2 = GameObject.Find("Spawn2").transform;
        if (Runner.IsServer)
        {
            // Chỉ server quản lý việc đặt vị trí spawn
            RpcSetSpawnPosition();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcSetSpawnPosition()
    {
        // Xác định vị trí spawn dựa trên ID hoặc logic
        if (Object.HasInputAuthority)
        {
            // Người chơi với quyền điều khiển chính
            transform.position = spawn1.position;
        }
        else
        {
            // Người chơi đối thủ
            transform.position = spawn2.position;
        }
    }
}
