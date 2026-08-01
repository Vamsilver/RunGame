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
            CreateRotatingSweeper(root.transform, new Vector3(0f, 0f, 11f));
            CreateMovingBlock(root.transform, new Vector3(-4.5f, 1.05f, 19f), new Vector3(9f, 0f, 0f), "Moving Crusher");
            GameObject barrel = CreateBarrel(root.transform, new Vector3(0f, 1f, 27f), "Explosive Barrel");
            EnsureFolder("Assets/Prefabs/Obstacles");
            GameObject barrelPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(
                barrel, "Assets/Prefabs/Obstacles/ExplosiveBarrel.prefab", InteractionMode.AutomatedAction);
            GameObject secondBarrel = (GameObject)PrefabUtility.InstantiatePrefab(barrelPrefab, root.transform);
            secondBarrel.name = "Explosive Barrel 02";
            secondBarrel.transform.position = new Vector3(4f, 1f, 30f);
        }

        private static void CreateRotatingSweeper(Transform parent, Vector3 position)
        {
            GameObject assembly = new("Rotating Damage Sweeper");
            assembly.transform.SetParent(parent);
            assembly.transform.position = position;

            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "Sweeper Hub";
            pillar.transform.SetParent(assembly.transform, false);
            pillar.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            pillar.transform.localScale = new Vector3(0.7f, 0.8f, 0.7f);
            pillar.GetComponent<Renderer>().sharedMaterial = GetOrCreateMaterial("HazardDarkMaterial", new Color(0.035f, 0.045f, 0.055f), 0.65f);

            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arm.name = "Rotating Damage Arm";
            arm.transform.SetParent(assembly.transform, false);
            arm.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            arm.transform.localScale = new Vector3(9.5f, 0.65f, 0.8f);
            arm.GetComponent<Renderer>().sharedMaterial = GetOrCreateMaterial("HazardMaterial", new Color(0.92f, 0.08f, 0.06f), 0.25f);
            Rigidbody body = arm.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            arm.AddComponent<RotatingObstacle>();
            arm.AddComponent<DamageObstacle>();
            AddWarningStripes(arm.transform, 9);
            CreateWorldLabel(assembly.transform, "ROTATING HAZARD", new Vector3(0f, 2.5f, 0f), new Color(1f, 0.25f, 0.1f));
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
            AddWarningStripes(block.transform, 3);
            CreateWorldLabel(block.transform, "MOVING CRUSHER", new Vector3(0f, 1.3f, 0f), new Color(1f, 0.48f, 0.08f));
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

            for (int i = -1; i <= 1; i++)
            {
                GameObject band = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                band.name = "Warning Band";
                band.transform.SetParent(barrel.transform, false);
                band.transform.localPosition = new Vector3(0f, i * 0.62f, 0f);
                band.transform.localScale = new Vector3(1.03f, 0.1f, 1.03f);
                Object.DestroyImmediate(band.GetComponent<Collider>());
                band.GetComponent<Renderer>().sharedMaterial = GetOrCreateMaterial("WarningMaterial", new Color(1f, 0.64f, 0.02f), 0.15f);
            }
            CreateWorldLabel(barrel.transform, "EXPLOSIVE  •  IMPACT > 5.5", new Vector3(0f, 1.65f, 0f), new Color(1f, 0.7f, 0.12f));
            return barrel;
        }

        private static void AddWarningStripes(Transform parent, int count)
        {
            Material warning = GetOrCreateMaterial("WarningMaterial", new Color(1f, 0.64f, 0.02f), 0.15f);
            for (int i = 0; i < count; i++)
            {
                GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stripe.name = "Warning Stripe";
                stripe.transform.SetParent(parent, false);
                float x = count == 1 ? 0f : Mathf.Lerp(-0.42f, 0.42f, (float)i / (count - 1));
                stripe.transform.localPosition = new Vector3(x, 0.51f, 0f);
                stripe.transform.localScale = new Vector3(0.06f, 0.03f, 1.03f);
                stripe.transform.localRotation = Quaternion.Euler(0f, 0f, 18f);
                Object.DestroyImmediate(stripe.GetComponent<Collider>());
                stripe.GetComponent<Renderer>().sharedMaterial = warning;
            }
        }

        private static void CreateWorldLabel(Transform parent, string value, Vector3 localPosition, Color color)
        {
            GameObject labelObject = new("Hazard Label");
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = localPosition;
            labelObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            TextMesh text = labelObject.AddComponent<TextMesh>();
            text.text = value;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.characterSize = 0.18f;
            text.fontSize = 64;
            text.color = color;
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

        private static Material GetOrCreateEmissiveMaterial(string name, Color color)
        {
            Material material = GetOrCreateMaterial(name, color, 0.1f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 4f);
            EditorUtility.SetDirty(material);
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
