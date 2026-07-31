using RunGame.Player;
using UnityEngine;

namespace RunGame.Effects
{
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class MovementParticleController : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;
        private ParticleSystem particles;

        private void Awake() => particles = GetComponent<ParticleSystem>();

        private void Update()
        {
            if (playerController.IsMoving)
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
