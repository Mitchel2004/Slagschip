using PlayerGrid;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpponentGrid
{
    [RequireComponent(typeof(UIDocument))]
    public class OpponentGridHandler : NetworkBehaviour
    {
        private UIDocument _document;

        [SerializeField] private GridHandler _gridHandler;

        private const byte _gridSize = GridHandler.gridSize;

        private Button[] _gridButtons = new Button[_gridSize * (_gridSize + 4)];

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        
            for (byte i = 0; i < _gridSize * _gridSize; i++)
            {
                Button _gridButton = new();

                _gridButton.AddToClassList("grid-button");

                _gridButton.style.width = new StyleLength(new Length(100 / _gridSize, LengthUnit.Percent));
                _gridButton.style.height = new StyleLength(new Length(100 / _gridSize, LengthUnit.Percent));

                _gridButton.RegisterCallbackOnce<ClickEvent, byte>(SetTargetCell, i);

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

                _horizontalLRGridButton.RegisterCallbackOnce<ClickEvent, byte>(SetTargetCell, (byte)(_gridSize * _gridSize + i));
                _horizontalRLGridButton.RegisterCallbackOnce<ClickEvent, byte>(SetTargetCell, (byte)(_gridSize * (_gridSize + 1) + i));
                _verticalTBGridButton.RegisterCallbackOnce<ClickEvent, byte>(SetTargetCell, (byte)(_gridSize * (_gridSize + 2) + i));
                _verticalBTGridButton.RegisterCallbackOnce<ClickEvent, byte>(SetTargetCell, (byte)(_gridSize * (_gridSize + 3) + i));

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

        private void SetTargetCell(ClickEvent _event, byte _targetCell)
        {
            _gridHandler.CheckTargetCellRpc(_targetCell);
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
