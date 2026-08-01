using UnityEngine;

namespace RunGame.Bonus
{
    [RequireComponent(typeof(Collider))]
    public sealed class BonusTriggerAnimator : MonoBehaviour
    {
        private static readonly int PlayerNearby = Animator.StringToHash("PlayerNearby");

        [SerializeField] private Animator bonusAnimator;
        [SerializeField] private ParticleSystem activationParticles;
        [SerializeField] private Light activationLight;
        [SerializeField, Min(1)] private int healAmount = 25;
        private bool consumed;

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
            bonusAnimator = GetComponentInParent<Animator>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            bonusAnimator.SetBool(PlayerNearby, true);
            if (activationParticles != null) activationParticles.Play();
            if (activationLight != null) activationLight.enabled = true;
            if (!consumed)
            {
                consumed = true;
                other.GetComponent<RunGame.Player.PlayerHealth>()?.Heal(healAmount);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            bonusAnimator.SetBool(PlayerNearby, false);
            if (activationParticles != null)
                activationParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            if (activationLight != null) activationLight.enabled = false;
        }
    }
}
