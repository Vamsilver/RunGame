#if UNITY_EDITOR
using System;
using RunGame.Bonus;
using RunGame.Collectibles;
using RunGame.Effects;
using RunGame.Obstacles;
using RunGame.Player;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RunGame.EditorTools
{
    public static class ValidateRunGame
    {
        private const string ScenePath = "Assets/Scenes/PortfolioDemo.unity";

        [MenuItem("RunGame/Validate Project")]
        public static void Validate()
        {
            EditorSceneManager.OpenScene(ScenePath);
            Require(UnityEngine.Object.FindFirstObjectByType<PlayerController>() != null, "PlayerController missing");
            Require(UnityEngine.Object.FindFirstObjectByType<CinemachineBrain>() != null, "CinemachineBrain missing");
            CinemachineCamera camera = UnityEngine.Object.FindFirstObjectByType<CinemachineCamera>();
            Require(camera != null && camera.Follow != null, "Cinemachine follow target missing");

            BonusTriggerAnimator bonus = UnityEngine.Object.FindFirstObjectByType<BonusTriggerAnimator>();
            Require(bonus != null, "Bonus trigger behaviour missing");
            Animator bonusAnimator = bonus.GetComponentInParent<Animator>();
            AnimatorController controller = bonusAnimator.runtimeAnimatorController as AnimatorController;
            Require(controller != null && controller.parameters.Length > 0, "Bonus Animator Controller missing");
            Require(controller.layers[0].stateMachine.states.Length >= 2, "Bonus state machine needs Idle and Active states");

            Require(UnityEngine.Object.FindFirstObjectByType<Wallet>() != null, "Wallet missing");
            Require(UnityEngine.Object.FindObjectsByType<CoinPickup>(FindObjectsSortMode.None).Length >= 1, "Coins missing");
            Require(UnityEngine.Object.FindFirstObjectByType<PlayerHealth>() != null, "PlayerHealth missing");
            Require(UnityEngine.Object.FindFirstObjectByType<OscillatingObstacle>() != null, "Moving obstacle missing");
            Require(UnityEngine.Object.FindFirstObjectByType<ExplosiveBarrel>() != null, "Explosive barrel missing");
            Require(UnityEngine.Object.FindFirstObjectByType<MovementParticleController>() != null, "Movement particles missing");
            Debug.Log("RunGame validation passed: all required portfolio mechanics are present.");
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

        [MenuItem("RunGame/Build/Windows Player")]
        public static void BuildWindowsPlayer()
        {
            Validate();
            BuildPlayerOptions options = new()
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/RunGame.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException($"Windows build failed: {report.summary.result}");
            Debug.Log($"Windows build passed: {report.summary.totalSize} bytes.");
        }

        public static void BuildWindowsFromCommandLine()
        {
            try
            {
                BuildWindowsPlayer();
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
