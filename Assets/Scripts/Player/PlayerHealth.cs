using System;
using System.Collections;
using UnityEngine;

namespace RunGame.Player
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maxHealth = 100;
        [SerializeField, Min(0f)] private float damageCooldown = 0.65f;

        private bool canTakeDamage = true;
        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public event Action<int, int> HealthChanged;

        private void Awake() => CurrentHealth = maxHealth;

        private void Start() => HealthChanged?.Invoke(CurrentHealth, maxHealth);

        public void TakeDamage(int damage)
        {
            if (!canTakeDamage || damage <= 0 || CurrentHealth <= 0) return;
            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            if (CurrentHealth == 0) Die();
            else StartCoroutine(DamageCooldown());
        }

        public void Kill()
        {
            if (CurrentHealth <= 0) return;
            CurrentHealth = 0;
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            Die();
        }

        private IEnumerator DamageCooldown()
        {
            canTakeDamage = false;
            yield return new WaitForSeconds(damageCooldown);
            canTakeDamage = true;
        }

        private void Die()
        {
            Debug.Log("Player destroyed: health reached zero.");
            Destroy(gameObject);
        }
    }
}
