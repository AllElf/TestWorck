using UnityEngine;

public class CameraTracking : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform _camera;   // Камера или CameraRig
    [SerializeField] private Transform _target;   // Игрок (Transform)

    [Header("Dead Zone")]
    [SerializeField, Min(0f)] private float chaseStartRadius = 4f; // когда начинаем следовать
    [SerializeField, Min(0f)] private float chaseStopRadius = 2f; // когда прекращаем (должно быть меньше start)

    [Header("Follow")]
    [SerializeField, Min(0f)] private float followSpeed = 8f;      // скорость камеры (ед/с)
    [SerializeField] private bool lockY = true;

    [Header("Stability")]
    [Tooltip("Минимальная дистанция, чтобы не дергать позицию на микрошаги.")]
    [SerializeField, Min(0f)] private float deadEpsilon = 0.02f;

    private Vector3 _startOffset;
    private bool _isChasing;

    private void Reset()
    {
        if (_camera == null && Camera.main != null)
            _camera = Camera.main.transform;
    }

    private void Start()
    {
        if (_camera == null && Camera.main != null)
            _camera = Camera.main.transform;

        if (_camera == null || _target == null)
        {
            Debug.LogError("CameraTracking: назначьте _camera и _target.");
            enabled = false;
            return;
        }

        // Важно: если игрок двигается Rigidbody, включите у него Interpolation=Interpolate,
        // тогда _target.position будет уже сглажен под кадр.
        _startOffset = _camera.position - _target.position;

        // гарантия гистерезиса
        if (chaseStopRadius >= chaseStartRadius)
            chaseStopRadius = chaseStartRadius * 0.5f;
    }

    private void FixedUpdate()
    {
        Vector3 desired = _target.position + _startOffset;

        Vector3 camPos = _camera.position;
        Vector2 camXZ = new Vector2(camPos.x, camPos.z);
        Vector2 desXZ = new Vector2(desired.x, desired.z);

        float dist = Vector2.Distance(camXZ, desXZ);

        // Включаем/выключаем режим преследования с гистерезисом
        if (!_isChasing)
        {
            if (dist > chaseStartRadius) _isChasing = true;
            else return;
        }
        else
        {
            if (dist < chaseStopRadius) { _isChasing = false; return; }
        }

        // Двигаем только по XZ
        Vector3 targetCamPos = desired;
        if (lockY) targetCamPos.y = camPos.y;

        // Если уже почти на месте — не делаем микродвижений (убирает дрожь)
        Vector3 delta = targetCamPos - camPos;
        delta.y = 0f;
        if (delta.sqrMagnitude <= deadEpsilon * deadEpsilon)
            return;

        // Предсказуемое движение без подпружинивания
        float step = followSpeed * Time.deltaTime;
        _camera.position = Vector3.MoveTowards(camPos, targetCamPos, step);
    }
}
