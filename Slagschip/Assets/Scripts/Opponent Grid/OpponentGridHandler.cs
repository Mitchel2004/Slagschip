using PlayerGrid;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpponentGrid
{
    [RequireComponent(typeof(UIDocument))]
    public class OpponentGridHandler : MonoBehaviour
    {
        private UIDocument _document;

        [SerializeField] private GridHandler _gridHandler;

        private const byte _gridSize = GridHandler.gridSize;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        
            for (byte i = 1; i <= _gridSize * _gridSize; i++)
            {
                Button _gridButton = new();

                _gridButton.AddToClassList("grid-button");

                _gridButton.style.width = new StyleLength(new Length(100 / _gridSize, LengthUnit.Percent));
                _gridButton.style.height = new StyleLength(new Length(100 / _gridSize, LengthUnit.Percent));

                _gridButton.RegisterCallbackOnce<ClickEvent, byte>(SetTargetCell, i);

                _document.rootVisualElement.Query("grid-container").First().Add(_gridButton);
            }

            for(byte i = 1; i <= _gridSize * 2; i++)
            {
                Button _horizontalGridButton = new();
                Button _verticalGridButton = new();

                _horizontalGridButton.AddToClassList("horizontal-grid-button");
                _verticalGridButton.AddToClassList("vertical-grid-button");

                _horizontalGridButton.style.height = new StyleLength(new Length(100 / _gridSize, LengthUnit.Percent));
                _verticalGridButton.style.width = new StyleLength(new Length(100 / _gridSize, LengthUnit.Percent));

                _horizontalGridButton.RegisterCallbackOnce<ClickEvent, byte>(SetTargetCell, (byte)(_gridSize * _gridSize + i));
                _verticalGridButton.RegisterCallbackOnce<ClickEvent, byte>(SetTargetCell, (byte)(_gridSize * _gridSize + _gridSize * 2 + i));

                _document.rootVisualElement.Query("horizontal-grid-container").First().Add(_horizontalGridButton);
                _document.rootVisualElement.Query("vertical-grid-container").First().Add(_verticalGridButton);
            }
        }

        private void SetTargetCell(ClickEvent _event, byte _targetCell)
        {
            _gridHandler.CheckTargetCellRpc(_targetCell);
        }
    }
}
