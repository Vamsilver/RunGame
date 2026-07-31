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
        private Renderer playerRenderer;
        private Color normalColor;
        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public event Action<int, int> HealthChanged;

        private void Awake()
        {
            CurrentHealth = maxHealth;
            playerRenderer = GetComponent<Renderer>();
            if (playerRenderer != null) normalColor = playerRenderer.material.color;
        }

        private void Start() => HealthChanged?.Invoke(CurrentHealth, maxHealth);

        public void TakeDamage(int damage)
        {
            if (!canTakeDamage || damage <= 0 || CurrentHealth <= 0) return;
            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            if (playerRenderer != null) StartCoroutine(DamageFlash());
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

        private IEnumerator DamageFlash()
        {
            playerRenderer.material.color = new Color(1f, 0.12f, 0.08f);
            yield return new WaitForSeconds(0.16f);
            if (playerRenderer != null) playerRenderer.material.color = normalColor;
        }

        private void Die()
        {
            Debug.Log("Player destroyed: health reached zero.");
            Destroy(gameObject);
        }
    }
}
