using OpponentGrid;
using Ships;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace PlayerGrid
{
    public class GridHandler : NetworkBehaviour
    {
        public static GridHandler instance;

        [SerializeField] private OpponentGridHandler _opponentGridHandler;

        public UnityEvent<bool> onValidate;
        public UnityEvent<Vector2> onMove;
        public UnityEvent<bool> onHit;

        public const byte gridSize = 10;

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
                if (_ship != null)
                    return;

                _ship = value;

                if (value != null)
                    _ship.Moveable();
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
            _grid = new GridCell[gridSize, gridSize];

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    _grid[i, j] = new GridCell(new Vector2Int(i, j));
                }
            }
        }
        private void FixedUpdate()
        {
            MoveOnGrid();
        }

        private void MoveOnGrid()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, interactionLayers))
            {
                onHit.Invoke(true);

                Vector3 gridPosition = hit.transform.position;
                float gridScale = hit.transform.localScale.x;
                float halfSize = gridSize / 2 * gridScale;

                float minX = gridPosition.x - halfSize;
                float minY = gridPosition.z - halfSize;

                float maxX = gridPosition.x + halfSize;
                float maxY = gridPosition.z + halfSize;

                float xPercentage = (hit.point.x - minX) / (maxX - minX) * 100;
                float yPercentage = (hit.point.z - minY) / (maxY - minY) * 100;

                int indexX = Mathf.FloorToInt(xPercentage / gridSize);
                int indexY = Mathf.FloorToInt(yPercentage / gridSize);
                if (_current != _grid[indexX, indexY])
                {
                    _current = _grid[indexX, indexY];

                    onMove.Invoke(new Vector2(indexX, indexY) * gridScale);
                }
                onValidate.Invoke(IsValidPosition());
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
                if (isOnGrid = x >= 0 && x < gridSize && y >= 0 && y < gridSize)
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
            _ship.OnClear.AddListener(Clear);
            _ship = null;
        }

        public void Clear(ShipBehaviour _requestedShip)
        {
            for (int i = 0; i < _requestedShip.shape.offsets.Length; i++)
            {
                int y = _requestedShip.position.y + _requestedShip.shape.offsets[i].y;

                int x = _requestedShip.position.x + _requestedShip.shape.offsets[i].x;

                _grid[x, y].isTaken = false;
            }

            _requestedShip.OnClear.RemoveListener(Clear);
        }

        // TODO: Check incoming target cell whether it is a hit or miss
        [Rpc(SendTo.NotMe)]
        public void CheckTargetCellRpc(byte _targetCell)
        {
            if (IsClient)
                Debug.Log(_targetCell);

                // if hit:
                _opponentGridHandler.OnHitRpc(_targetCell);

                // if miss:
                _opponentGridHandler.OnMissRpc(_targetCell);
        }
    }
}