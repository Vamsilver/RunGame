using UnityEngine;

namespace RunGame.Bonus
{
    [RequireComponent(typeof(Collider))]
    public sealed class BonusTriggerAnimator : MonoBehaviour
    {
        private static readonly int PlayerNearby = Animator.StringToHash("PlayerNearby");

        [SerializeField] private Animator bonusAnimator;

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
            bonusAnimator = GetComponentInParent<Animator>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) bonusAnimator.SetBool(PlayerNearby, true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) bonusAnimator.SetBool(PlayerNearby, false);
        }
    }
}
