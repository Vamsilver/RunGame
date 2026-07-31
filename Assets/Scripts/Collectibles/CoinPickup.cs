using UnityEngine;

namespace RunGame.Collectibles
{
    [RequireComponent(typeof(Collider))]
    public sealed class CoinPickup : MonoBehaviour
    {
        [SerializeField, Min(1)] private int value = 1;
        [SerializeField, Min(0f)] private float rotationSpeed = 120f;
        private bool collected;

        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void Update() => transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);

        private void OnTriggerEnter(Collider other)
        {
            if (collected || !other.CompareTag("Player")) return;
            Wallet wallet = other.GetComponent<Wallet>();
            if (wallet == null) return;
            collected = true;
            wallet.AddCoins(value);
            Destroy(gameObject);
        }
    }
}
