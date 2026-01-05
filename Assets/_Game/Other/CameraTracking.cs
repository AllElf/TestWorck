using UnityEngine;

public class CameraTracking : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform _camera;       // Камера или CameraRig
    [SerializeField] private Rigidbody _targetRb;     // Rigidbody игрока (важно!)
    [SerializeField] private Transform _target;       // Если Rigidbody не задан, будет fallback

    [Header("Dead Zone & Follow")]
    [SerializeField, Min(0f)] private float startChaseRadius = 4f; // старт погони (вышли за радиус)
    [SerializeField, Min(0f)] private float stopChaseRadius = 1f;  // стоп погони (достаточно близко)
    [SerializeField, Min(0.01f)] private float smoothTime = 0.18f;
    [SerializeField] private float maxSpeed = 80f;
    [SerializeField] private bool lockY = true;

    private Vector3 _startOffset;
    private Vector3 _velocity;
    private bool _isChasing;

    // Для интерполяции цели
    private Vector3 _rbPrevPos;
    private Vector3 _rbCurrPos;

    private void Reset()
    {
        if (_camera == null && Camera.main != null)
            _camera = Camera.main.transform;
    }

    private void Start()
    {
        if (_camera == null && Camera.main != null)
            _camera = Camera.main.transform;

        if (_camera == null)
        {
            Debug.LogError("CameraTracking: назначьте _camera.");
            enabled = false;
            return;
        }

        // Если не задан Rigidbody, попробуем взять из _target
        if (_targetRb == null && _target != null)
            _targetRb = _target.GetComponent<Rigidbody>();

        if (_targetRb == null && _target == null)
        {
            Debug.LogError("CameraTracking: назначьте _targetRb или _target.");
            enabled = false;
            return;
        }

        Vector3 targetPos = GetTargetPosition();
        _startOffset = _camera.position - targetPos;

        _rbPrevPos = targetPos;
        _rbCurrPos = targetPos;

        if (stopChaseRadius > startChaseRadius)
            stopChaseRadius = startChaseRadius * 0.5f;
    }

    private void FixedUpdate()
    {
        // обновляем позиции цели на физическом тике
        Vector3 pos = GetTargetPosition();
        _rbPrevPos = _rbCurrPos;
        _rbCurrPos = pos;
    }

    private void LateUpdate()
    {
        Vector3 targetPos = GetInterpolatedTargetPosition();
        Vector3 desired = targetPos + _startOffset;

        // расстояние по XZ
        Vector2 camXZ = new Vector2(_camera.position.x, _camera.position.z);
        Vector2 desXZ = new Vector2(desired.x, desired.z);
        float dist = Vector2.Distance(camXZ, desXZ);

        // старт погони
        if (!_isChasing)
        {
            if (dist > startChaseRadius)
            {
                _isChasing = true;
                _velocity = Vector3.zero;
            }
            else
            {
                return;
            }
        }

        // движение к цели
        Vector3 targetCamPos = desired;
        if (lockY)
            targetCamPos.y = _camera.position.y;

        _camera.position = Vector3.SmoothDamp(
            _camera.position,
            targetCamPos,
            ref _velocity,
            smoothTime,
            maxSpeed,
            Time.deltaTime
        );

        // стоп погони (гистерезис)
        Vector2 newXZ = new Vector2(_camera.position.x, _camera.position.z);
        float newDist = Vector2.Distance(newXZ, desXZ);

        if (newDist <= stopChaseRadius)
        {
            // “снап” в точку, чтобы убрать микро-дрожание
            _camera.position = new Vector3(targetCamPos.x, _camera.position.y, targetCamPos.z);
            _velocity = Vector3.zero;
            _isChasing = false;
        }
    }

    private Vector3 GetTargetPosition()
    {
        if (_targetRb != null) return _targetRb.position;
        return _target != null ? _target.position : Vector3.zero;
    }

    private Vector3 GetInterpolatedTargetPosition()
    {
        // alpha между FixedUpdate тиками
        float alpha = 0f;
        if (Time.fixedDeltaTime > 0f)
        {
            alpha = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
            alpha = Mathf.Clamp01(alpha);
        }

        return Vector3.Lerp(_rbPrevPos, _rbCurrPos, alpha);
    }
}
