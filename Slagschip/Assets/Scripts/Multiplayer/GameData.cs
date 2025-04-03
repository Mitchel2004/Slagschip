using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Multiplayer
{
    public class GameData : NetworkBehaviour
    {
        public static GameData instance;
        public NetworkVariable<ulong> currentPlayerTurn = new();
        public NetworkVariable<bool> isHostTurn = new();

        [SerializeField] private InputAction debugPlayerSwitch;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(instance);
            }
        }

        private void Start()
        {
            debugPlayerSwitch.Enable();
            debugPlayerSwitch.performed += context => SwitchPlayerTurnRpc();
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
            isHostTurn.Value = true;
        }

        [Rpc(SendTo.Server)]
        public void SwitchPlayerTurnRpc()
        {
            IReadOnlyList<ulong> _connectedClientIds = NetworkManager.ConnectedClientsIds;

            if (IsServer)
                currentPlayerTurn.Value = _connectedClientIds[currentPlayerTurn.Value == _connectedClientIds.Last() ? 0 : _connectedClientIds.ToList().IndexOf(currentPlayerTurn.Value) + 1];
                isHostTurn.Value = !isHostTurn.Value;
        }
    }
}
