using System;
using UnityEngine;

namespace RunGame.Persistence
{
    public static class RunProgress
    {
        private const string LevelKey = "RunGame.Level";
        private const string SeedKey = "RunGame.Seed";
        private const string CoinsKey = "RunGame.Coins";
        private const string HealthKey = "RunGame.Health";

        public static int Level { get; private set; }
        public static int Seed { get; private set; }
        public static int Coins { get; private set; }
        public static int Health { get; private set; }
        public static int ModuleCount => 4 + Level;
        public static float DifficultyMultiplier => 1f + (Level - 1) * 0.15f;

        public static void Load()
        {
            if (!PlayerPrefs.HasKey(LevelKey)) ResetProgress();
            Level = PlayerPrefs.GetInt(LevelKey, 1);
            Seed = PlayerPrefs.GetInt(SeedKey, CreateSeed());
            Coins = PlayerPrefs.GetInt(CoinsKey, 0);
            Health = Mathf.Clamp(PlayerPrefs.GetInt(HealthKey, 100), 1, 100);
        }

        public static void CompleteLevel(int coins, int health)
        {
            Coins = Mathf.Max(0, coins);
            Health = Mathf.Clamp(health, 1, 100);
            Level++;
            Seed = CreateSeed();
            Save();
        }

        public static void ResetProgress()
        {
            Level = 1;
            Seed = CreateSeed();
            Coins = 0;
            Health = 100;
            Save();
        }

        private static int CreateSeed() => unchecked((int)(DateTime.UtcNow.Ticks ^ Environment.TickCount));

        private static void Save()
        {
            PlayerPrefs.SetInt(LevelKey, Level);
            PlayerPrefs.SetInt(SeedKey, Seed);
            PlayerPrefs.SetInt(CoinsKey, Coins);
            PlayerPrefs.SetInt(HealthKey, Health);
            PlayerPrefs.Save();
        }
    }
}
