using System;
using UnityEngine;

namespace RunGame.Collectibles
{
    public sealed class Wallet : MonoBehaviour
    {
        [SerializeField, Min(0)] private int coins;

        public int Coins => coins;
        public event Action<int> CoinsChanged;

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            coins += amount;
            CoinsChanged?.Invoke(coins);
        }
    }
}
