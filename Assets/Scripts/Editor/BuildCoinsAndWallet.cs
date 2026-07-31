#if UNITY_EDITOR
using RunGame.Collectibles;
using RunGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RunGame.EditorTools
{
    public static class BuildCoinsAndWallet
    {
        private const string ScenePath = "Assets/Scenes/PortfolioDemo.unity";

        [MenuItem("RunGame/Build/03 Coins and Wallet")]
        public static void Build()
        {
            EditorSceneManager.OpenScene(ScenePath);
            GameObject player = GameObject.FindWithTag("Player");
            Wallet wallet = player.GetComponent<Wallet>() ?? player.AddComponent<Wallet>();
            CreateCoins();
            CreateWalletHud(wallet);
            Scene scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("RunGame stage 03 complete: coins, wallet, and wallet UI created.");
        }

        public static void BuildFromCommandLine()
        {
            Build();
            EditorApplication.Exit(0);
        }

        private static void CreateCoins()
        {
            GameObject previous = GameObject.Find("Coins");
            if (previous != null) Object.DestroyImmediate(previous);
            GameObject parent = new("Coins");
            Vector3[] positions =
            {
                new(-2.5f, 1.15f, 0f), new(0f, 1.15f, 2.5f), new(2.5f, 1.15f, 5f),
                new(0f, 1.15f, 8f), new(-3f, 1.15f, 11f), new(0f, 1.15f, 14f),
                new(3f, 1.15f, 17f), new(0f, 1.15f, 20f), new(-2f, 1.15f, 24f)
            };
            Material material = CreateCoinMaterial();
            GameObject coinPrefab = CreateCoinPrefab(material);
            for (int i = 0; i < positions.Length; i++)
            {
                GameObject coin = (GameObject)PrefabUtility.InstantiatePrefab(coinPrefab, parent.transform);
                coin.name = $"Coin {i + 1:00}";
                coin.transform.position = positions[i];
            }
        }

        private static GameObject CreateCoinPrefab(Material material)
        {
            EnsureFolder("Assets/Prefabs/Collectibles");
            GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coin.name = "Coin";
            coin.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            coin.transform.localScale = new Vector3(0.52f, 0.1f, 0.52f);
            coin.GetComponent<Renderer>().sharedMaterial = material;
            coin.GetComponent<Collider>().isTrigger = true;
            coin.AddComponent<CoinPickup>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(coin, "Assets/Prefabs/Collectibles/Coin.prefab");
            Object.DestroyImmediate(coin);
            return prefab;
        }

        private static void CreateWalletHud(Wallet wallet)
        {
            GameObject previous = GameObject.Find("HUD");
            if (previous != null) Object.DestroyImmediate(previous);
            GameObject canvasObject = new("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            GameObject panel = new("Wallet Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasObject.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(30f, -30f);
            panelRect.sizeDelta = new Vector2(280f, 82f);
            panel.GetComponent<Image>().color = new Color(0.025f, 0.05f, 0.08f, 0.88f);

            GameObject textObject = new("Coin Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(panel.transform, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(18f, 8f);
            textRect.offsetMax = new Vector2(-18f, -8f);
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 34;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(1f, 0.78f, 0.12f);
            text.text = "COINS  00";

            WalletView view = canvasObject.AddComponent<WalletView>();
            SerializedObject serialized = new(view);
            serialized.FindProperty("wallet").objectReferenceValue = wallet;
            serialized.FindProperty("coinText").objectReferenceValue = text;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Material CreateCoinMaterial()
        {
            const string path = "Assets/Materials/CoinMaterial.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader) { color = new Color(1f, 0.58f, 0.04f) };
            material.SetFloat("_Metallic", 0.72f);
            material.SetFloat("_Smoothness", 0.8f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(0.35f, 0.11f, 0.01f));
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
