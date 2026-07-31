#if UNITY_EDITOR
using RunGame.Effects;
using RunGame.Gameplay;
using RunGame.Player;
using RunGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RunGame.EditorTools
{
    public static class BuildParticlesAndPolish
    {
        private const string ScenePath = "Assets/Scenes/PortfolioDemo.unity";

        [MenuItem("RunGame/Build/05 Particles and Polish")]
        public static void Build()
        {
            EditorSceneManager.OpenScene(ScenePath);
            GameObject player = GameObject.FindWithTag("Player");
            CreateMovementParticles(player);
            CreateInstructions();
            GameObject completionBanner = CreateCompletionBanner();
            CreateGoalMarker(completionBanner);
            Scene scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("RunGame stage 05 complete: movement particles and final scene polish created.");
        }

        public static void BuildFromCommandLine()
        {
            Build();
            EditorApplication.Exit(0);
        }

        private static void CreateMovementParticles(GameObject player)
        {
            Transform previous = player.transform.Find("Movement Dust");
            if (previous != null) Object.DestroyImmediate(previous.gameObject);
            GameObject dust = new("Movement Dust");
            dust.transform.SetParent(player.transform, false);
            dust.transform.localPosition = new Vector3(0f, -0.9f, -0.48f);
            ParticleSystem particles = dust.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.duration = 1f;
            main.loop = true;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.65f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.24f, 0.72f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.48f, 0.43f, 0.36f, 0.62f), new Color(0.72f, 0.68f, 0.6f, 0.35f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.08f;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.75f, 0.08f, 0.5f);
            ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
            color.enabled = true;
            Gradient gradient = new();
            gradient.SetKeys(
                new[] { new GradientColorKey(new Color(0.58f, 0.52f, 0.43f), 0f), new GradientColorKey(new Color(0.34f, 0.32f, 0.3f), 1f) },
                new[] { new GradientAlphaKey(0.58f, 0f), new GradientAlphaKey(0.25f, 0.35f), new GradientAlphaKey(0f, 1f) });
            color.color = gradient;
            ParticleSystem.NoiseModule noise = particles.noise;
            noise.enabled = true;
            noise.strength = 0.22f;
            noise.frequency = 0.55f;

            ParticleSystemRenderer renderer = dust.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = CreateParticleMaterial();
            MovementParticleController controller = dust.AddComponent<MovementParticleController>();
            SerializedObject serialized = new(controller);
            serialized.FindProperty("playerController").objectReferenceValue = player.GetComponent<PlayerController>();
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateInstructions()
        {
            GameObject hud = GameObject.Find("HUD");
            GameInstructions existing = hud.GetComponent<GameInstructions>();
            if (existing == null) hud.AddComponent<GameInstructions>();
            GameObject previous = GameObject.Find("Instructions");
            if (previous != null) Object.DestroyImmediate(previous);
            GameObject textObject = new("Instructions", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(hud.transform, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 28f);
            rect.sizeDelta = new Vector2(900f, 64f);
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 25;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.88f, 0.94f, 1f);
            text.text = "WASD / ARROWS — MOVE     •     R — RESTART";
        }

        private static GameObject CreateCompletionBanner()
        {
            GameObject hud = GameObject.Find("HUD");
            GameObject previous = GameObject.Find("Completion Banner");
            if (previous != null) Object.DestroyImmediate(previous);

            GameObject panel = new("Completion Banner", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(hud.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = new Vector2(0f, 250f);
            panelRect.sizeDelta = new Vector2(720f, 125f);
            panel.GetComponent<Image>().color = new Color(0.025f, 0.07f, 0.08f, 0.92f);

            GameObject textObject = new("Completion Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(panel.transform, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 52;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.2f, 1f, 0.65f);
            text.text = "LEVEL COMPLETE!";
            panel.SetActive(false);
            return panel;
        }

        private static void CreateGoalMarker(GameObject completionBanner)
        {
            GameObject previous = GameObject.Find("Finish Marker");
            if (previous != null) Object.DestroyImmediate(previous);
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "Finish Marker";
            marker.transform.position = new Vector3(0f, 0.05f, 34f);
            marker.transform.localScale = new Vector3(8f, 0.1f, 1.4f);
            marker.GetComponent<Renderer>().sharedMaterial = CreateFinishMaterial();
            marker.GetComponent<Collider>().isTrigger = true;

            ParticleSystem celebrationParticles = CreateCelebrationParticles(marker.transform);
            LevelFinishSequence finishSequence = marker.AddComponent<LevelFinishSequence>();
            SerializedObject serialized = new(finishSequence);
            serialized.FindProperty("completionBanner").objectReferenceValue = completionBanner;
            serialized.FindProperty("celebrationParticles").objectReferenceValue = celebrationParticles;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static ParticleSystem CreateCelebrationParticles(Transform parent)
        {
            GameObject effect = new("Finish Confetti");
            effect.transform.SetParent(parent, false);
            effect.transform.localPosition = new Vector3(0f, 4f, 0f);
            effect.transform.localScale = new Vector3(0.125f, 10f, 0.72f);
            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.8f, 3.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 7f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.32f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.1f, 1f, 0.55f), new Color(1f, 0.28f, 0.7f));
            main.gravityModifier = 0.55f;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 120) });
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(7f, 0.2f, 1f);
            ParticleSystem.RotationOverLifetimeModule rotation = particles.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-5f, 5f);
            particles.GetComponent<ParticleSystemRenderer>().sharedMaterial = CreateConfettiMaterial();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return particles;
        }

        private static Material CreateParticleMaterial()
        {
            const string path = "Assets/Materials/MovementParticleMaterial.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                material.color = new Color(0.62f, 0.57f, 0.49f, 0.52f);
                EditorUtility.SetDirty(material);
                return material;
            }
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit");
            material = new Material(shader) { color = new Color(0.62f, 0.57f, 0.49f, 0.52f) };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Material CreateFinishMaterial()
        {
            const string path = "Assets/Materials/FinishMaterial.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader) { color = new Color(0.08f, 0.95f, 0.55f) };
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(0.02f, 0.45f, 0.17f) * 2f);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Material CreateConfettiMaterial()
        {
            const string path = "Assets/Materials/ConfettiMaterial.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit");
            material = new Material(shader) { color = Color.white };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
#endif
