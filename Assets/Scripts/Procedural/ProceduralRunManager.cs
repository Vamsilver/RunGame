using System.Collections;
using System.Collections.Generic;
using RunGame.Collectibles;
using RunGame.Gameplay;
using RunGame.Obstacles;
using RunGame.Persistence;
using RunGame.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RunGame.Procedural
{
    public sealed class ProceduralRunManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] modulePrefabs;
        [SerializeField] private GameObject finishPrefab;
        [SerializeField] private Transform player;
        [SerializeField] private Material bridgeMaterial;
        [SerializeField, Min(0f)] private float moduleGap = 5f;
        [SerializeField] private Text levelText;
        [SerializeField] private Text difficultyText;
        [SerializeField] private Text moduleText;
        [SerializeField] private GameObject completionBanner;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Text countdownText;

        private Wallet wallet;
        private PlayerHealth health;
        private bool transitioning;

        public int CurrentLevel => RunProgress.Level;
        public int CurrentSeed => RunProgress.Seed;
        public float Difficulty => RunProgress.DifficultyMultiplier;

        private void Start()
        {
            RunProgress.Load();
            wallet = player.GetComponent<Wallet>();
            health = player.GetComponent<PlayerHealth>();
            wallet.SetCoins(RunProgress.Coins);
            health.Initialize(RunProgress.Health);
            player.GetComponent<PlayerController>()?.SetDifficulty(Difficulty);
            health.Died += RestartAfterDeath;
            BuildRun();
            RefreshHud();
        }

        private void OnDestroy()
        {
            if (health != null) health.Died -= RestartAfterDeath;
        }

        public void CompleteLevel()
        {
            if (transitioning) return;
            transitioning = true;
            RunProgress.CompleteLevel(wallet.Coins, health.CurrentHealth);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void RestartAfterDeath()
        {
            if (!transitioning) StartCoroutine(ReloadAfterDelay());
        }

        private IEnumerator ReloadAfterDelay()
        {
            transitioning = true;
            yield return new WaitForSeconds(1.1f);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void BuildRun()
        {
            if (modulePrefabs == null || modulePrefabs.Length == 0) return;
            List<int> sequence = GenerateModuleSequence(RunProgress.Seed, RunProgress.ModuleCount, modulePrefabs.Length);
            float nextStart = 5f;

            for (int i = 0; i < sequence.Count; i++)
            {
                GameObject prefab = modulePrefabs[sequence[i]];
                RunModule moduleAsset = prefab.GetComponent<RunModule>();
                float length = moduleAsset != null ? moduleAsset.Length : 18f;
                CreateBridge(nextStart - moduleGap, nextStart);
                GameObject module = Instantiate(prefab, new Vector3(0f, 0f, nextStart + length * 0.5f), Quaternion.identity, transform);
                module.name = $"{i + 1:00} - {moduleAsset?.ModuleName ?? prefab.name}";
                ApplyDifficulty(module);
                nextStart += length + moduleGap;
            }

            CreateBridge(nextStart - moduleGap, nextStart);
            GameObject finish = Instantiate(finishPrefab, new Vector3(0f, 0f, nextStart + 1.5f), Quaternion.identity, transform);
            LevelFinishSequence sequenceController = finish.GetComponent<LevelFinishSequence>();
            if (sequenceController != null)
                sequenceController.Configure(this, completionBanner, nextLevelButton, countdownText);
            List<string> generatedNames = new(sequence.Count);
            foreach (int index in sequence) generatedNames.Add(modulePrefabs[index].GetComponent<RunModule>().ModuleName);
            Debug.Log($"Generated level {RunProgress.Level} with seed {RunProgress.Seed}: {string.Join(" -> ", generatedNames)}");
        }

        private void ApplyDifficulty(GameObject module)
        {
            foreach (BarrelFlowSpawner spawner in module.GetComponentsInChildren<BarrelFlowSpawner>(true))
                spawner.SetDifficulty(Difficulty);
            foreach (OscillatingObstacle obstacle in module.GetComponentsInChildren<OscillatingObstacle>(true))
                obstacle.SetDifficulty(Difficulty);
            foreach (RotatingObstacle obstacle in module.GetComponentsInChildren<RotatingObstacle>(true))
                obstacle.SetDifficulty(Difficulty);
        }

        private void CreateBridge(float start, float end)
        {
            if (end <= start) return;
            GameObject bridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bridge.name = "Safe Connector";
            bridge.transform.SetParent(transform);
            bridge.transform.position = new Vector3(0f, -0.5f, (start + end) * 0.5f);
            bridge.transform.localScale = new Vector3(12f, 1f, end - start);
            bridge.GetComponent<Renderer>().sharedMaterial = bridgeMaterial;
        }

        public static List<int> GenerateModuleSequence(int seed, int count, int typeCount)
        {
            System.Random random = new(seed);
            List<int> result = new(count);
            List<int> firstSet = new(typeCount);
            for (int i = 0; i < typeCount; i++) firstSet.Add(i);
            for (int i = firstSet.Count - 1; i > 0; i--)
            {
                int swap = random.Next(i + 1);
                (firstSet[i], firstSet[swap]) = (firstSet[swap], firstSet[i]);
            }
            foreach (int value in firstSet)
            {
                if (result.Count >= count) break;
                result.Add(value);
            }
            while (result.Count < count)
            {
                int candidate;
                do candidate = random.Next(typeCount);
                while (result.Count > 0 && candidate == result[^1]);
                result.Add(candidate);
            }
            return result;
        }

        private void RefreshHud()
        {
            levelText.text = $"LEVEL  {RunProgress.Level}";
            moduleText.text = $"MODULES  {RunProgress.ModuleCount}";
            float value = Difficulty;
            string label = value < 1.5f ? "EASY" : value < 2.5f ? "NORMAL" : value < 3.5f ? "HARD" : "EXTREME";
            difficultyText.text = $"{label}  x{value:0.00}";
        }
    }
}
