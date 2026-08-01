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
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.28f, 0.72f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3.5f, 9.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.45f, 1.45f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.9f, 0.18f), new Color(1f, 0.28f, 0.015f));
            main.gravityModifier = -0.08f;
            main.loop = false;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 58) });
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.32f;
            ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
            color.enabled = true;
            Gradient fireGradient = new();
            fireGradient.SetKeys(
                new[] { new GradientColorKey(new Color(1f, 1f, 0.45f), 0f), new GradientColorKey(new Color(1f, 0.2f, 0.01f), 0.58f), new GradientColorKey(new Color(0.18f, 0.025f, 0.01f), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.9f, 0.45f), new GradientAlphaKey(0f, 1f) });
            color.color = fireGradient;
            ParticleSystem.SizeOverLifetimeModule size = particles.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.25f, 1f, 1.35f));
            Material fireMaterial = Resources.Load<Material>("Effects/FireParticleMaterial");
            if (fireMaterial != null) effect.GetComponent<ParticleSystemRenderer>().sharedMaterial = fireMaterial;
            Light flash = effect.AddComponent<Light>();
            flash.type = LightType.Point;
            flash.color = new Color(1f, 0.32f, 0.025f);
            flash.intensity = 7f;
            flash.range = 7f;
            particles.Play();
            Destroy(effect, 1.3f);
        }
    }
}
