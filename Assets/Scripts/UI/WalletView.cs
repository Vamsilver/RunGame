using RunGame.Collectibles;
using UnityEngine;
using UnityEngine.UI;

namespace RunGame.UI
{
    public sealed class WalletView : MonoBehaviour
    {
        [SerializeField] private Wallet wallet;
        [SerializeField] private Text coinText;

        private void OnEnable()
        {
            wallet.CoinsChanged += Refresh;
            Refresh(wallet.Coins);
        }

        private void OnDisable() => wallet.CoinsChanged -= Refresh;
        private void Refresh(int amount) => coinText.text = $"COINS  {amount:00}";
    }
}
