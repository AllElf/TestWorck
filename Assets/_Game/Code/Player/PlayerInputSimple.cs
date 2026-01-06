using UnityEngine;

namespace Game.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerInputSimple : MonoBehaviour
    {
        [SerializeField] private RigidbodyTopDownMotor motor;
        [SerializeField] private Animator animatorPlayer;

        [Header("Camera Relative")]
        [Tooltip("Если не задано — будет искать Camera.main автоматически (каждый кадр при необходимости).")]
        [SerializeField] private Transform cameraTransform;

        private float x,y;
        bool isRun = false;

        private void Reset()
        {
            motor = GetComponent<RigidbodyTopDownMotor>();
        }

        private void Update()
        {
            if (motor == null) return;

            // Если камера не задана или была уничтожена/выключена — переищем
            if (cameraTransform == null)
            {
                var cam = Camera.main;
                if (cam != null) cameraTransform = cam.transform;
            }

             x = Input.GetAxisRaw("Horizontal"); // A/D
             y = Input.GetAxisRaw("Vertical");   // W/S

            Vector3 moveWorld;

            if (cameraTransform != null)
            {
                // forward/right камеры в плоскости XZ
                Vector3 camForward = cameraTransform.forward;
                camForward.y = 0f;
                camForward.Normalize();

                Vector3 camRight = cameraTransform.right;
                camRight.y = 0f;
                camRight.Normalize();

                moveWorld = camRight * x + camForward * y;
            }
            else
            {
                // fallback: мировые оси
                moveWorld = new Vector3(x, 0f, y);
            }

            motor.SetMoveWorld(moveWorld);
            AnimatorMove();
        }

        public void AnimatorMove()
        {
            if (animatorPlayer == null) return;

            AnimatorStateInfo stateInfo = animatorPlayer.GetCurrentAnimatorStateInfo(0);

            if ((x != 0 || y != 0) && !stateInfo.IsName("Running"))
            {
                animatorPlayer.Play("Running");
                isRun = true;
            }
            else if ((x == 0 && y == 0) && !stateInfo.IsName("Idle"))
            {
                animatorPlayer.Play("Idle");
                isRun = false;
            }
        }
    }
}
