using FX;
using PlayerGrid;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Ships
{
    public class ShipBehaviour : MonoBehaviour
    {
        public GridShape shape;

        public UnityEvent onPlace;
        public UnityEvent onClick;
        public UnityEvent onStartMove;
        public bool dismantleMines;

        public Vector2Int position;

        private InputAction _rotateLeft, _rotateRight;

        private System.Action<InputAction.CallbackContext> _rotateLeftAction;
        private System.Action<InputAction.CallbackContext> _rotateRightAction;

        private bool[] _hits;

        [SerializeField] private FXSystem[] effects;
        [SerializeField] private PlacingIndicator indicator;
        [SerializeField] private Animator animator;

        public UnityEvent<ShipBehaviour> OnClear { get; private set; } = new UnityEvent<ShipBehaviour>();
        public UnityEvent OnSink { get; private set; } = new UnityEvent();

        private void Start()
        {
            InitializeEvents();

            _hits = new bool[shape.offsets.Length];
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
            Select();
        }

        private void SetEnabled(bool enabled)
        {
            indicator.gameObject.SetActive(enabled);
        }

        private void MoveTo(Vector3 position)
        {
            transform.position = new Vector3(position.x, 0, position.z);
        }

        private void Rotate(Vector3 rotation)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + rotation);
        }

        private void Validate(bool _isOnGrid)
        {
            if (_isOnGrid)
            {
                indicator.SetActive();

                onClick.RemoveListener(Placed);
                onClick.AddListener(Placed);
            }
            else
            {
                indicator.SetInactive();

                onClick.RemoveListener(Placed);
            }
        }

        private void OnMouseDown()
        {
            onClick.Invoke();
        }

        private void Selectable()
        {
            GridHandler.instance.OnHover.RemoveListener(SetEnabled);
            GridHandler.instance.OnMove.RemoveListener(MoveTo);
            GridHandler.instance.OnValidate.RemoveListener(Validate);

            onClick.RemoveListener(Selectable);
            onClick.AddListener(TryMove);

            _rotateLeft.started -= _rotateLeftAction;
            _rotateRight.started -= _rotateRightAction;
        }
        public void Select()
        {
            onClick.Invoke();
        }

        private void TryMove()
        {
            GridHandler.instance.Ship = this;
        }
        public void Moveable()
        {
            onStartMove.Invoke();

            OnClear.Invoke(this);

            GridHandler.instance.OnHover.AddListener(SetEnabled);
            GridHandler.instance.OnMove.AddListener(MoveTo);
            GridHandler.instance.OnValidate.AddListener(Validate);

            _rotateLeft.started += _rotateLeftAction;
            _rotateRight.started += _rotateRightAction;

            onClick.RemoveListener(TryMove);
        }

        private void Placed()
        {
            onPlace.Invoke();
                
            GridHandler.instance.Ship = null;

            GridHandler.instance.OnHover.RemoveListener(SetEnabled);
            GridHandler.instance.OnMove.RemoveListener(MoveTo);
            GridHandler.instance.OnValidate.RemoveListener(Validate);

            onClick.RemoveListener(Placed);
            onClick.AddListener(TryMove);

            _rotateLeft.started -= _rotateLeftAction;
            _rotateRight.started -= _rotateRightAction;

            SetEnabled(false);
        }

        public void Hit(Vector2Int _attackPosition)
        {
            _hits[shape[_attackPosition - position]] = true;
            FindEffectOnOffset(_attackPosition - position).Play();
            CheckForSink();
        }

        private void CheckForSink()
        {
            if (_hits.All(b => b))
            {
                animator.GetBehaviour<ShipSink>().OnStartSink.AddListener(() => StopEffects());
                animator.SetTrigger("Sink");
                animator.GetBehaviour<ShipSink>().OnEndSink.AddListener(() => Destroy(gameObject));
                OnSink.Invoke();
            }
        }

        public bool IsHitAtPoint(Vector2Int _attackPosition)
        {
            return _hits[shape[_attackPosition - position]];
        }

        public void Lock()
        {
            onClick.RemoveAllListeners();
        }
        
        public FXSystem FindEffectOnOffset(Vector2Int _offset)
        {
            return effects[shape[_offset]];
        }

        private void StopEffects()
        {
            foreach (FXSystem effect in effects)
            {
                effect.Stop();
            }
        }
    }
}