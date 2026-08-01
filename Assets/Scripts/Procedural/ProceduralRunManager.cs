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
            CreateContinuousCity(-8f, nextStart + 4f);
            CreateInvisibleBoundary(-8f, nextStart + 4f);
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

        private void CreateContinuousCity(float start, float end)
        {
            GameObject[] buildingPrefabs = Resources.LoadAll<GameObject>("CityBuildings");
            if (buildingPrefabs.Length == 0) return;
            GameObject treePrefab = Resources.Load<GameObject>("CityProps/CityTree");
            GameObject carPrefab = Resources.Load<GameObject>("CityProps/CityCar");
            System.Random random = new(RunProgress.Seed);
            float length = end - start;
            CreateScenerySurface("City Pavement", new Vector3(-10f, -0.62f, (start + end) * 0.5f), new Vector3(8f, 0.24f, length), new Color(0.2f, 0.22f, 0.27f));
            CreateScenerySurface("City Pavement", new Vector3(10f, -0.62f, (start + end) * 0.5f), new Vector3(8f, 0.24f, length), new Color(0.2f, 0.22f, 0.27f));
            CreateScenerySurface("Grass Background", new Vector3(-22f, -0.68f, (start + end) * 0.5f), new Vector3(16f, 0.18f, length), new Color(0.08f, 0.48f, 0.16f));
            CreateScenerySurface("Grass Background", new Vector3(22f, -0.68f, (start + end) * 0.5f), new Vector3(16f, 0.18f, length), new Color(0.08f, 0.48f, 0.16f));

            const float spacing = 7.2f;
            int slotCount = Mathf.CeilToInt(length / spacing);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int slot = 0; slot < slotCount; slot++)
                {
                    float z = start + 3.6f + slot * spacing;
                    if (z > end - 2f) break;
                    bool intersection = slot > 0 && slot % 5 == 0;
                    if (intersection)
                    {
                        CreateScenerySurface("Side Intersection", new Vector3(side * 17f, -0.54f, z), new Vector3(22f, 0.28f, 4.8f), new Color(0.075f, 0.085f, 0.105f));
                        if (carPrefab != null)
                        {
                            GameObject car = Instantiate(carPrefab, transform);
                            car.name = "Intersection Car";
                            car.transform.position = new Vector3(side * (12f + (float)random.NextDouble() * 7f), -0.48f, z);
                            car.transform.rotation = Quaternion.Euler(0f, side < 0 ? 90f : -90f, 0f);
                        }
                        continue;
                    }

                    float width = 3.2f + (float)random.NextDouble() * 2.2f;
                    float depth = 3.4f + (float)random.NextDouble() * 2.4f;
                    float height = 4.5f + (float)random.NextDouble() * 7.5f;
                    float x = side * (9f + (float)random.NextDouble() * 3f);

                    GameObject prefab = buildingPrefabs[random.Next(buildingPrefabs.Length)];
                    GameObject building = Instantiate(prefab, transform);
                    building.name = $"City Building {side}-{slot + 1}";
                    building.transform.position = new Vector3(x, -0.5f, z);
                    building.transform.localScale = new Vector3(width / 4f, height / 6f, depth / 4f);

                    if (treePrefab != null)
                    {
                        GameObject tree = Instantiate(treePrefab, transform);
                        tree.name = "Background Tree";
                        tree.transform.position = new Vector3(side * (15f + (float)random.NextDouble() * 4f), -0.5f, z + ((float)random.NextDouble() - 0.5f) * 3f);
                        tree.transform.localScale = Vector3.one * (0.8f + (float)random.NextDouble() * 0.55f);
                    }
                }
            }
        }

        private void CreateScenerySurface(string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
            surface.name = name;
            surface.transform.SetParent(transform);
            surface.transform.position = position;
            surface.transform.localScale = scale;
            Destroy(surface.GetComponent<Collider>());
            surface.GetComponent<Renderer>().material.color = color;
        }

        private void CreateInvisibleBoundary(float start, float end)
        {
            float length = end - start;
            CreateBoundaryWall("Left Invisible Wall", new Vector3(-6.25f, 3.5f, (start + end) * 0.5f), new Vector3(0.3f, 8f, length));
            CreateBoundaryWall("Right Invisible Wall", new Vector3(6.25f, 3.5f, (start + end) * 0.5f), new Vector3(0.3f, 8f, length));
            CreateBoundaryWall("Start Invisible Wall", new Vector3(0f, 3.5f, start), new Vector3(12.8f, 8f, 0.3f));
            CreateBoundaryWall("Finish Invisible Wall", new Vector3(0f, 3.5f, end), new Vector3(12.8f, 8f, 0.3f));
        }

        private void CreateBoundaryWall(string name, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(transform);
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().enabled = false;
            wall.AddComponent<PlayerBoundary>();
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
