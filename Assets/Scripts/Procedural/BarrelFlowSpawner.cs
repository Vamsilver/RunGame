using System.Collections;
using RunGame.Obstacles;
using UnityEngine;

namespace RunGame.Procedural
{
    public sealed class BarrelFlowSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject rollingBarrelPrefab;
        [SerializeField] private Transform leftSpawn;
        [SerializeField] private Transform rightSpawn;
        [SerializeField, Min(0.5f)] private float baseInterval = 2.4f;
        [SerializeField, Min(1f)] private float barrelSpeed = 7f;
        [SerializeField, Min(1f)] private float barrelLifetime = 8f;

        private float difficulty = 1f;
        private int side;

        public void SetDifficulty(float value) => difficulty = Mathf.Max(1f, value);

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(0.8f);
            while (enabled)
            {
                Spawn(side++ % 2 == 0 ? leftSpawn : rightSpawn);
                float interval = baseInterval / Mathf.Lerp(1f, difficulty, 0.65f);
                yield return new WaitForSeconds(Mathf.Max(0.65f, interval));
            }
        }

        private void Spawn(Transform point)
        {
            if (rollingBarrelPrefab == null || point == null) return;
            // Keep the cylinder axle along Z even if a future prefab rebuild
            // changes the orientation saved on the asset root.
            Quaternion spawnRotation = point.rotation * Quaternion.Euler(90f, 0f, 0f);
            GameObject barrel = Instantiate(rollingBarrelPrefab, point.position, spawnRotation, transform);
            Rigidbody body = barrel.GetComponent<Rigidbody>();
            if (body != null)
            {
                Vector3 towardCenter = new(-Mathf.Sign(point.localPosition.x), 0f, 0f);
                body.linearVelocity = towardCenter * barrelSpeed * Mathf.Sqrt(difficulty);
                body.angularVelocity = Vector3.forward * -towardCenter.x * barrelSpeed;
                body.maxAngularVelocity = 30f;
            }
            Destroy(barrel, barrelLifetime);
        }
    }
}
