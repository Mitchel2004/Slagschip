using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Rendering;

namespace PlayerGrid
{
    public class GridHandler : MonoBehaviour
    {
        [SerializeField] private LayerMask interactionLayers;
        [SerializeField] private GridShape shape;

        private const int _gridSize = 10;
        private GridCell[,] _grid;
        private GridCell _current;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            InitializeGrid();
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
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100, interactionLayers))
            {
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

                    IsValidPosition();
                }
            }
        }

        void IsValidPosition()
        {
            bool[] check = new bool[shape.offsets.Length];
            for (int i = 0; i < shape.offsets.Length; i++)
            {
                bool yMin = _current.position.y + shape.offsets[i].y >= 0;
                bool yMax = _current.position.y + shape.offsets[i].y < _gridSize;

                bool xMin = _current.position.x + shape.offsets[i].x >= 0;
                bool xMax = _current.position.x + shape.offsets[i].x < _gridSize;
                check[i] = yMin && yMax && xMin && xMax;
                
            }
            Debug.Log(check.All(b => b));
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                shape.RotateCounterClockwise();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                shape.RotateClockwise();
            }
        }
    }
}