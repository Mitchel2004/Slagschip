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

        PlayerInput playerInput = GetComponent<PlayerInput>();
        playerInput.enabled = false;

        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadCompletedRpc;
    }

    [Rpc(SendTo.Owner)]
    private void OnLoadCompletedRpc(ulong _clientId, string _sceneName, LoadSceneMode _loadSceneMode)
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        StartCoroutine(EnablePlayerInput(playerInput));
    }

    IEnumerator EnablePlayerInput(PlayerInput _playerInput)
    {
        yield return new WaitForEndOfFrame();

        _playerInput.enabled = true;
    }
}
