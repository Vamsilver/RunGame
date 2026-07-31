using RunGame.Player;
using UnityEngine;
using UnityEngine.UI;

namespace RunGame.UI
{
    public sealed class HealthView : MonoBehaviour
    {
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private Text healthText;
        [SerializeField] private Image healthFill;

        private void OnEnable()
        {
            playerHealth.HealthChanged += Refresh;
            Refresh(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }

        private void OnDisable()
        {
            if (playerHealth != null) playerHealth.HealthChanged -= Refresh;
        }

        private void Refresh(int current, int maximum)
        {
            healthText.text = $"HP  {current:000}";
            healthFill.fillAmount = maximum > 0 ? (float)current / maximum : 0f;
        }
    }
}
