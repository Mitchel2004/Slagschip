using UnityEngine;

namespace Ships
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class PlacingIndicator : MonoBehaviour
    {
        private Mesh _mesh;
        private MeshFilter _filter;
        private MeshRenderer _renderer;

        private static int[] _tris = new int[] {
        4, 0, 1,
        5, 4, 1,
        5, 1, 2,
        6, 5, 2,
        6, 2, 3,
        7, 6, 3,
        7, 3, 0,
        4, 7, 0,

        12, 9, 8,
        13, 9, 12,
        13, 10, 9,
        14, 10, 13,
        14, 11, 10,
        15, 11, 14,
        15, 8, 11,
        12, 8, 15,

        0, 8, 9,
        0, 9, 1,

        1, 9, 10,
        1, 10, 2,

        2, 10, 11,
        2, 11, 3,

        3, 11, 8,
        3, 8, 0,

        4, 13, 12,
        4, 5, 13,

        5, 14, 13,
        5, 6, 14,

        6, 15, 14,
        6, 7, 15,

        7, 12, 15,
        7, 4, 12
    };

        [SerializeField] private float thickness = 1;
        [SerializeField] private float width = 1;
        [SerializeField] private float depth = 1;

        [SerializeField] private Material active, inactive;

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            GenerateMesh();
        }

        public void Initialize()
        {
            _filter = GetComponent<MeshFilter>();
            _renderer = GetComponent<MeshRenderer>();
        }

        public void GenerateMesh()
        {
            _mesh = new Mesh();

            _mesh.vertices = new Vector3[] {
            new Vector3(-depth / 2, thickness / 2, width / 2),
            new Vector3(depth / 2, thickness / 2, width / 2),
            new Vector3(depth / 2, thickness / 2, -width / 2),
            new Vector3(-depth / 2, thickness / 2, -width / 2),
            new Vector3(-depth / 2 - thickness, thickness / 2, width / 2 + thickness),
            new Vector3(depth / 2 + thickness, thickness / 2, width / 2 + thickness),
            new Vector3(depth / 2 + thickness, thickness / 2, -width / 2 - thickness),
            new Vector3(-depth / 2 - thickness, thickness / 2, -width / 2 - thickness),

            new Vector3(-depth / 2, -thickness / 2, width / 2),
            new Vector3(depth / 2, -thickness / 2, width / 2),
            new Vector3(depth / 2, -thickness / 2, -width / 2),
            new Vector3(-depth / 2, -thickness / 2, -width / 2),
            new Vector3(-depth / 2 - thickness, -thickness / 2, width / 2 + thickness),
            new Vector3(depth / 2 + thickness, -thickness / 2, width / 2 + thickness),
            new Vector3(depth / 2 + thickness, -thickness / 2, -width / 2 - thickness),
            new Vector3(-depth / 2 - thickness, -thickness / 2, -width / 2 - thickness)
        };

            _mesh.triangles = _tris;

            _filter.sharedMesh = _mesh;
        }

        public void SetActive()
        {
            _renderer.material = active;
        }
        public void SetInactive()
        {
            _renderer.material = inactive;
        }
    }
}
