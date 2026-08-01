#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using RunGame.Gameplay;
using RunGame.Effects;
using RunGame.Obstacles;
using RunGame.Procedural;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RunGame.EditorTools
{
    public static class ValidateProceduralRun
    {
        private const string ScenePath = "Assets/Scenes/ProceduralRun.unity";

        [MenuItem("RunGame/Validate Procedural Run")]
        public static void Validate()
        {
            Require(EditorBuildSettings.scenes.Length > 0 && EditorBuildSettings.scenes[0].enabled && EditorBuildSettings.scenes[0].path == ScenePath, "ProceduralRun must be the first enabled build scene");
            EditorSceneManager.OpenScene(ScenePath);
            Require(UnityEngine.Object.FindFirstObjectByType<ProceduralRunManager>() != null, "ProceduralRunManager missing");
            Require(UnityEngine.Object.FindFirstObjectByType<CinemachineCamera>() != null, "Cinemachine camera missing");
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/Player.prefab");
            MovementParticleController movementParticles = playerPrefab.GetComponentInChildren<MovementParticleController>(true);
            Require(movementParticles != null, "Player prefab movement particles missing");
            ParticleSystemRenderer movementRenderer = movementParticles.GetComponent<ParticleSystemRenderer>();
            Require(movementRenderer != null && movementRenderer.sharedMaterial.shader.name == "RunGame/Round Particle", "Player movement particles must use the round particle shader");
            ParticleSystem movementSystem = movementParticles.GetComponent<ParticleSystem>();
            Require(movementParticles.transform.localPosition.y > -0.7f, "Movement dust emitter is buried in the road");
            Require(movementSystem.velocityOverLifetime.enabled && movementSystem.velocityOverLifetime.y.constantMax >= 1f, "Movement dust must rise upward");
            Gradient movementGradient = movementSystem.colorOverLifetime.color.gradient;
            Color particleStart = movementGradient.Evaluate(0f);
            Color particleEnd = movementGradient.Evaluate(1f);
            Require(particleStart.r > 0.9f && particleStart.g > 0.9f && particleStart.b > 0.9f, "Movement particles must start white");
            Require(particleEnd.a <= 0.01f && particleEnd.r < 0.65f, "Movement particles must fade to transparent gray");
            Require(AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Modules" }).Length >= 8, "Module prefabs missing");
            GameObject finish = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Modules/ProceduralFinish.prefab");
            Require(finish != null && finish.GetComponent<LevelFinishSequence>() != null, "Procedural finish missing");
            int checkerTiles = 0;
            foreach (Transform child in finish.transform)
                if (child.name.StartsWith("Checker Tile")) checkerTiles++;
            Require(checkerTiles == 20, "Finish must contain a 10 by 2 black-and-white checker stripe");
            string[] required = { "CoinModule", "BonusModule", "RollingBarrelsModule", "MovingHazardsModule", "StaticBarrelsModule", "DamageSpinnerModule" };
            foreach (string name in required)
            {
                GameObject modulePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Modules/{name}.prefab");
                Require(modulePrefab != null, $"{name} missing");
                foreach (Transform child in modulePrefab.transform)
                    Require(!child.name.StartsWith("Safety Rail"), $"{name} still contains a visible safety rail");
                TextMesh label = modulePrefab.transform.Find("Module Label")?.GetComponent<TextMesh>();
                Require(label != null && label.characterSize <= 0.11f, $"{name} label is wider than the module");
                Require(label.GetComponent<MeshRenderer>().sharedMaterial.shader.name == "RunGame/World Text Depth", $"{name} label is visible through geometry");
            }
            GameObject spinner = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Modules/DamageSpinnerModule.prefab");
            Require(spinner.GetComponentInChildren<RotatingObstacle>(true) != null, "Damage spinner must rotate");
            Require(spinner.GetComponentInChildren<DamageObstacle>(true) != null, "Damage spinner Rigidbody root must deal damage");
            GameObject rollingBarrel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Modules/RollingExplosiveBarrel.prefab");
            Require(rollingBarrel != null, "Rolling barrel prefab missing");
            Vector3 axle = rollingBarrel.transform.rotation * Vector3.up;
            Require(Mathf.Abs(Vector3.Dot(axle.normalized, Vector3.forward)) > 0.99f, "Rolling barrel axle must be horizontal along Z");
            GameObject staticBarrel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Obstacles/ExplosiveBarrel.prefab");
            Require(staticBarrel != null, "Static barrel prefab missing");
            Require(staticBarrel.transform.Find("Warning Light") == null, "Decorative barrel warning light must be removed");
            Require(rollingBarrel.transform.childCount == staticBarrel.transform.childCount, "Rolling and static barrels must share the same visual child structure");
            Require(rollingBarrel.GetComponent<Rigidbody>() != null && !rollingBarrel.GetComponent<Rigidbody>().isKinematic, "Rolling barrel must use dynamic physics");
            GameObject flowModule = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Modules/RollingBarrelsModule.prefab");
            Transform flow = flowModule.transform.Find("Alternating Barrel Flow");
            Require(flow != null && Mathf.Abs(flow.Find("Left Spawn").localPosition.z - flow.Find("Right Spawn").localPosition.z) >= 6f, "Barrel lanes must not intersect");
            Require(Mathf.Abs(flow.Find("Left Spawn").localPosition.x) > 6f && Mathf.Abs(flow.Find("Right Spawn").localPosition.x) > 6f, "Barrels must spawn outside the rails and roll away");
            Require(AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources/CityBuildings" }).Length >= 3, "Colorful city building prefabs missing");
            Require(AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources/CityProps" }).Length >= 2, "City tree and car prefabs missing");
            Require(typeof(IDifficultyScalable).IsAssignableFrom(typeof(BarrelFlowSpawner)), "Barrel flow must use the shared difficulty contract");
            Require(typeof(IDifficultyScalable).IsAssignableFrom(typeof(OscillatingObstacle)), "Moving obstacles must use the shared difficulty contract");
            Require(typeof(IDifficultyScalable).IsAssignableFrom(typeof(RotatingObstacle)), "Rotating obstacles must use the shared difficulty contract");
            List<int> first = ProceduralRunManager.GenerateModuleSequence(123456, 12, 6);
            List<int> repeated = ProceduralRunManager.GenerateModuleSequence(123456, 12, 6);
            Require(string.Join(",", first) == string.Join(",", repeated), "Seed is not reproducible");
            Require(new HashSet<int>(first.GetRange(0, 6)).Count == 6, "Initial sequence does not include every module type");
            for (int i = 1; i < first.Count; i++)
                Require(first[i] != first[i - 1], "Adjacent duplicate modules detected");
            Debug.Log("Procedural Run validation passed: generator, six module types, finish, and Cinemachine are present.");
        }

        public static void ValidateFromCommandLine()
        {
            try
            {
                Validate();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void BuildWindowsFromCommandLine()
        {
            try
            {
                Validate();
                BuildPlayerOptions options = new()
                {
                    scenes = new[] { ScenePath },
                    locationPathName = "Builds/ProceduralRun.exe",
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.None
                };
                BuildReport report = BuildPipeline.BuildPlayer(options);
                if (report.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException($"Procedural Windows build failed: {report.summary.result}");
                Debug.Log($"Procedural Windows build passed: {report.summary.totalSize} bytes.");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
#endif
