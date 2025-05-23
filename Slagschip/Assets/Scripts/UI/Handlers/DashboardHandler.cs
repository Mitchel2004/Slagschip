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
using Ships;
using SceneManagement;
using System.Collections;
using System.Linq;
using UnityEngine.InputSystem;
using Utilities.CompassDirections;
using static UnityEngine.Rendering.DebugUI.MessageBox;

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
        [SerializeField] private ShipPlacer _shipPlacer;
        [SerializeField] private TMP_Text _sessionCodeText;
        [SerializeField] private UnityEngine.UI.Button _copySessionCode;
        [SerializeField] private UnityEngine.UI.Button _leaveSessionButton;
        [SerializeField] private AudioSource winSource, loseSource;

        private NetworkVariable<byte> _readyPlayers = new();
        private List<byte> _mineTargets = new();
        private bool _inPregame = true;

        private const byte GridSize = GridHandler.gridSize;
        private const byte PlayerCount = 2;

        private Dictionary<byte, Button> _gridButtons = new();
        private Dictionary<byte, Button> _horizontalGridButtons = new();
        private Dictionary<byte, Button> _verticalGridButtons = new();

        private byte _targetCell;
        private ECompassDirection _torpedoDirection = ECompassDirection.NorthEast;
        private byte _mineCount = GridHandler._maxMines;

        private byte _torpedoCount;
        private bool _showTorpedoReceivedTutorial = true;

        private InputAction _rotate;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();

            _rotate = InputSystem.actions.FindAction("Rotate");
            
            Query("turn-information").RegisterCallback<TransitionEndEvent>(e => OnTurnFadeEnd());

            RegisterCallbacks(new Dictionary<string, Action> {
                {"menu-button", OnMenu},
                {"resume-button", OnResume},
                {"give-up-button", OnGiveUp},
                {"attack-button", OnAttack},
                {"torpedo-button", OnTorpedo},
                {"naval-mine-button", OnMine},
                {"ready-button", Ready},
                {"play-code", CopyPlayCode},
                {"to-start-button", OnToStart},
                {"leave-button", OnLeave}
            });
            _gridHandler.OnIsReady.AddListener(IsReady);

            _shipPlacer.DonePlacing.AddListener(HideShipLabel);

            BindTutorialCloseCallbacks(new Dictionary<string, Action> {
                {"attack-tutorial", () => ShowTutorial("torpedo-tutorial") }
            });

            RegisterShipCallbacks(new string[]
            {
                ShipButtonId(EShip.Schiedam),
                ShipButtonId(EShip.VanAmstel),
                ShipButtonId(EShip.VanSpeijk),
                ShipButtonId(EShip.DeRuyter),
                ShipButtonId(EShip.JohanDeWitt)
            });
            
            CreateGridButtons();

            RegisterBorderButtons(
                "horizontal-lr-grid-container",
                ECompassDirection.East, 
                i => i,
                _horizontalGridButtons
            );

            RegisterBorderButtons(
                "horizontal-rl-grid-container",
                ECompassDirection.West,
                i => i + 90,
                _horizontalGridButtons
            );

            RegisterBorderButtons(
                "vertical-tb-grid-container",
                ECompassDirection.South,
                i => i * 10 + 9,
                _verticalGridButtons
            );

            RegisterBorderButtons(
                "vertical-bt-grid-container",
                ECompassDirection.North,
                i => i * 10,
                _verticalGridButtons
            );
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            GameData.instance.currentPlayerTurn.OnValueChanged += OnPlayerTurnChange;

            Query<Label>("naval-mine-counter").text = _mineCount.ToString();
            Query<Label>("torpedo-counter").text = _torpedoCount.ToString();

            if (IsServer)
                _readyPlayers.Value = 0;

            onGameStart.AddListener(StartTurn);
            SetPlayCode();

            var teamClass = IsHost ? "team-alfa" : "team-bravo";
            Query("team-name").AddToClassList(teamClass);
            var teamName = IsHost ? "Alfa" : "Bravo";
            Query<Label>("team-name").text = teamName;

            if (IsHost)
            {
                HidePregame();
                AwaitPlayers();
            }
        }

        private void RegisterCallbacks(Dictionary<string, Action> callbacks)
        {
            foreach (var kv in callbacks)
            {
                var btn = Query<Button>(kv.Key);
                btn.clicked += kv.Value;
            }
        }
        private void RegisterShipCallbacks(string[] shipIds)
        {
            for (int i = 0; i < shipIds.Length; i++)
            {
                var shipType = (EShip)i;
                var shipId = shipIds[i];
                var element = Query(shipId);
                element.RegisterCallback<ClickEvent>(e => TryPlaceShip(shipType));
                element.RegisterCallback<FocusEvent>(e => ShowShipLabel(shipType));
                element.RegisterCallback<FocusOutEvent>(e => HideShipLabel());
                element.RegisterCallback<MouseOverEvent>(e => ShowShipLabel(shipType));
                element.RegisterCallback<MouseLeaveEvent>(e => HideShipLabel());
            }
        }
        private void BindTutorialCloseCallbacks(Dictionary<string, Action> callbacks)
        {
            foreach (var btn in FindElementsByClass<Button>("close-tutorial-button"))
            {
                btn.clicked += () => CloseTutorial(btn);
                if (callbacks.ContainsKey(btn.parent.name))
                {
                    btn.clicked += callbacks[btn.parent.name];
                }
            }
        }
        private void CreateGridButtons()
        {
            byte totalGridCells = GridSize * GridSize;

            for (byte i = 0 ; i < totalGridCells; i++)
            {
                Button gridButton = new();

                gridButton.AddToClassList("grid-button");
                gridButton.style.width = new StyleLength(new Length(100f / GridSize, LengthUnit.Percent));
                gridButton.style.height = new StyleLength(new Length(100f / GridSize, LengthUnit.Percent));
                gridButton.RegisterCallback<ClickEvent, byte>(SetTargetCell, i);

                Query("grid-container").Add(gridButton);

                _gridButtons.Add(i, gridButton);
            }
        }

        private void RegisterBorderButtons(string containerName, ECompassDirection direction, Func<int, int> indexCalculator, Dictionary<byte, Button> dictionary)
        {
            List<Button> buttons = QueryAll(containerName).Children<Button>().ToList();
            byte totalGridCells = GridSize * GridSize;

            for (int i = 0; i < buttons.Count; i++)
            {
                byte index = (byte)(totalGridCells + indexCalculator(i));
                Button button = buttons[i];

                button.RegisterCallback<ClickEvent, KeyValuePair<byte, ECompassDirection>>(
                    SetTargetCell,
                    new KeyValuePair<byte, ECompassDirection>(index, direction)
                );

                dictionary.Add(index, button);
            }
        }

        [Rpc(SendTo.Everyone)]
        private void ShowNavalMineTutorialRpc() => ShowTutorial("naval-mine-tutorial");

        private void AwaitPlayers() => StartCoroutine(WaitForPlayersRoutine());

        private IEnumerator WaitForPlayersRoutine()
        {
            while (NetworkManager.ConnectedClients.Count != PlayerCount)
                yield return null;

            ShowPregame();
            BindRotateTutorialRpc();
        }
        private void ShowPregame()
        {
            ShowElement("pregame-buttons");
            HideElement("awaiting-players");
            ShowElement("selection-container");

            ShowNavalMineTutorialRpc();
        }
        private void HidePregame()
        {
            HideElement("pregame-buttons");
            ShowElement("awaiting-players");
            HideElement("selection-container");
        }

        [Rpc(SendTo.Everyone)]
        private void BindRotateTutorialRpc()
        {
            _gridHandler.OnMove.AddListener(ShowRotateTutorial);
        }

        private void TryPlaceShip(EShip ship)
        {
            if (_shipPlacer.IsPlacing)
                return;
                
            _shipPlacer.PlaceShip(ship);

            ShowShipLabel(ship);
            ToggleButton(ShipButtonId(ship), false);
        }
        private void ShowShipLabel(EShip ship)
        {
            if (_shipPlacer.IsPlacing)
                return;
            ShowElement("ship-label");
            Query<Label>("ship-label").text = ShipName(ship);
        }
        private void HideShipLabel()
        {
            if (_shipPlacer.IsPlacing)
                return;
            HideElement("ship-label");
        }
        private string ShipName(EShip ship) => ship switch
        {
            EShip.Schiedam => "Zr.Ms. Schiedam",
            EShip.VanAmstel => "Zr.Ms. Van Amstel",
            EShip.VanSpeijk => "Zr.Ms. Van Speijk",
            EShip.DeRuyter => "Zr.Ms. De Ruyter",
            EShip.JohanDeWitt => "Zr.Ms. Johan De Witt",
            _ => string.Empty
        };
        private string ShipButtonId(EShip ship) => ship switch
        {
            EShip.Schiedam => "ship-button-schiedam",
            EShip.VanAmstel => "ship-button-vanamstel",
            EShip.VanSpeijk => "ship-button-vanspeijk",
            EShip.DeRuyter => "ship-button-deruyter",
            EShip.JohanDeWitt => "ship-button-johandewitt",
            _ => string.Empty
        };

        private void RotateTorpedoOrientation(InputAction.CallbackContext _context)
        {
            bool isHorizontal = Query("horizontal-lr-grid-container").style.visibility == Visibility.Visible;

            HideElement(isHorizontal ? "horizontal-lr-grid-container" : "vertical-tb-grid-container");
            HideElement(isHorizontal ? "horizontal-rl-grid-container" : "vertical-bt-grid-container");
            ShowElement(isHorizontal ? "vertical-tb-grid-container" : "horizontal-lr-grid-container");
            ShowElement(isHorizontal ? "vertical-bt-grid-container" : "horizontal-rl-grid-container");

            ToggleButton("attack-button", false);

            if (_targetCell >= 100)
            {
                if (IsHorizontal(_torpedoDirection))
                {
                    _horizontalGridButtons[_targetCell].Children().First().RemoveFromClassList("selected-torpedo-button");
                }
                else
                {
                    _verticalGridButtons[_targetCell].Children().First().RemoveFromClassList("selected-torpedo-button");
                }
            }

            _targetCell = 100;
        }

        private void ToggleTorpedoMode(bool _toggle)
        {
            if (_toggle)
            {
                Query("torpedo-button").AddToClassList("selected-torpedo-button");

                ShowElement("horizontal-lr-grid-container");
                ShowElement("horizontal-rl-grid-container");

                _rotate.started += RotateTorpedoOrientation;
                GetCellButton(_targetCell).RemoveFromClassList("selected-grid-button");
            }
            else
            {
                Query("torpedo-button").RemoveFromClassList("selected-torpedo-button");

                HideElement("horizontal-lr-grid-container");
                HideElement("horizontal-rl-grid-container");
                HideElement("vertical-tb-grid-container");
                HideElement("vertical-bt-grid-container");

                _rotate.started -= RotateTorpedoOrientation;

                if (_targetCell >= 100)
                {
                    if (IsHorizontal(_torpedoDirection))
                    {
                        _horizontalGridButtons[_targetCell].Children().First().RemoveFromClassList("selected-torpedo-button");
                    }
                    else
                    {
                        _verticalGridButtons[_targetCell].Children().First().RemoveFromClassList("selected-torpedo-button");
                    }
                }
                _targetCell = 0;
            }

            _document.rootVisualElement.Query("attack-button").First().SetEnabled(false);
        }

        private void StartTurn()
        {
            if (!IsHost)
                ShowElement("grid-cover");
        }

        private void OnTurnFadeEnd()
        {
            VisualElement _turnInformation = Query("turn-information");

            if (_turnInformation.style.opacity == 0)
            {
                HideElement("turn-screen");
            }
            else
            {
                StyleTeamName(GameData.instance.currentPlayerTurn.Value, Query<Label>("turn-team-name"));

                _turnInformation.style.opacity = 0;
            }
        }

        private void OnPlayerTurnChange(ulong previousValue, ulong newValue)
        {
            ShowElement("turn-screen");

            if (NetworkManager.Singleton.LocalClientId == newValue)
            {
                HideElement("grid-cover");

                if (_torpedoCount > 0)
                    ToggleButton("torpedo-button", true);
            }
            else
            {
                ShowElement("grid-cover");
                ToggleButton("torpedo-button", false);
            }

            VisualElement _teamText = _document.rootVisualElement.Query("team-text").First();

            StyleTeamName(newValue, Query<Label>("team-text"));

            Query("turn-information").style.opacity = 1;
        }

        private void StyleTeamName(ulong teamId, Label label)
        {
            bool isAlfa = teamId == 0;
            var currentTeam = isAlfa ? "team-alfa" : "team-bravo";
            var otherTeam = !isAlfa ? "team-alfa" : "team-bravo";

            label.AddToClassList(currentTeam);
            label.RemoveFromClassList(otherTeam);

            label.text = isAlfa ? "Alfa" : "Bravo";
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

        private void OnLeave()
        {
            LoadMenu();
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void LeaveSessionRpc()
        {
            _leaveSessionButton.onClick.Invoke();
        }

        [Rpc(SendTo.NotMe)]
        private void ShowDisconnectedScreenRpc()
        {
            ShowElement("disconnected-screen");
        }

        private void SetTargetCell(ClickEvent e, byte targetCell)
        {
            GetCellButton(_targetCell).RemoveFromClassList("selected-grid-button");
            _targetCell = targetCell;
            GetCellButton(targetCell).AddToClassList("selected-grid-button");

            if (_inPregame)
            {
                ToggleButton("naval-mine-button", false);
                if (_mineTargets.Contains(targetCell))
                    return;
                if (!_gridHandler.MineAllowed())
                    return;

                ToggleButton("naval-mine-button", true);
            }
            else 
            {
                ToggleButton("attack-button", true);
            }
        }

        private void SetTargetCell(ClickEvent _event, KeyValuePair<byte, ECompassDirection> _targetCell)
        {
            _torpedoDirection = _targetCell.Value;

            if (this._targetCell >= 100)
            {
                if (IsHorizontal(_torpedoDirection))
                {
                    _horizontalGridButtons[this._targetCell].Children().First().RemoveFromClassList("selected-torpedo-button");
                }
                else
                {
                    _verticalGridButtons[this._targetCell].Children().First().RemoveFromClassList("selected-torpedo-button");
                }
            }

            this._targetCell = _targetCell.Key;

            if (IsHorizontal(_torpedoDirection))
            {
                _horizontalGridButtons[_targetCell.Key].Children().First().AddToClassList("selected-torpedo-button");
            }
            else
            {
                _verticalGridButtons[_targetCell.Key].Children().First().AddToClassList("selected-torpedo-button");
            }

            ToggleButton("attack-button", true);
        }

        private Button GetCellButton(byte _targetCell)
        {
            return _gridButtons[_targetCell];
        }

        [Rpc(SendTo.Everyone)]
        private void StartGameRpc()
        {
            HideElement("pregame-buttons");
            ShowElement("game-buttons");
            HideElement("selection-container");

            Camera.main.transform.position += Vector3.forward;

            onGameStart.Invoke();
            _inPregame = false;

            ShowTutorial("attack-tutorial", () => ShowTutorial("torpedo-tutorial"));
        }

        private void FadeOutTutorial(string tutorialName) => StartCoroutine(FadeOutTutorialRoutine(Query(tutorialName)));

        private IEnumerator FadeOutTutorialRoutine(VisualElement _visualElement)
        {
            yield return null;

            _visualElement.style.opacity = 0;
        }

        private void ShowTutorial(string tutorialName)
        {
            ShowElement(tutorialName);
            Query(tutorialName).RegisterCallbackOnce<TransitionEndEvent>(CloseTutorial);
            FadeOutTutorial(tutorialName);
        }
        private void ShowTutorial(string tutorialName, Action fadeEndCallback)
        {
            Query(tutorialName).RegisterCallbackOnce<TransitionEndEvent>(e => fadeEndCallback());
            Query(tutorialName).RegisterCallbackOnce<TransitionEndEvent>(CloseTutorial);
            ShowTutorial(tutorialName);
        }

        private void CloseTutorial(VisualElement _visualElement)
        {
            _visualElement.parent.style.display = DisplayStyle.None;
        }
        private void CloseTutorial(TransitionEndEvent e)
        {
            VisualElement elem = (VisualElement)e.target;
            elem.style.display = DisplayStyle.None;
        }

        private void ShowRotateTutorial(Vector3 _position)
        {
            _gridHandler.OnMove.RemoveListener(ShowRotateTutorial);

            ShowTutorial("rotate-tutorial");
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

            Query<Button>("play-code").text = _sessionCodeText.text;

            if (IsHost)
                ShowTutorial("play-code-tutorial");
        }

        [Rpc(SendTo.Server)]
        private void SetPlayerReadyRpc()
        {
            if (++_readyPlayers.Value == PlayerCount)
                StartGameRpc();
        }

        public void IsReady(bool _ready)
        {
            ToggleButton("ready-button", _ready);
        }

        [Rpc(SendTo.NotMe)]
        public void OnHitRpc(byte targetCell)
        {
            StyleGridButton("hitted-grid-button", targetCell);
        }

        [Rpc(SendTo.NotMe)]
        public void OnMissRpc(byte targetCell)
        {
            StyleGridButton("missed-grid-button", targetCell);
        }
        private void StyleGridButton(string styleClass, byte targetCell)
        {
            if (IsClient)
            {
                GetCellButton(targetCell).AddToClassList(styleClass);
                GetCellButton(targetCell).RemoveFromClassList("grid-button");
                GetCellButton(targetCell).RemoveFromClassList("mine-grid-button");
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

        private void OnMenu()
        {
            ShowElement("pause-screen");
        }

        private void OnResume()
        {
            HideElement("pause-screen");
        }

        private void OnGiveUp()
        {
            ShowDisconnectedScreenRpc();
            NetworkManager.OnClientDisconnectCallback += LoadMenu;
            LeaveSessionRpc();
        }

        public void ReceiveTorpedo()
        {
            if (_showTorpedoReceivedTutorial)
            {
                _showTorpedoReceivedTutorial = false;
                ShowTutorial("torpedo-received-tutorial");
            }

            Query<Label>("torpedo-counter").text = (++_torpedoCount).ToString();

            ToggleButton("torpedo-button", true);
        }

        private void OnTorpedo()
        {
            ToggleTorpedoMode(!Query("torpedo-button").ClassListContains("selected-torpedo-button"));
        }

        private void OnMine()
        {
            if (_mineTargets.Contains(_targetCell))
                return;

            _mineTargets.Add(_targetCell);
            _gridHandler.PlaceMineRpc(_targetCell);
            Query<Label>("naval-mine-counter").text = (--_mineCount).ToString();
            ToggleButton("naval-mine-button", false);

            if (IsClient)
            {
                GetCellButton(_targetCell).AddToClassList("mine-grid-button");
            }
        }

        private void OnAttack()
        {
            if (_torpedoDirection != ECompassDirection.NorthEast)
            {
                _gridHandler.CheckTargetCellRpc(_targetCell, _torpedoDirection);

                if (IsHorizontal(_torpedoDirection))
                {
                    _horizontalGridButtons[_targetCell].Children().First().RemoveFromClassList("selected-torpedo-button");
                    _horizontalGridButtons[_targetCell].Children().First().AddToClassList("fired-torpedo-button");
                    _horizontalGridButtons[_targetCell].UnregisterCallback<ClickEvent, KeyValuePair<byte, ECompassDirection>>(SetTargetCell);
                }
                else
                {
                    _verticalGridButtons[_targetCell].Children().First().RemoveFromClassList("selected-torpedo-button");
                    _verticalGridButtons[_targetCell].Children().First().AddToClassList("fired-torpedo-button");
                    _verticalGridButtons[_targetCell].UnregisterCallback<ClickEvent, KeyValuePair<byte, ECompassDirection>>(SetTargetCell);
                }

                _torpedoDirection = ECompassDirection.NorthEast;
                Query<Label>("torpedo-counter").text = (--_torpedoCount).ToString();

                if (_torpedoCount <= 0)
                    ToggleButton("torpedo-button", false);
            }
            else
            {
                _gridHandler.CheckTargetCellRpc(_targetCell);
                GetCellButton(_targetCell).UnregisterCallback<ClickEvent, byte>(SetTargetCell);
            }

            ToggleButton("attack-button", false);

            ToggleTorpedoMode(false);
        }

        private void Ready()
        {
            onReady.Invoke();
            SetPlayerReadyRpc();
            ToggleButton("ready-button", false);
        }


        public void LoseScreen()
        {
            WinLoseScreenStyle("lose-holder", () => loseSource.Play());
            WinScreenRPC();

            LeaveSessionRpc();
        }

        [Rpc(SendTo.NotMe)]
        public void WinScreenRPC()
        {
            WinLoseScreenStyle("win-holder", () => winSource.Play());
            LeaveSessionRpc();
        }

        private void WinLoseScreenStyle(string name, Action appearEvent)
        {
            ShowElement("win-lose-screen");
            Query(name).style.display = DisplayStyle.Flex;
            Query("win-lose-pop-up").AddToClassList("appear");
            Query("win-lose-pop-up").RegisterCallbackOnce<TransitionEndEvent>(e => appearEvent.Invoke());
        }

        private void CopyPlayCode() => _copySessionCode.onClick.Invoke();

        private void OnToStart() => LoadMenu();
        
        private T Query<T>(string name) where T : VisualElement => _document.rootVisualElement.Q<T>(name);
        private VisualElement Query(string name) => _document.rootVisualElement.Q(name);
        private UQueryBuilder<VisualElement> QueryAll(string name) => _document.rootVisualElement.Query(name);
        private List<T> FindElementsByClass<T>(string className) where T : VisualElement => _document.rootVisualElement.Query<T>(className: className).ToList();
        private List<VisualElement> FindElementsByClass(string className) => _document.rootVisualElement.Query(className: className).ToList();

        private bool IsHorizontal(ECompassDirection direction) => direction == ECompassDirection.East || direction == ECompassDirection.West;

        private void ShowElement(string name) => Query(name).style.visibility = Visibility.Visible;

        private void HideElement(string name) => Query(name).style.visibility = Visibility.Hidden;

        private void ToggleButton(string buttonName, bool enabled)
        {
            Query(buttonName).SetEnabled(enabled);
        }
    }
}
