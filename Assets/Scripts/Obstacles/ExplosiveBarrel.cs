using RunGame.Player;
using UnityEngine;

namespace RunGame.Obstacles
{
    public sealed class ExplosiveBarrel : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float lethalImpactSpeed = 5.5f;
        private bool exploded;

        private void OnCollisionEnter(Collision collision)
        {
            if (exploded || !collision.collider.CompareTag("Player")) return;
            if (collision.relativeVelocity.magnitude < lethalImpactSpeed) return;
            exploded = true;
            collision.collider.GetComponent<PlayerHealth>()?.Kill();
            CreateExplosionEffect(collision.GetContact(0).point);
            gameObject.SetActive(false);
            Destroy(gameObject, 2f);
        }

        private static void CreateExplosionEffect(Vector3 position)
        {
            GameObject effect = new("Barrel Explosion");
            effect.transform.position = position;
            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.duration = 0.6f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 11f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.35f, 1.1f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.18f, 0.01f), new Color(1f, 0.8f, 0.08f));
            main.loop = false;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 42) });
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.4f;
            particles.Play();
            Destroy(effect, 2f);
        }
    }
}
