using RunGame.Player;
using UnityEngine;

namespace RunGame.Obstacles
{
    public sealed class DamageObstacle : MonoBehaviour
    {
        [SerializeField, Min(1)] private int damage = 25;

        private void OnCollisionEnter(Collision collision) => Damage(collision.collider);
        private void OnCollisionStay(Collision collision) => Damage(collision.collider);

        private void Damage(Collider other)
        {
            if (other.CompareTag("Player")) other.GetComponent<PlayerHealth>()?.TakeDamage(damage);
        }
    }
}
