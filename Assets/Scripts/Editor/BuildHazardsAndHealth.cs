#if UNITY_EDITOR
using RunGame.Obstacles;
using RunGame.Player;
using RunGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RunGame.EditorTools
{
    public static class BuildHazardsAndHealth
    {
        private const string ScenePath = "Assets/Scenes/PortfolioDemo.unity";

        [MenuItem("RunGame/Build/04 Hazards and Health")]
        public static void Build()
        {
            EditorSceneManager.OpenScene(ScenePath);
            GameObject player = GameObject.FindWithTag("Player");
            PlayerHealth health = player.GetComponent<PlayerHealth>() ?? player.AddComponent<PlayerHealth>();
            CreateHazards();
            CreateHealthHud(health);
            Scene scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("RunGame stage 04 complete: health, moving hazards, and explosive barrels created.");
        }

        public static void BuildFromCommandLine()
        {
            Build();
            EditorApplication.Exit(0);
        }

        private static void CreateHazards()
        {
            GameObject previous = GameObject.Find("Hazards");
            if (previous != null) Object.DestroyImmediate(previous);
            GameObject root = new("Hazards");
            CreateMovingBlock(root.transform, new Vector3(-4f, 0.8f, 7f), new Vector3(8f, 0f, 0f), "Moving Crusher A");
            CreateMovingBlock(root.transform, new Vector3(4f, 0.8f, 18f), new Vector3(-8f, 0f, 0f), "Moving Crusher B");
            GameObject barrel = CreateBarrel(root.transform, new Vector3(0f, 1f, 27f), "Explosive Barrel");
            EnsureFolder("Assets/Prefabs/Obstacles");
            GameObject barrelPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(
                barrel, "Assets/Prefabs/Obstacles/ExplosiveBarrel.prefab", InteractionMode.AutomatedAction);
            GameObject secondBarrel = (GameObject)PrefabUtility.InstantiatePrefab(barrelPrefab, root.transform);
            secondBarrel.name = "Explosive Barrel 02";
            secondBarrel.transform.position = new Vector3(4f, 1f, 30f);
        }

        private static void CreateMovingBlock(Transform parent, Vector3 position, Vector3 offset, string name)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent);
            block.transform.position = position;
            block.transform.localScale = new Vector3(2.4f, 1.6f, 1.2f);
            block.GetComponent<Renderer>().sharedMaterial = GetOrCreateMaterial("HazardMaterial", new Color(0.92f, 0.08f, 0.06f), 0.25f);
            Rigidbody body = block.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            OscillatingObstacle movement = block.AddComponent<OscillatingObstacle>();
            block.AddComponent<DamageObstacle>();
            SerializedObject serialized = new(movement);
            serialized.FindProperty("localOffset").vector3Value = offset;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateBarrel(Transform parent, Vector3 position, string name)
        {
            GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = name;
            barrel.transform.SetParent(parent);
            barrel.transform.position = position;
            barrel.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            barrel.GetComponent<Renderer>().sharedMaterial = GetOrCreateMaterial("BarrelMaterial", new Color(0.18f, 0.62f, 0.12f), 0.42f);
            Rigidbody body = barrel.AddComponent<Rigidbody>();
            body.isKinematic = true;
            barrel.AddComponent<ExplosiveBarrel>();

            GameObject band = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            band.name = "Warning Band";
            band.transform.SetParent(barrel.transform, false);
            band.transform.localScale = new Vector3(1.02f, 0.22f, 1.02f);
            Object.DestroyImmediate(band.GetComponent<Collider>());
            band.GetComponent<Renderer>().sharedMaterial = GetOrCreateMaterial("WarningMaterial", new Color(1f, 0.64f, 0.02f), 0.15f);
            return barrel;
        }

        private static void CreateHealthHud(PlayerHealth health)
        {
            GameObject hud = GameObject.Find("HUD");
            GameObject previous = GameObject.Find("Health Panel");
            if (previous != null) Object.DestroyImmediate(previous);
            GameObject panel = new("Health Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(hud.transform, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-30f, -30f);
            rect.sizeDelta = new Vector2(330f, 82f);
            panel.GetComponent<Image>().color = new Color(0.025f, 0.05f, 0.08f, 0.88f);

            GameObject fillObject = new("Health Fill", typeof(RectTransform), typeof(Image));
            fillObject.transform.SetParent(panel.transform, false);
            RectTransform fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 0f);
            fillRect.pivot = new Vector2(0f, 0f);
            fillRect.anchoredPosition = new Vector2(15f, 12f);
            fillRect.sizeDelta = new Vector2(-30f, 12f);
            Image fill = fillObject.GetComponent<Image>();
            fill.color = new Color(0.1f, 0.9f, 0.38f);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;

            GameObject textObject = new("Health Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(panel.transform, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(15f, 23f);
            textRect.offsetMax = new Vector2(-15f, -5f);
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 30;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = "HP  100";

            HealthView view = hud.AddComponent<HealthView>();
            SerializedObject serialized = new(view);
            serialized.FindProperty("playerHealth").objectReferenceValue = health;
            serialized.FindProperty("healthText").objectReferenceValue = text;
            serialized.FindProperty("healthFill").objectReferenceValue = fill;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Material GetOrCreateMaterial(string name, Color color, float metallic)
        {
            string path = $"Assets/Materials/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader) { color = color };
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", 0.55f);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            string folder = System.IO.Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent ?? "Assets", folder);
        }
    }
}
#endif
