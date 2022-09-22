using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] FishSpawner spawner;

    [SerializeField] private NetworkPrefabRef _playerPrefab;
    [SerializeField] private GameObject mainMenuUI;

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            // Create a unique position for the player
            Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.DefaultPlayers) * 3, 1, 0);
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            // Keep track of the player avatars so we can remove it when they disconnect
            _spawnedCharacters.Add(player, networkPlayerObject);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        // Find and remove the players avatar
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) 
    {
        var data = new NetworkInputData();

        data.direction = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxis("Vertical"), 0);

        if (_mouseButton0)
            data.buttons |= NetworkInputData.MOUSEBUTTON0_DOWN;
        _mouseButton0 = false;

        if (_mouseButton0_Release)
            data.buttons |= NetworkInputData.MOUSEBUTTON0_UP;
        _mouseButton0_Release = false;

        if (_mouseButton1)
            data.buttons |= NetworkInputData.MOUSEBUTTON1_DOWN;
        _mouseButton1 = false;

        if (_mouseButton1_Release)
            data.buttons |= NetworkInputData.MOUSEBUTTON1_UP;
        _mouseButton1_Release = false;


        if (PlayerController.localPlayer != null)
        {
            data.currentAim = PlayerController.localPlayer.shooter.GetCurrentAim();
            data.shotPower = PlayerController.localPlayer.shooter.GetShotPower();
        }

        input.Set(data);
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) 
    {
        Debug.Log("Connected To Server");
       
    }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) 
    {
        Debug.Log("Scene Loaded");
        GameManager.singleton.screenSaver.HideLoadScreen();
        mainMenuUI.SetActive(true);
    }
    public void OnSceneLoadStart(NetworkRunner runner) { }


    private NetworkRunner _runner;
    private bool _mouseButton0;
    private bool _mouseButton0_Release;
    private bool _mouseButton1;
    private bool _mouseButton1_Release;

    void Awake()
    {
        mainMenuUI.SetActive(false);
    }

    private void Update()
    {
        _mouseButton0 = _mouseButton0 | Input.GetMouseButton(0);
        _mouseButton0_Release = _mouseButton0_Release | Input.GetMouseButtonUp(0);

        _mouseButton1 = _mouseButton1 | Input.GetMouseButton(1);
        _mouseButton1_Release = _mouseButton1_Release | Input.GetMouseButtonUp(1);
    }

    private void OnGUI()
    {
        if (_runner == null)
        {
            if (GUI.Button(new Rect(0, 0, 200, 40), "Host"))
            {
                StartGame(GameMode.Host);
            }
            if (GUI.Button(new Rect(0, 40, 200, 40), "Join"))
            {
                StartGame(GameMode.Client);
            }
        }
    }

    async void StartGame(GameMode mode)
    {
        // Create the Fusion runner and let it know that we will be providing user input
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        GameManager.singleton.screenSaver.PlayLoadScreen();

        // Start or join (depends on gamemode) a session with a specific name
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }
}