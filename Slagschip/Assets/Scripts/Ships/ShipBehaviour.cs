using PlayerGrid;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Ships
{
    public class ShipBehaviour : MonoBehaviour
    {
        public GridShape shape;

        public UnityEvent onPlace;

        private MeshRenderer _renderer;
        private Material _material;

        private InputAction _rotateLeft, _rotateRight;

        private System.Action<InputAction.CallbackContext> _rotateLeftAction;
        private System.Action<InputAction.CallbackContext> _rotateRightAction;

        [SerializeField] private UnityEvent onStartMove;
        [SerializeField] private UnityEvent onClick;

        private void Start()
        {
            _renderer = transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
            _material = _renderer.material;
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            _rotateLeft = InputSystem.actions.FindAction("RotateLeft");
            _rotateRight = InputSystem.actions.FindAction("RotateRight");

            _rotateLeftAction += context => {
                Rotate(new Vector3(0, -90, 0));
            };
            _rotateRightAction += context => {
                Rotate(new Vector3(0, 90, 0));
            };

            Selectable();
        }

        private void SetEnabled(bool enabled)
        {
            _renderer.enabled = enabled;
        }

        private void MoveTo(Vector2 position)
        {
            transform.position = new Vector3(position.x, 0, position.y);
        }

        private void Rotate(Vector3 rotation)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + rotation);
        }
        private void ResetRotation()
        {
            transform.rotation = Quaternion.Euler(Vector3.zero);
        }

        private void Validate(bool _isOnGrid)
        {
            if (_isOnGrid)
            {
                _material.color = Color.green;

                onClick.RemoveListener(Placed);
                onClick.AddListener(Placed);
            }
            else
            {
                _material.color = Color.red;
                onClick.RemoveListener(Placed);
            }
        }

        private void OnMouseDown()
        {
            onClick.Invoke();
        }

        private void Selectable()
        {
            GridHandler.instance.onHit.RemoveListener(SetEnabled);
            GridHandler.instance.onMove.RemoveListener(MoveTo);
            GridHandler.instance.onValidate.RemoveListener(Validate);

            onStartMove.AddListener(ResetRotation);

            onClick.RemoveListener(Selectable);
            onClick.AddListener(Moveable);

            _rotateLeft.started -= _rotateLeftAction;
            _rotateRight.started -= _rotateRightAction;
        }

        private void Moveable()
        {
            GridHandler.instance.Ship = this;
            onStartMove.Invoke();
            onStartMove.RemoveListener(ResetRotation);

            GridHandler.instance.onHit.AddListener(SetEnabled);
            GridHandler.instance.onMove.AddListener(MoveTo);
            GridHandler.instance.onValidate.AddListener(Validate);

            _rotateLeft.started += _rotateLeftAction;
            _rotateRight.started += _rotateRightAction;

            onClick.RemoveListener(Moveable);
        }

        private void Placed()
        {
            onPlace.Invoke();

            GridHandler.instance.onHit.RemoveListener(SetEnabled);
            GridHandler.instance.onMove.RemoveListener(MoveTo);
            GridHandler.instance.onValidate.RemoveListener(Validate);

            onClick.RemoveListener(Placed);
            onClick.AddListener(Moveable);

            _rotateLeft.started -= _rotateLeftAction;
            _rotateRight.started -= _rotateRightAction;

            _material.color =new Color(0.5f, 0.5f, 0.5f);  
        }    
    }
}