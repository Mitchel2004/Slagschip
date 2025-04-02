using Ships;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace PlayerGrid
{
    public class GridHandler : MonoBehaviour
    {
        public static GridHandler instance;

        public UnityEvent<bool> onValidate;
        public UnityEvent<Vector2> onMove;
        public UnityEvent<bool> onHit;

        private const byte _gridSize = 10;
        private GridCell[,] _grid;
        private GridCell _current;

        private InputAction _rotateLeft, _rotateRight;

        private ShipBehaviour _ship;

        [SerializeField] private LayerMask interactionLayers;

        public ShipBehaviour Ship
        {
            private get 
            { 
                return _ship; 
            }
            set 
            {
                _ship = value;
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(this);
            }

            InitializeGrid();

            InitializeInput();
        }

        private void InitializeInput()
        {
            _rotateLeft = InputSystem.actions.FindAction("RotateLeft");
            _rotateRight = InputSystem.actions.FindAction("RotateRight");

            _rotateLeft.started += context => {
                if (_ship != null)
                    _ship.shape.RotateCounterClockwise();
                onValidate.Invoke(IsValidPosition());
            };
            _rotateRight.started += context => {
                if (_ship != null)
                    _ship.shape.RotateClockwise();
                onValidate.Invoke(IsValidPosition());
            };
        }

        private void InitializeGrid()
        {
            _grid = new GridCell[_gridSize, _gridSize];

            for (int i = 0; i < _gridSize; i++)
            {
                for (int j = 0; j < _gridSize; j++)
                {
                    _grid[i, j] = new GridCell(new Vector2Int(i, j));
                }
            }
        }

        private void FixedUpdate()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, interactionLayers))
            {
                onHit.Invoke(true);

                Vector3 gridPosition = hit.transform.position;
                float gridScale = hit.transform.localScale.x;
                float halfSize = _gridSize / 2 * gridScale;

                float minX = gridPosition.x - halfSize;
                float minY = gridPosition.z - halfSize;

                float maxX = gridPosition.x + halfSize;
                float maxY = gridPosition.z + halfSize;

                float xPercentage = (hit.point.x - minX) / (maxX - minX) * 100;
                float yPercentage = (hit.point.z - minY) / (maxY - minY) * 100;

                int indexX = Mathf.FloorToInt(xPercentage / _gridSize);
                int indexY = Mathf.FloorToInt(yPercentage / _gridSize);
                if (_current != _grid[indexX, indexY])
                {
                    _current = _grid[indexX, indexY];

                    onValidate.Invoke(IsValidPosition());
                    onMove.Invoke(new Vector2(indexX, indexY) * gridScale);
                }
            }
            else
            {
                onHit.Invoke(false);
            }
        }

        private bool IsValidPosition()
        {
            if (_ship == null)
                return false;
            bool[] check = new bool[_ship.shape.offsets.Length];
            for (int i = 0; i < _ship.shape.offsets.Length; i++)
            {
                int y = _current.position.y + _ship.shape.offsets[i].y;

                int x = _current.position.x + _ship.shape.offsets[i].x;

                GridCell offsetCell = _current;
                bool isOnGrid;
                if (isOnGrid = x >= 0 && x < _gridSize && y >= 0 && y < _gridSize)
                    offsetCell = _grid[x, y];

                check[i] = isOnGrid && !offsetCell.isTaken;
            }

            return check.All(b => b);
        }

        public void Place()
        {
            for (int i = 0; i < _ship.shape.offsets.Length; i++)
            {
                int y = _current.position.y + _ship.shape.offsets[i].y;

                int x = _current.position.x + _ship.shape.offsets[i].x;

                _grid[x, y].isTaken = true;
            }

            _ship.position = _current.position;
            _ship.onClear.AddListener(Clear);
        }

        public void Clear(ShipBehaviour _requestedShip)
        {
            for (int i = 0; i < _requestedShip.shape.offsets.Length; i++)
            {
                int y = _requestedShip.position.y + _requestedShip.shape.offsets[i].y;

                int x = _requestedShip.position.x + _requestedShip.shape.offsets[i].x;

                _grid[x, y].isTaken = false;
            }

            _requestedShip.onClear.RemoveListener(Clear);
        }
    }
}