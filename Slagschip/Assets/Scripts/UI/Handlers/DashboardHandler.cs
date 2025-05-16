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
using SceneManagement;
using System.Collections;

namespace UIHandlers
{
    [RequireComponent(typeof(UIDocument))]
    public class DashboardHandler : NetworkBehaviour
    {
        public UnityEvent onReady;
        public UnityEvent onGameStart;

        private UIDocument _document;

        [SerializeField] private string loadingScene;
        [SerializeField] private GridHandler _gridHandler;
        [SerializeField] private TMP_Text _sessionCodeText;
        [SerializeField] private UnityEngine.UI.Button _copySessionCode;
        [SerializeField] private UnityEngine.UI.Button _leaveSessionButton;

        private NetworkVariable<byte> _readyPlayers = new();
        private List<byte> _mineTargets = new();
        private bool _inPregame = true;

        private const byte _gridSize = GridHandler.gridSize;
        private const byte _playerCount = 2;

        private Button[] _gridButtons = new Button[_gridSize * (_gridSize + 4)];

        private byte _targetCell;

        Action<TransitionEndEvent> OnTutorialClose;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();

            _document.rootVisualElement.Query("turn-information").First().RegisterCallback<TransitionEndEvent>(OnTurnFadeEnd);
            _document.rootVisualElement.Query("menu-button").First().RegisterCallback<ClickEvent>(OnMenu);
            _document.rootVisualElement.Query("resume-button").First().RegisterCallback<ClickEvent>(OnResume);
            _document.rootVisualElement.Query("give-up-button").First().RegisterCallback<ClickEvent>(OnGiveUp);
            _document.rootVisualElement.Query("attack-button").First().RegisterCallback<ClickEvent>(OnAttack);
            _document.rootVisualElement.Query("torpedo-button").First().RegisterCallback<ClickEvent>(OnTorpedo);
            _document.rootVisualElement.Query("naval-mine-button").First().RegisterCallback<ClickEvent>(OnMine);
            _document.rootVisualElement.Query("ready-button").First().RegisterCallback<ClickEvent>(Ready);
            _document.rootVisualElement.Query("play-code").First().RegisterCallback<ClickEvent>(CopyPlayCode);
            _document.rootVisualElement.Query("to-start-button").First().RegisterCallback<ClickEvent>(OnToStart);

            GridHandler.instance.OnMove.AddListener(ShowRotateTutorial);

            foreach (VisualElement _button in _document.rootVisualElement.Query(className: "close-tutorial-button").ToList())
            {
                _button.RegisterCallback<ClickEvent, VisualElement>(CloseTutorial, _button);
            }

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

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            GameData.instance.currentPlayerTurn.OnValueChanged += OnPlayerTurnChange;

            if (IsServer)
                _readyPlayers.Value = 0;
            
            onGameStart.AddListener(StartTurn);

            SetPlayCode();

            if (IsHost)
            {
                _document.rootVisualElement.Query("team-name").First().AddToClassList("team-alfa");
                _document.rootVisualElement.Query<Label>("team-name").First().text = "AlfA";
            }
            else
            {
                _document.rootVisualElement.Query("team-name").First().AddToClassList("team-bravo");
                _document.rootVisualElement.Query<Label>("team-name").First().text = "BrAvo";
            }
        }

        private IEnumerator FadeOutTutorial(VisualElement _visualElement)
        {
            yield return null;

            _visualElement.style.opacity = 0;
        }

        private void ShowRotateTutorial(Vector3 _position)
        {
            GridHandler.instance.OnMove.RemoveListener(ShowRotateTutorial);

            ShowVisualElement(_document.rootVisualElement.Query("rotate-tutorial").First());
            StartCoroutine(FadeOutTutorial(_document.rootVisualElement.Query("rotate-tutorial").First()));
        }

        private void ShowVisualElement(VisualElement _visualElement)
        {
            _visualElement.style.visibility = Visibility.Visible;
        }

        private void HideVisualElement(VisualElement _visualElement)
        {
            _visualElement.style.visibility = Visibility.Hidden;
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
                {
                    _sessionCodeText.text = _session.Code;
                    break;
                }
            }

            _document.rootVisualElement.Query<Button>("play-code").First().text = _sessionCodeText.text;

            if (IsHost)
            {
                ShowVisualElement(_document.rootVisualElement.Query("play-code-tutorial").First());
                StartCoroutine(FadeOutTutorial(_document.rootVisualElement.Query("play-code-tutorial").First()));
            }
        }

        private void CopyPlayCode(ClickEvent _event)
        {
            _copySessionCode.onClick.Invoke();
        }

        private void OnTurnFadeEnd(TransitionEndEvent _event)
        {
            VisualElement _turnInformation = _document.rootVisualElement.Query("turn-information").First();

            if (_turnInformation.style.opacity == 0)
            {
                HideVisualElement(_document.rootVisualElement.Query("turn-screen").First());
            }
            else
            {
                VisualElement _turnTeamName = _document.rootVisualElement.Query("turn-team-name").First();

                if (GameData.instance.currentPlayerTurn.Value == 0)
                {
                    _turnTeamName.AddToClassList("team-alfa");
                    _turnTeamName.RemoveFromClassList("team-bravo");

                    _document.rootVisualElement.Query<Label>("turn-team-name").First().text = "AlfA";
                }
                else
                {
                    _turnTeamName.AddToClassList("team-bravo");
                    _turnTeamName.RemoveFromClassList("team-alfa");

                    _document.rootVisualElement.Query<Label>("turn-team-name").First().text = "BrAvo";
                }

                _turnInformation.style.opacity = 0;
            }
        }

        private void OnPlayerTurnChange(ulong _previousValue, ulong _newValue)
        {
            ShowVisualElement(_document.rootVisualElement.Query("turn-screen").First());

            if (NetworkManager.Singleton.LocalClientId == _newValue)
            {
                HideVisualElement(_document.rootVisualElement.Query("grid-cover").First());
            }
            else
            {
                ShowVisualElement(_document.rootVisualElement.Query("grid-cover").First());
            }

            VisualElement _teamText = _document.rootVisualElement.Query("team-text").First();

            if (_newValue == 0)
            {
                _teamText.AddToClassList("team-alfa");
                _teamText.RemoveFromClassList("team-bravo");

                _document.rootVisualElement.Query<Label>("team-text").First().text = "AlfA";
            }
            else
            {
                _teamText.AddToClassList("team-bravo");
                _teamText.RemoveFromClassList("team-alfa");

                _document.rootVisualElement.Query<Label>("team-text").First().text = "BrAvo";
            }

            _document.rootVisualElement.Query("turn-information").First().style.opacity = 1;
        }

        private void OnMenu(ClickEvent _event)
        {
            ShowVisualElement(_document.rootVisualElement.Query("pause-screen").First());
        }

        private void OnResume(ClickEvent _event)
        {
            HideVisualElement(_document.rootVisualElement.Query("pause-screen").First());
        }

        private void LoadMenu()
        {
            SceneLoader.instance.LoadScene(loadingScene);
        }

        private void LoadMenu(ulong _clientId)
        {
            NetworkManager.OnClientDisconnectCallback -= LoadMenu;
            SceneLoader.instance.LoadScene(loadingScene);
        }

        private void OnGiveUp(ClickEvent _event)
        {
            ShowDisconnectedScreenRpc();

            NetworkManager.OnClientDisconnectCallback += LoadMenu;

            LeaveSessionRpc();
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void LeaveSessionRpc()
        {
            _leaveSessionButton.onClick.Invoke();
        }

        [Rpc(SendTo.NotMe)]
        private void ShowDisconnectedScreenRpc()
        {
            ShowVisualElement(_document.rootVisualElement.Query("disconnected-screen").First());
        }

        private void OnToStart(ClickEvent _event)
        {
            LoadMenu();
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
            if (_mineTargets.Contains(_targetCell))
                return;

            _mineTargets.Add(_targetCell);
            _gridHandler.PlaceMineRpc(_targetCell);
            _document.rootVisualElement.Query<Button>("naval-mine-button").First().SetEnabled(false);
            if (IsClient)
            {
                GetCellButton(_targetCell).AddToClassList("mine-grid-button");
            }
        }

        private void SetTargetCell(ClickEvent _event, byte _targetCell)
        {
            GetCellButton(this._targetCell).RemoveFromClassList("selected-grid-button");
            this._targetCell = _targetCell;
            GetCellButton(_targetCell).AddToClassList("selected-grid-button");

            if (_inPregame)
            {
                if (_mineTargets.Contains(_targetCell))
                    return;

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
            HideVisualElement(_document.rootVisualElement.Query("pregame-buttons").First());
            ShowVisualElement(_document.rootVisualElement.Query("game-buttons").First());

            onGameStart.Invoke();
            _inPregame = false;

            ShowAttackTutorialRpc();
        }

        [Rpc(SendTo.Me)]
        private void ShowAttackTutorialRpc()
        {
            _document.rootVisualElement.Query("attack-tutorial").First().RegisterCallbackOnce<TransitionEndEvent>(OnAttackTutorialFadeEnd);

            OnTutorialClose += OnAttackTutorialFadeEnd;

            ShowVisualElement(_document.rootVisualElement.Query("attack-tutorial").First());
            StartCoroutine(FadeOutTutorial(_document.rootVisualElement.Query("attack-tutorial").First()));
        }

        private void OnAttackTutorialFadeEnd(TransitionEndEvent _event)
        {
            OnTutorialClose -= OnAttackTutorialFadeEnd;

            ShowVisualElement(_document.rootVisualElement.Query("torpedo-tutorial").First());
            StartCoroutine(FadeOutTutorial(_document.rootVisualElement.Query("torpedo-tutorial").First()));
        }

        [Rpc(SendTo.Server)]
        private void SetPlayerReadyRpc()
        {
            _readyPlayers.Value++;

            if (_readyPlayers.Value == _playerCount)
                StartGameRpc();
        }

        public void IsReady(bool _ready)
        {
            Button readyButton = _document.rootVisualElement.Query<Button>("ready-button");

            readyButton.SetEnabled(_ready);
        }

        [Rpc(SendTo.NotMe)]
        public void OnHitRpc(byte _targetCell)
        {
            if (IsClient)
            {
                GetCellButton(_targetCell).AddToClassList("hitted-grid-button");
                GetCellButton(_targetCell).RemoveFromClassList("grid-button");
                GetCellButton(_targetCell).RemoveFromClassList("mine-grid-button");
            }
        }

        [Rpc(SendTo.NotMe)]
        public void OnMissRpc(byte _targetCell)
        {
            if (IsClient)
            {
                GetCellButton(_targetCell).AddToClassList("missed-grid-button");
                GetCellButton(_targetCell).RemoveFromClassList("grid-button");
                GetCellButton(_targetCell).RemoveFromClassList("mine-grid-button");
            }
        }

        [Rpc(SendTo.NotMe)]
        public void LockGridButtonRpc(byte _targetCell)
        {
            if (IsClient)
            {
                GetCellButton(_targetCell).UnregisterCallback<ClickEvent, byte>(SetTargetCell);
            }
        }

        private void CloseTutorial(ClickEvent _event, VisualElement _visualElement)
        {
            _visualElement.parent.style.display = DisplayStyle.None;

            OnTutorialClose?.Invoke(null);
        }
    }
}
