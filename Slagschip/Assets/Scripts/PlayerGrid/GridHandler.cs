using Multiplayer;
using UIHandlers;
using Ships;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Utilities;
using Utilities.Generic;
using static UnityEditor.PlayerSettings;

namespace PlayerGrid
{
    public class GridHandler : NetworkBehaviour
    {
        public static GridHandler instance;

        [SerializeField] private DashboardHandler _dashboardHandler;
        [SerializeField] private GameData gameData;
        private float _gridScale => transform.localScale.x;

        public const byte gridSize = 10;
        private const byte _maxShips = 1;
        private const byte _maxMines = 2;

        private int _mineCount;

        private GridCell[,] _grid;
        private GridCell _current;

        private InputAction _rotateLeft, _rotateRight;

        private ShipBehaviour _ship;
        private UniqueList<ShipBehaviour> _ships = new UniqueList<ShipBehaviour>();

        private bool _placing = false;

        [SerializeField] private LayerMask interactionLayers;
        
        public UnityEvent<bool> OnValidate { get; private set; } = new UnityEvent<bool>();
        public UnityEvent<Vector3> OnMove { get; private set; } = new UnityEvent<Vector3>();
        public UnityEvent<bool> OnHover { get; private set; } = new UnityEvent<bool>();
        public UnityEvent<bool> OnIsReady { get; private set; } = new UnityEvent<bool>();

        public UnityEvent<GridCell> OnAttacked { get; private set; } = new UnityEvent<GridCell>();
        public UnityEvent<GridCell> OnMineSet { get; private set; } = new UnityEvent<GridCell>();

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
        }

        private void Start()
        {
            InitializeInput();
        }

        private void InitializeInput()
        {
            InputActionAsset actions = FindFirstObjectByType<PlayerInput>().actions;
            _rotateLeft = actions.FindAction("RotateLeft");
            _rotateRight = actions.FindAction("RotateRight");

            if (IsClient)
            {
                _rotateLeft.started += context => {
                    if (_ship != null)
                        _ship.shape.RotateCounterClockwise();
                    OnValidate.Invoke(IsValidPosition());
                };
                _rotateRight.started += context => {
                    if (_ship != null)
                        _ship.shape.RotateClockwise();
                    OnValidate.Invoke(IsValidPosition());
                };
            }
        }

        private void InitializeGrid()
        {
            _grid = new GridCell[gridSize, gridSize];

            float startX = -gridSize / 2f + _gridScale / 2f;
            float startZ = -gridSize / 2f + _gridScale / 2f;

            for (int j = 0; j < gridSize; j++)
            {
                for (int i = 0; i < gridSize; i++)
                {
                    float localX = startX + i * _gridScale;
                    float localZ = startZ + j * _gridScale;

                    Vector3 localPos = new Vector3(localX, 0, localZ);
                    Vector3 worldPos = transform.TransformPoint(localPos);

                    _grid[i, j] = new GridCell(new Vector2Int(i, j), worldPos);
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
                OnHover.Invoke(true);

                Vector3 gridPosition = hit.transform.position;
                float halfSize = gridSize / 2 * _gridScale;

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

                    OnMove.Invoke(_grid[indexX, indexY].worldPosition);
                }
                OnValidate.Invoke(IsValidPosition());
            }
            else
            {
                OnHover.Invoke(false);
            }
        }

        private void CheckReadiness()
        {
            if (_ships.Count == _maxShips && !_placing && _mineCount == _maxMines)
            {
                OnIsReady.Invoke(true);
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

                int x = _current.position.x - _ship.shape.offsets[i].x;

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

                int x = _current.position.x - _ship.shape.offsets[i].x;

                _grid[x, y].isTaken = true;
            }

            _ships.Add(_ship);
            _ship.position = _current.position;
            _ship.OnClear.AddListener(Clear);

            _placing = false;

            _ship = null;

            CheckReadiness();
        }

        public void Clear(ShipBehaviour _requestedShip)
        {
            for (int i = 0; i < _requestedShip.shape.offsets.Length; i++)
            {
                int y = _requestedShip.position.y + _requestedShip.shape.offsets[i].y;

                int x = _requestedShip.position.x - _requestedShip.shape.offsets[i].x;

                _grid[x, y].isTaken = false;
            }
            _requestedShip.OnClear.RemoveListener(Clear);

            _placing = true;
            OnIsReady.Invoke(false);
        }

        // TODO: Check incoming target cell whether it is a hit or miss
        [Rpc(SendTo.NotMe)]
        public void CheckTargetCellRpc(byte _targetCell)
        {
            if (IsClient)
            {
                bool isHit = false;
                if (_targetCell <= 100)
                {
                    Vector2Int pos = CellUnpacker.CellPosition(_targetCell);
                    isHit = Hit(pos);
                    OnAttacked.Invoke(_grid[pos.x, pos.y]);
                }

                //TODO: Torpedo hit check with correct target cell

                if (isHit)
                {
                    _dashboardHandler.OnHitRpc(_targetCell);
                }
                else
                {
                    _dashboardHandler.OnMissRpc(_targetCell);

                    gameData.SwitchPlayerTurnRpc();
                }
            }
        }

        public bool Hit(Vector2Int _position)
        {
            return _grid[_position.x, _position.y].isTaken;
        }

        public void MineCallback(Vector2Int target)
        {
            _dashboardHandler.OnHitRpc(CellUnpacker.PackCell(target));
        }

        [Rpc(SendTo.NotMe)]
        public void PlaceMineRpc(byte _targetCell)
        {
            Vector2Int pos = CellUnpacker.CellPosition(_targetCell);
            OnMineSet.Invoke(_grid[pos.x, pos.y]);
            IncrementMineCountRpc();
        }
        [Rpc(SendTo.NotMe)]
        private void IncrementMineCountRpc()
        {
            _mineCount++;
            CheckReadiness();
        }

        public void LockGrid()
        {
            for (int i = 0; i < _ships.Count; i++)
            {
                _ships[i].Lock();
            }
        }
        public ShipBehaviour ShipFromCell(GridCell _cell)
        {
            for (int i = 0; i < _ships.Count; i++)
            {
                if (_ships[i].shape.ContainsOffset(_cell.position - _ships[i].position))
                    return _ships[i];
            }
            return null;
        }
        public ShipBehaviour ShipFromPosition(Vector2Int _position)
        {
            for (int i = 0; i < _ships.Count; i++)
            {
                if (_ships[i].shape.ContainsOffset(_position - _ships[i].position))
                    return _ships[i];
            }
            return null;
        }
        public Vector3 CellWorldPosition(Vector2Int _cellPosition)
        {
            return _grid[_cellPosition.x, _cellPosition.y].worldPosition;
        }
    }
}