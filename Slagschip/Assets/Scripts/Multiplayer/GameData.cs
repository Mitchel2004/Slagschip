using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Multiplayer
{
    public class GameData : NetworkBehaviour
    {
        public NetworkVariable<ulong> currentPlayerTurn = new();

        [SerializeField] private InputAction _debugPlayerSwitch;

        private void Start()
        {
            _debugPlayerSwitch.Enable();
            _debugPlayerSwitch.performed += context => SwitchPlayerTurnRpc();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (IsServer)
                NetworkManager.OnServerStarted += InitializePlayerTurn;
        }
        
        private void InitializePlayerTurn()
        {
            currentPlayerTurn.Value = NetworkManager.ConnectedClientsIds[0];
        }

        [Rpc(SendTo.Server)]
        public void SwitchPlayerTurnRpc()
        {
            IReadOnlyList<ulong> _connectedClientIds = NetworkManager.ConnectedClientsIds;

            if (IsServer)
                currentPlayerTurn.Value = _connectedClientIds[currentPlayerTurn.Value == _connectedClientIds.Last() ? 0 : _connectedClientIds.ToList().IndexOf(currentPlayerTurn.Value) + 1];
        }
    }
}
