using System;
using System.Collections;
using UnityEngine;

namespace RunGame.Player
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maxHealth = 100;
        [SerializeField, Min(0f)] private float damageCooldown = 0.65f;
        [SerializeField] private ParticleSystem damageParticles;

        private bool canTakeDamage = true;
        private Renderer playerRenderer;
        private Color normalColor;
        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public event Action<int, int> HealthChanged;
        public event Action Died;

        private void Awake()
        {
            CurrentHealth = maxHealth;
            if (damageParticles == null)
                damageParticles = transform.Find("Damage Hit Effect")?.GetComponent<ParticleSystem>();
            playerRenderer = GetComponent<Renderer>();
            if (playerRenderer != null) normalColor = playerRenderer.material.color;
        }

        private void Start() => HealthChanged?.Invoke(CurrentHealth, maxHealth);

        public void Initialize(int health)
        {
            CurrentHealth = Mathf.Clamp(health, 1, maxHealth);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public int Heal(int amount)
        {
            if (amount <= 0 || CurrentHealth <= 0) return 0;
            int previous = CurrentHealth;
            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            return CurrentHealth - previous;
        }

        public void TakeDamage(int damage)
        {
            if (!canTakeDamage || damage <= 0 || CurrentHealth <= 0) return;
            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            if (playerRenderer != null) StartCoroutine(DamageFlash());
            if (damageParticles != null) damageParticles.Play(true);
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
            Died?.Invoke();
            Destroy(gameObject);
        }
    }
}
