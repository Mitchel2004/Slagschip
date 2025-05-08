using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInputHandler : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadCompletedRpc;
    }

    [Rpc(SendTo.Owner)]
    private void OnLoadCompletedRpc(ulong _clientId, string _sceneName, LoadSceneMode _loadSceneMode)
    {
        if (_clientId == NetworkManager.Singleton.LocalClientId && _sceneName == "Main")
        {
            NetworkObject _localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;

            StartCoroutine(EnablePlayerInput(_localPlayer.GetComponent<PlayerInput>()));
        }
    }

    IEnumerator EnablePlayerInput(PlayerInput _playerInput)
    {
        yield return new WaitForEndOfFrame();

        _playerInput.enabled = true;
    }
}
