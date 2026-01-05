using UnityEngine;

namespace Game.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RigidbodyTopDownMotor : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private bool rotateToMoveDirection = true;
        [SerializeField] private float rotationLerp = 12f;

        [Header("Ground Plane")]
        [Tooltip("Обычно Y фиксируем. Если true — удерживаем Rigidbody на исходной Y.")]
        [SerializeField] private bool lockYPosition = true;

        private Rigidbody _rb;
        private Vector3 _moveWorld; // уже в мировых координатах (XZ)
        private float _startY;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.constraints = RigidbodyConstraints.FreezeRotation;
            _startY = _rb.position.y;
        }

        /// <summary>
        /// Вектор движения в МИРОВЫХ координатах (обычно XZ), уже относительно камеры.
        /// </summary>
        public void SetMoveWorld(Vector3 moveWorld)
        {
            // ограничим длину, чтобы по диагонали скорость не росла
            _moveWorld = Vector3.ClampMagnitude(moveWorld, 1f);
        }

        private void FixedUpdate()
        {
            Vector3 move = _moveWorld;
            move.y = 0f;

            Vector3 targetPos = _rb.position + move * (moveSpeed * Time.fixedDeltaTime);
            if (lockYPosition) targetPos.y = _startY;

            _rb.MovePosition(targetPos);

            if (rotateToMoveDirection && move.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(move, Vector3.up);
                Quaternion newRot = Quaternion.Slerp(_rb.rotation, targetRot, rotationLerp * Time.fixedDeltaTime);
                _rb.MoveRotation(newRot);
            }
        }
    }
}
