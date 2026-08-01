#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using RunGame.Gameplay;
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
            EditorSceneManager.OpenScene(ScenePath);
            Require(UnityEngine.Object.FindFirstObjectByType<ProceduralRunManager>() != null, "ProceduralRunManager missing");
            Require(UnityEngine.Object.FindFirstObjectByType<CinemachineCamera>() != null, "Cinemachine camera missing");
            Require(AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Modules" }).Length >= 7, "Module prefabs missing");
            GameObject finish = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Modules/ProceduralFinish.prefab");
            Require(finish != null && finish.GetComponent<LevelFinishSequence>() != null, "Procedural finish missing");
            string[] required = { "CoinModule", "BonusModule", "RollingBarrelsModule", "MovingHazardsModule", "StaticBarrelsModule" };
            foreach (string name in required)
                Require(AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Modules/{name}.prefab") != null, $"{name} missing");
            List<int> first = ProceduralRunManager.GenerateModuleSequence(123456, 12, 5);
            List<int> repeated = ProceduralRunManager.GenerateModuleSequence(123456, 12, 5);
            Require(string.Join(",", first) == string.Join(",", repeated), "Seed is not reproducible");
            Require(new HashSet<int>(first.GetRange(0, 5)).Count == 5, "First level does not include every base module");
            for (int i = 1; i < first.Count; i++)
                Require(first[i] != first[i - 1], "Adjacent duplicate modules detected");
            Debug.Log("Procedural Run validation passed: generator, five module types, finish, and Cinemachine are present.");
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
