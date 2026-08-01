using UnityEngine;
using RunGame.Procedural;

namespace RunGame.Obstacles
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RotatingObstacle : MonoBehaviour, IDifficultyScalable
    {
        [SerializeField] private Vector3 axis = Vector3.up;
        [SerializeField] private float degreesPerSecond = 105f;
        private Rigidbody body;
        private float initialSpeed;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            initialSpeed = degreesPerSecond;
        }

        public void SetDifficulty(float multiplier) => degreesPerSecond = initialSpeed * Mathf.Sqrt(Mathf.Max(1f, multiplier));

        private void FixedUpdate()
        {
            Quaternion step = Quaternion.AngleAxis(degreesPerSecond * Time.fixedDeltaTime, axis.normalized);
            body.MoveRotation(step * body.rotation);
        }
    }
}
