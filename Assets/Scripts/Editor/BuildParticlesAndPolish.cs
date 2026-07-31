#if UNITY_EDITOR
using RunGame.Effects;
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
            CreateGoalMarker();
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
            dust.transform.localPosition = new Vector3(0f, -0.85f, -0.45f);
            dust.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            ParticleSystem particles = dust.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.duration = 1f;
            main.loop = true;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.7f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.7f, 1.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.48f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.42f, 0.72f, 0.9f, 0.65f), new Color(0.8f, 0.9f, 1f, 0.2f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 26f;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 22f;
            shape.radius = 0.32f;
            ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
            color.enabled = true;
            Gradient gradient = new();
            gradient.SetKeys(
                new[] { new GradientColorKey(new Color(0.35f, 0.75f, 1f), 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0.7f, 0f), new GradientAlphaKey(0f, 1f) });
            color.color = gradient;

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

        private static void CreateGoalMarker()
        {
            GameObject previous = GameObject.Find("Finish Marker");
            if (previous != null) Object.DestroyImmediate(previous);
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "Finish Marker";
            marker.transform.position = new Vector3(0f, 0.05f, 34f);
            marker.transform.localScale = new Vector3(8f, 0.1f, 1.4f);
            marker.GetComponent<Renderer>().sharedMaterial = CreateFinishMaterial();
        }

        private static Material CreateParticleMaterial()
        {
            const string path = "Assets/Materials/MovementParticleMaterial.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit");
            material = new Material(shader) { color = new Color(0.45f, 0.8f, 1f, 0.55f) };
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
    }
}
#endif
