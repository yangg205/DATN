using UnityEngine;
using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using Random = UnityEngine.Random;


public class MainManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    public NetworkPrefabRef _tankPlayerPrefab;
    public NetworkPrefabRef _damePlayerPrefab;
    public NetworkPrefabRef _magicPlayerPrefab;


    public NetworkRunner _runner;
    public NetworkSceneManagerDefault _sceneManager;

    // Khởi tạo các biến 
    void Awake()
    {
        if (_runner == null)
        {
            GameObject runnerObj = new GameObject("NetworkRunner");
            _runner = runnerObj.AddComponent<NetworkRunner>();
            _runner.AddCallbacks(this);
            _sceneManager = runnerObj.AddComponent<NetworkSceneManagerDefault>();
        }

        ConnectToFusion();
    }

    async void ConnectToFusion()
    {
        Debug.Log("Connecting to Fushion Network...");
        _runner.ProvideInput = true;// Cho phép người chơi nhập input 
        string sessionName = "MyGameSession"; // Tên phiên 

        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Shared,// chế độ Shared Mode
            SceneManager = _sceneManager,
            SessionName = sessionName,
            PlayerCount = 5, // Số lượng người chơi tối đa
            IsVisible = true, // Có hiển thị phiên hay không 
            IsOpen = true, // Có cho phép người chơi khác tham gia hay không 
        };
        // Kết nối mạng vào Fushion
        var result = await _runner.StartGame(startGameArgs);
        if (result.Ok)
        {
            Debug.Log("Connected to Fushion Network successfully!");
        }
        else
        {
            Debug.LogError($"Failed to connect: {result.ShutdownReason}");

        }
        InvokeRepeating(nameof(SpawnEnemy), 5, 5);
    }

    public NetworkPrefabRef[] EnemyPrefabRefs;
    private NetworkObject _spawnedEnemy;
    public void SpawnEnemy()
    {
        var enemyPrefab = EnemyPrefabRefs[Random.Range(0, EnemyPrefabRefs.Length)];
        var position = new Vector3(Random.Range(-10, 10), 1, Random.Range(-10, 10));
        var rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        _spawnedEnemy = _runner.Spawn(
            enemyPrefab,
            position,
            rotation,
            null,
            (r, o) =>
            {
                EnemyAI enemyAI = o.GetComponent<EnemyAI>();
                enemyAI.networkRunner = r;
            }
        );
        Invoke(nameof(DeSpawnEnemy), 30);
    }

    void DeSpawnEnemy()
    {
        if(_spawnedEnemy != null)
        {
            _runner.Despawn(_spawnedEnemy);
        }
    }
    public void OnPlayerJoiner(NetworkRunner runner, PlayerRef player)
    {
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }
    // Hàm này sẽ được gọi sau khi kết nối mạng thành công 
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log(".......Player joined: " + player);
        // thực hiện spawn nhân vật cho người chơi 
        var playerName = PlayerPrefs.GetString("PlayerName");
        var playerClass = PlayerPrefs.GetString("PlayerClass");

        var prefab = playerClass.Equals("Tank") ? _tankPlayerPrefab : _magicPlayerPrefab;
        //(playerClass.Equals("Dame") ? _damePlayerPrefab : 
        var position = new Vector3(567.19751f, 23.1399994f, 734.330017f);

        _runner.Spawn(
            prefabRef: prefab,
            position,
            Quaternion.identity,
            player,
            (r, o) =>
            {
                Debug.Log("Player spawned " + o);
            }
        );
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {

    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {

    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {

    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {

    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {

    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {

    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {

    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {

    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }

    public void OnConnectedToServer(NetworkRunner runner)
    {

    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {

    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }
}
