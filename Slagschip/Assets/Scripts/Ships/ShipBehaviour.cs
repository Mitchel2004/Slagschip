using PlayerGrid;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Ships
{
    public class ShipBehaviour : MonoBehaviour
    {
        public GridShape shape;

        private MeshRenderer _renderer;
        private Material _material;

        private InputAction _rotateLeft, _rotateRight;


        private void Start()
        {
            _renderer = GetComponent<MeshRenderer>();
            _material = _renderer.material;
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            GridHandler.instance.onValidate.AddListener(Validate);

            GridHandler.instance.onMove.AddListener(position => {
                transform.position = new Vector3(position.x, 0, position.y);
            });

            GridHandler.instance.onHit.AddListener(hit => {
                _renderer.enabled = hit;
            });

            _rotateLeft = InputSystem.actions.FindAction("RotateLeft");
            _rotateRight = InputSystem.actions.FindAction("RotateRight");

            _rotateLeft.started += context => {
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, -90, 0));
            };
            _rotateRight.started += context => {

                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, 90, 0));
            };
        }

        private void Validate(bool _isOnGrid)
        {
            if (_isOnGrid)
            {
                _material.color = Color.green;
            }
            else
            {
                _material.color = Color.red;
            }
        }

    }
}