using Multiplayer;
using PlayerGrid;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Unity.Services.Multiplayer;
using TMPro;
using System.Linq;

namespace UIHandlers
{
    [RequireComponent(typeof(UIDocument))]
    public class DashboardHandler : NetworkBehaviour
    {
        public UnityEvent onReady;
        public UnityEvent onGameStart;

        private UIDocument _document;

        [SerializeField] private GameData _gameData;
        [SerializeField] private GridHandler _gridHandler;
        [SerializeField] private TMP_Text _sessionCodeText;
        [SerializeField] private UnityEngine.UI.Button _copySessionCode;

        private NetworkVariable<byte> _readyPlayers = new();
        private bool _inPregame = true;

        private const byte _gridSize = GridHandler.gridSize;
        private const byte _playerCount = 2;

        private Button[] _gridButtons = new Button[_gridSize * (_gridSize + 4)];

        private byte _targetCell;

        private void Awake()
        {
            _gameData.currentPlayerTurn.OnValueChanged += OnPlayerTurnChange;

            _document = GetComponent<UIDocument>();

            _document.rootVisualElement.Query("turn-information").First().RegisterCallback<TransitionEndEvent>(TurnFadeOut);
            _document.rootVisualElement.Query("menu-button").First().RegisterCallback<ClickEvent>(OnMenu);
            _document.rootVisualElement.Query("attack-button").First().RegisterCallback<ClickEvent>(OnAttack);
            _document.rootVisualElement.Query("torpedo-button").First().RegisterCallback<ClickEvent>(OnTorpedo);
            _document.rootVisualElement.Query("naval-mine-button").First().RegisterCallback<ClickEvent>(OnMine);
            _document.rootVisualElement.Query("ready-button").First().RegisterCallback<ClickEvent>(Ready);
            _document.rootVisualElement.Query("play-code").First().RegisterCallback<ClickEvent>(OnPlayCode);

            _gridHandler.OnIsReady.AddListener(IsReady);
            
            for (byte i = 0; i < _gridSize * _gridSize; i++)
            {
                Button _gridButton = new();

                _gridButton.AddToClassList("grid-button");

                _gridButton.style.width = new StyleLength(new Length(100 / _gridSize, LengthUnit.Percent));
                _gridButton.style.height = new StyleLength(new Length(100 / _gridSize, LengthUnit.Percent));

                _gridButton.RegisterCallback<ClickEvent, byte>(SetTargetCell, i);

                _document.rootVisualElement.Query("grid-container").First().Add(_gridButton);

                _gridButtons[i] = _gridButton;
            }

            for (byte i = 0; i < _gridSize; i++)
            {
                Button _horizontalLRGridButton = new();
                Button _horizontalRLGridButton = new();
                Button _verticalTBGridButton = new();
                Button _verticalBTGridButton = new();

                _horizontalLRGridButton.AddToClassList("horizontal-grid-button");
                _horizontalRLGridButton.AddToClassList("horizontal-grid-button");
                _verticalTBGridButton.AddToClassList("vertical-grid-button");
                _verticalBTGridButton.AddToClassList("vertical-grid-button");

                _horizontalLRGridButton.style.height = new StyleLength(new Length(100 / _gridSize, LengthUnit.Percent));
                _horizontalRLGridButton.style.height = new StyleLength(new Length(100 / _gridSize, LengthUnit.Percent));
                _verticalTBGridButton.style.width = new StyleLength(new Length(100 / _gridSize, LengthUnit.Percent));
                _verticalBTGridButton.style.width = new StyleLength(new Length(100 / _gridSize, LengthUnit.Percent));

                _horizontalLRGridButton.RegisterCallback<ClickEvent, byte>(SetTargetCell, (byte)(_gridSize * _gridSize + i));
                _horizontalRLGridButton.RegisterCallback<ClickEvent, byte>(SetTargetCell, (byte)(_gridSize * (_gridSize + 1) + i));
                _verticalTBGridButton.RegisterCallback<ClickEvent, byte>(SetTargetCell, (byte)(_gridSize * (_gridSize + 2) + i));
                _verticalBTGridButton.RegisterCallback<ClickEvent, byte>(SetTargetCell, (byte)(_gridSize * (_gridSize + 3) + i));

                _document.rootVisualElement.Query("horizontal-lr-grid-container").First().Add(_horizontalLRGridButton);
                _document.rootVisualElement.Query("horizontal-rl-grid-container").First().Add(_horizontalRLGridButton);
                _document.rootVisualElement.Query("vertical-tb-grid-container").First().Add(_verticalTBGridButton);
                _document.rootVisualElement.Query("vertical-bt-grid-container").First().Add(_verticalBTGridButton);

                _gridButtons[_gridSize * _gridSize + i] = _horizontalLRGridButton;
                _gridButtons[_gridSize * (_gridSize + 1) + i] = _horizontalRLGridButton;
                _gridButtons[_gridSize * (_gridSize + 2) + i] = _verticalTBGridButton;
                _gridButtons[_gridSize * (_gridSize + 3) + i] = _verticalBTGridButton;
            }
        }

        private void OnPlayCode(ClickEvent _event)
        {
            _copySessionCode.onClick.Invoke();
        }

        private void TurnFadeOut(TransitionEndEvent _event)
        {
            if (_document.rootVisualElement.Query("turn-information").First().style.opacity == 0)
            {
                _document.rootVisualElement.Query("turn-screen").First().style.display = DisplayStyle.None;
            }
            else
            {
                _document.rootVisualElement.Query("turn-information").First().style.opacity = 0;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (IsServer)
                _readyPlayers.Value = 0;

            onGameStart.AddListener(StartTurn);

            SetPlayCode();
        }

        private void StartTurn()
        {
            if (!IsHost)
                _document.rootVisualElement.Query("grid-cover").First().style.visibility = Visibility.Visible;
        }

        private async void SetPlayCode()
        {
            List<string> _joinedSessions = await MultiplayerService.Instance.GetJoinedSessionIdsAsync();

            foreach (ISession _session in MultiplayerService.Instance.Sessions.Values)
            {
                if (_joinedSessions.Contains(_session.Id))
                    _sessionCodeText.text = _session.Code;
            }

            _document.rootVisualElement.Query<Button>("play-code").First().text = _sessionCodeText.text;
        }

        private void OnPlayerTurnChange(ulong _previousValue, ulong _newValue)
        {
            _document.rootVisualElement.Query("turn-screen").First().style.display = DisplayStyle.Flex;

            if(NetworkManager.Singleton.LocalClientId == _newValue)
            {
                _document.rootVisualElement.Query("grid-cover").First().style.visibility = Visibility.Hidden;
                _document.rootVisualElement.Query("turn-information").First().RemoveFromClassList("their-turn");
                _document.rootVisualElement.Query("turn-information").First().AddToClassList("your-turn");
            }
            else
            {
                _document.rootVisualElement.Query("grid-cover").First().style.visibility = Visibility.Visible;
                _document.rootVisualElement.Query("turn-information").First().RemoveFromClassList("your-turn");
                _document.rootVisualElement.Query("turn-information").First().AddToClassList("their-turn");
            }

            _document.rootVisualElement.Query("turn-information").First().style.opacity = 1;
        }

        private void OnMenu(ClickEvent _event)
        {
            throw new NotImplementedException();
        }

        private void OnAttack(ClickEvent _event)
        {
            _gridHandler.CheckTargetCellRpc(_targetCell);
            _document.rootVisualElement.Query("attack-button").First().SetEnabled(false);
            GetCellButton(_targetCell).UnregisterCallback<ClickEvent, byte>(SetTargetCell);
        }

        private void OnTorpedo(ClickEvent _event)
        {
            throw new NotImplementedException();
        }
        private void OnMine(ClickEvent _event)
        {
            _gridHandler.PlaceMineRpc(_targetCell);
            _document.rootVisualElement.Query<Button>("naval-mine-button").First().SetEnabled(false);
        }

        private void SetTargetCell(ClickEvent _event, byte _targetCell)
        {
            GetCellButton(this._targetCell).RemoveFromClassList("selected-grid-button");
            this._targetCell = _targetCell;
            GetCellButton(_targetCell).AddToClassList("selected-grid-button");

            if (_inPregame)
            {
                _document.rootVisualElement.Query<Button>("naval-mine-button").First().SetEnabled(true);
            }
            else 
            {
                _document.rootVisualElement.Query("attack-button").First().SetEnabled(true);
            }
        }

        private Button GetCellButton(byte _targetCell)
        {
            return _gridButtons[_targetCell];
        }

        private void Ready(ClickEvent _event)
        {
            onReady.Invoke();
            SetPlayerReadyRpc();
            Button readyButton = (Button)_document.rootVisualElement.Query("ready-button");
            readyButton.SetEnabled(false);
        }

        [Rpc(SendTo.Everyone)]
        private void StartGameRpc()
        {
            _document.rootVisualElement.Query("pregame-buttons").First().style.display = DisplayStyle.None;
            _document.rootVisualElement.Query("game-buttons").First().style.display = DisplayStyle.Flex;
            onGameStart.Invoke();
            _inPregame = false;
        }

        [Rpc(SendTo.Server)]
        private void SetPlayerReadyRpc()
        {
            _readyPlayers.Value ++;

            if (_readyPlayers.Value == _playerCount)
            {
                StartGameRpc();
            }
        }

        public void IsReady(bool _ready)
        {
            Button readyButton = (Button)_document.rootVisualElement.Query("ready-button");
            readyButton.SetEnabled(_ready);
        }

        [Rpc(SendTo.NotMe)]
        public void OnHitRpc(byte _targetCell)
        {
            if (IsClient)
            {
                GetCellButton(_targetCell).AddToClassList("hitted-grid-button");
                GetCellButton(_targetCell).RemoveFromClassList("grid-button");
            }
        }

        [Rpc(SendTo.NotMe)]
        public void OnMissRpc(byte _targetCell)
        {
            if (IsClient)
            {
                GetCellButton(_targetCell).AddToClassList("missed-grid-button");
                GetCellButton(_targetCell).RemoveFromClassList("grid-button");
            }
        }
    }
}
