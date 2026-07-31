using RunGame.Player;
using UnityEngine;

namespace RunGame.Effects
{
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class MovementParticleController : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;
        [SerializeField, Min(0f)] private float minimumSpeed = 0.35f;
        [SerializeField, Min(0f)] private float maximumEmissionRate = 42f;
        private ParticleSystem particles;

        private void Awake() => particles = GetComponent<ParticleSystem>();

        private void Update()
        {
            float speed = playerController.HorizontalVelocity.magnitude;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = maximumEmissionRate * Mathf.InverseLerp(minimumSpeed, 7f, speed);

            if (speed > minimumSpeed)
            {
                if (!particles.isPlaying) particles.Play();
            }
            else if (particles.isPlaying)
            {
                particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}
