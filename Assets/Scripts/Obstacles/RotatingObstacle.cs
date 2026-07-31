using UnityEngine;

namespace RunGame.Obstacles
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RotatingObstacle : MonoBehaviour
    {
        [SerializeField] private Vector3 axis = Vector3.up;
        [SerializeField] private float degreesPerSecond = 105f;
        private Rigidbody body;

        private void Awake() => body = GetComponent<Rigidbody>();

        private void FixedUpdate()
        {
            Quaternion step = Quaternion.AngleAxis(degreesPerSecond * Time.fixedDeltaTime, axis.normalized);
            body.MoveRotation(step * body.rotation);
        }
    }
}
