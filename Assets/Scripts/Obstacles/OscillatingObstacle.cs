using UnityEngine;

namespace RunGame.Obstacles
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class OscillatingObstacle : MonoBehaviour
    {
        [SerializeField] private Vector3 localOffset = new(7f, 0f, 0f);
        [SerializeField, Min(0.1f)] private float cycleDuration = 2.5f;
        private Rigidbody body;
        private Vector3 startPosition;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            startPosition = transform.position;
        }

        private void FixedUpdate()
        {
            float progress = Mathf.PingPong(Time.time / cycleDuration, 1f);
            float eased = Mathf.SmoothStep(0f, 1f, progress);
            body.MovePosition(startPosition + transform.TransformDirection(localOffset) * eased);
        }
    }
}
