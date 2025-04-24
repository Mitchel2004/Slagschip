using Multiplayer;
using PlayerGrid;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpponentGrid
{
    [RequireComponent(typeof(UIDocument))]
    public class DashboardHandler : NetworkBehaviour
    {
        private UIDocument _document;

        [SerializeField] private GameData _gameData;
        [SerializeField] private GridHandler _gridHandler;

        private const byte _gridSize = GridHandler.gridSize;

        private Button[] _gridButtons = new Button[_gridSize * (_gridSize + 4)];

        private byte _targetCell;

        private void Awake()
        {
            _gameData.currentPlayerTurn.OnValueChanged += OnPlayerTurnChange;

            _document = GetComponent<UIDocument>();

            _document.rootVisualElement.Query("menu-button").First().RegisterCallback<ClickEvent>(OnMenu);
            _document.rootVisualElement.Query("attack-button").First().RegisterCallback<ClickEvent>(OnAttack);
            _document.rootVisualElement.Query("torpedo-button").First().RegisterCallback<ClickEvent>(OnTorpedo);

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

            if (!IsHost)
                _document.rootVisualElement.Query("grid-cover").First().style.visibility = Visibility.Visible;
        }

        private void OnPlayerTurnChange(ulong _previousValue, ulong _newValue)
        {
            if (NetworkManager.Singleton.LocalClientId == _newValue)
            {
                _document.rootVisualElement.Query("grid-cover").First().style.visibility = Visibility.Hidden;
            }
            else
            {
                _document.rootVisualElement.Query("grid-cover").First().style.visibility = Visibility.Visible;
            }
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

        private void SetTargetCell(ClickEvent _event, byte _targetCell)
        {
            GetCellButton(this._targetCell).RemoveFromClassList("selected-grid-button");
            this._targetCell = _targetCell;
            GetCellButton(_targetCell).AddToClassList("selected-grid-button");
            _document.rootVisualElement.Query("attack-button").First().SetEnabled(true);
        }

        private Button GetCellButton(byte _targetCell)
        {
            return _gridButtons[_targetCell];
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
