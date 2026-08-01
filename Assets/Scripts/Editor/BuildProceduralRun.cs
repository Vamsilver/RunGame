#if UNITY_EDITOR
using RunGame.Collectibles;
using RunGame.Gameplay;
using RunGame.Obstacles;
using RunGame.Player;
using RunGame.Procedural;
using RunGame.UI;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RunGame.EditorTools
{
    public static class BuildProceduralRun
    {
        private const string ScenePath = "Assets/Scenes/ProceduralRun.unity";
        private const float ModuleLength = 18f;

        [MenuItem("RunGame/Build/06 Procedural Run")]
        public static void Build()
        {
            EnsureFolder("Assets/Prefabs/Modules");
            GameObject rollingBarrel = CreateRollingBarrelPrefab();
            GameObject[] modules =
            {
                CreateCoinModule(),
                CreateBonusModule(),
                CreateRollingBarrelModule(rollingBarrel),
                CreateMovingHazardModule(),
                CreateStaticBarrelModule(),
                CreateSpinnerModule()
            };
            GameObject finishPrefab = CreateFinishPrefab();
            CreateScene(modules, finishPrefab);
            AssetDatabase.SaveAssets();
            Debug.Log("RunGame procedural run complete: six modules and runtime scene created.");
        }

        public static void BuildFromCommandLine()
        {
            Build();
            EditorApplication.Exit(0);
        }

        public static void BuildRollingBarrelOnlyFromCommandLine()
        {
            CreateRollingBarrelPrefab();
            AssetDatabase.SaveAssets();
            Debug.Log("Rolling barrel rebuilt from the static barrel visual prefab.");
            EditorApplication.Exit(0);
        }

        public static void RepairObstacleModulesFromCommandLine()
        {
            RemoveWarningLightFromStaticBarrel();
            GameObject rollingBarrel = CreateRollingBarrelPrefab();
            CreateRollingBarrelModule(rollingBarrel);
            CreateSpinnerModule();
            AssetDatabase.SaveAssets();
            Debug.Log("Obstacle modules repaired: horizontal barrel flow, clean barrel visual, and damaging spinner.");
            EditorApplication.Exit(0);
        }

        private static GameObject CreateCoinModule()
        {
            GameObject root = CreateModuleBase("Coin Module", "COIN RUN", new Color(0.12f, 0.7f, 0.95f));
            AddCoins(root.transform, new[]
            {
                new Vector3(-2f, 1.1f, -6f), new Vector3(0f, 1.1f, -3f), new Vector3(2f, 1.1f, 0f),
                new Vector3(0f, 1.1f, 3f), new Vector3(-2f, 1.1f, 6f)
            });
            return SaveModule(root, "CoinModule");
        }

        private static GameObject CreateBonusModule()
        {
            GameObject root = CreateModuleBase("Bonus Module", "HEALING BONUS +25 HP", new Color(0.72f, 0.18f, 1f));
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Bonuses/AnimatedBonus.prefab");
            GameObject bonus = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            bonus.transform.SetParent(root.transform, false);
            bonus.transform.localPosition = Vector3.zero;
            AddCoins(root.transform, new[] { new Vector3(-3.8f, 1.1f, -6f), new Vector3(3.8f, 1.1f, 6f) });
            return SaveModule(root, "BonusModule");
        }

        private static GameObject CreateRollingBarrelModule(GameObject rollingBarrel)
        {
            GameObject root = CreateModuleBase("Rolling Barrels Module", "ROLLING BARREL FLOW", new Color(1f, 0.35f, 0.05f));
            Material rampMaterial = GetMaterial("HazardDarkMaterial");
            CreateRamp(root.transform, new Vector3(-4.3f, 1.25f, 0f), -13f, rampMaterial);
            CreateRamp(root.transform, new Vector3(4.3f, 1.25f, 0f), 13f, rampMaterial);

            GameObject spawnerObject = new("Alternating Barrel Flow");
            spawnerObject.transform.SetParent(root.transform, false);
            Transform left = new GameObject("Left Spawn").transform;
            left.SetParent(spawnerObject.transform, false);
            left.localPosition = new Vector3(-5.2f, 2.45f, 0f);
            Transform right = new GameObject("Right Spawn").transform;
            right.SetParent(spawnerObject.transform, false);
            right.localPosition = new Vector3(5.2f, 2.45f, 0f);
            BarrelFlowSpawner spawner = spawnerObject.AddComponent<BarrelFlowSpawner>();
            SerializedObject serialized = new(spawner);
            serialized.FindProperty("rollingBarrelPrefab").objectReferenceValue = rollingBarrel;
            serialized.FindProperty("leftSpawn").objectReferenceValue = left;
            serialized.FindProperty("rightSpawn").objectReferenceValue = right;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AddCoins(root.transform, new[] { new Vector3(0f, 1.1f, -6f), new Vector3(0f, 1.1f, 6f) });
            return SaveModule(root, "RollingBarrelsModule");
        }

        private static GameObject CreateMovingHazardModule()
        {
            GameObject root = CreateModuleBase("Moving Hazards Module", "MOVING DAMAGE ZONE", new Color(1f, 0.08f, 0.06f));
            CreateMovingHazard(root.transform, new Vector3(-4f, 0.9f, -4f), new Vector3(8f, 0f, 0f), 2.6f);
            CreateMovingHazard(root.transform, new Vector3(4f, 0.9f, 4f), new Vector3(-8f, 0f, 0f), 2.2f);
            AddCoins(root.transform, new[] { new Vector3(0f, 1.1f, -7f), new Vector3(0f, 1.1f, 0f), new Vector3(0f, 1.1f, 7f) });
            return SaveModule(root, "MovingHazardsModule");
        }

        private static GameObject CreateStaticBarrelModule()
        {
            GameObject root = CreateModuleBase("Static Barrels Module", "LETHAL BARREL SLALOM", new Color(0.25f, 0.9f, 0.18f));
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Obstacles/ExplosiveBarrel.prefab");
            Vector3[] positions =
            {
                new(-2.8f, 1f, -6f), new(2.8f, 1f, -3f), new(-2.8f, 1f, 0f),
                new(2.8f, 1f, 3f), new(-2.8f, 1f, 6f)
            };
            foreach (Vector3 position in positions)
            {
                GameObject barrel = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                barrel.transform.SetParent(root.transform, false);
                barrel.transform.localPosition = position;
            }
            AddCoins(root.transform, new[]
            {
                new Vector3(2.8f, 1.1f, -6f), new Vector3(-2.8f, 1.1f, -3f), new Vector3(2.8f, 1.1f, 0f),
                new Vector3(-2.8f, 1.1f, 3f), new Vector3(2.8f, 1.1f, 6f)
            });
            return SaveModule(root, "StaticBarrelsModule");
        }

        private static GameObject CreateSpinnerModule()
        {
            GameObject root = CreateModuleBase("Damage Spinner Module", "DAMAGE SPINNER", new Color(1f, 0.12f, 0.35f));
            CreateDamageSpinner(root.transform, new Vector3(0f, 0.65f, 0f));
            AddCoins(root.transform, new[]
            {
                new Vector3(-3.8f, 1.1f, -6f), new Vector3(3.8f, 1.1f, -3f),
                new Vector3(-3.8f, 1.1f, 3f), new Vector3(3.8f, 1.1f, 6f)
            });
            return SaveModule(root, "DamageSpinnerModule");
        }

        private static void CreateDamageSpinner(Transform root, Vector3 position)
        {
            GameObject spinner = new("Damage Spinner");
            spinner.transform.SetParent(root, false);
            spinner.transform.localPosition = position;
            Rigidbody body = spinner.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            spinner.AddComponent<RotatingObstacle>();
            spinner.AddComponent<DamageObstacle>();

            GameObject hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hub.name = "Spinner Hub";
            hub.transform.SetParent(spinner.transform, false);
            hub.transform.localScale = new Vector3(0.7f, 0.45f, 0.7f);
            hub.GetComponent<Renderer>().sharedMaterial = GetMaterial("HazardDarkMaterial");

            for (int i = 0; i < 2; i++)
            {
                GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blade.name = $"Damage Blade {i + 1}";
                blade.transform.SetParent(spinner.transform, false);
                blade.transform.localRotation = Quaternion.Euler(0f, i * 90f, 0f);
                blade.transform.localScale = new Vector3(10f, 0.45f, 0.65f);
                blade.GetComponent<Renderer>().sharedMaterial = GetMaterial("HazardMaterial");
            }
        }

        private static void RemoveWarningLightFromStaticBarrel()
        {
            const string path = "Assets/Prefabs/Obstacles/ExplosiveBarrel.prefab";
            GameObject contents = PrefabUtility.LoadPrefabContents(path);
            Transform warningLight = contents.transform.Find("Warning Light");
            if (warningLight != null) Object.DestroyImmediate(warningLight.gameObject);
            PrefabUtility.SaveAsPrefabAsset(contents, path);
            PrefabUtility.UnloadPrefabContents(contents);
        }

        private static GameObject CreateModuleBase(string name, string label, Color accent)
        {
            GameObject root = new(name);
            RunModule module = root.AddComponent<RunModule>();
            SerializedObject serialized = new(module);
            serialized.FindProperty("length").floatValue = ModuleLength;
            serialized.FindProperty("moduleName").stringValue = name;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Module Ground";
            ground.transform.SetParent(root.transform, false);
            ground.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            ground.transform.localScale = new Vector3(12f, 1f, ModuleLength);
            ground.GetComponent<Renderer>().sharedMaterial = GetMaterial("GroundMaterial");
            CreateRail(root.transform, -6f, accent);
            CreateRail(root.transform, 6f, accent);
            CreateLabel(root.transform, label, accent);
            return root;
        }

        private static void CreateRail(Transform root, float x, Color color)
        {
            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "Safety Rail";
            rail.transform.SetParent(root, false);
            rail.transform.localPosition = new Vector3(x, 0.4f, 0f);
            rail.transform.localScale = new Vector3(0.25f, 0.8f, ModuleLength);
            rail.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"ModuleAccent_{ColorUtility.ToHtmlStringRGB(color)}", color, true);
        }

        private static void CreateLabel(Transform root, string value, Color color)
        {
            GameObject label = new("Module Label");
            label.transform.SetParent(root, false);
            label.transform.localPosition = new Vector3(0f, 0.08f, -7.8f);
            label.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            TextMesh text = label.AddComponent<TextMesh>();
            text.text = value;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontSize = 58;
            text.characterSize = 0.18f;
            text.color = color;
        }

        private static void CreateRamp(Transform root, Vector3 position, float zAngle, Material material)
        {
            GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.name = "Barrel Ramp";
            ramp.transform.SetParent(root, false);
            ramp.transform.localPosition = position;
            ramp.transform.localRotation = Quaternion.Euler(0f, 0f, zAngle);
            ramp.transform.localScale = new Vector3(4.2f, 0.35f, 4f);
            ramp.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void CreateMovingHazard(Transform root, Vector3 position, Vector3 offset, float duration)
        {
            GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.name = "Moving Damage Block";
            obstacle.transform.SetParent(root, false);
            obstacle.transform.localPosition = position;
            obstacle.transform.localScale = new Vector3(2.2f, 1.8f, 1.2f);
            obstacle.GetComponent<Renderer>().sharedMaterial = GetMaterial("HazardMaterial");
            Rigidbody body = obstacle.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            OscillatingObstacle movement = obstacle.AddComponent<OscillatingObstacle>();
            obstacle.AddComponent<DamageObstacle>();
            SerializedObject serialized = new(movement);
            serialized.FindProperty("localOffset").vector3Value = offset;
            serialized.FindProperty("cycleDuration").floatValue = duration;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddCoins(Transform root, Vector3[] positions)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Collectibles/Coin.prefab");
            foreach (Vector3 position in positions)
            {
                GameObject coin = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                coin.transform.SetParent(root, false);
                coin.transform.localPosition = position;
            }
        }

        private static GameObject SaveModule(GameObject root, string fileName)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, $"Assets/Prefabs/Modules/{fileName}.prefab");
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateRollingBarrelPrefab()
        {
            GameObject staticPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Obstacles/ExplosiveBarrel.prefab");
            GameObject barrel = (GameObject)PrefabUtility.InstantiatePrefab(staticPrefab);
            PrefabUtility.UnpackPrefabInstance(barrel, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            barrel.name = "Rolling Explosive Barrel";
            Transform warningLight = barrel.transform.Find("Warning Light");
            if (warningLight != null) Object.DestroyImmediate(warningLight.gameObject);
            // A barrel moving across X needs its axle along Z, so rotate the
            // cylinder's default Y axis onto Z.
            barrel.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            Rigidbody body = barrel.GetComponent<Rigidbody>();
            body.isKinematic = false;
            body.mass = 2.8f;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(barrel, "Assets/Prefabs/Modules/RollingExplosiveBarrel.prefab");
            Object.DestroyImmediate(barrel);
            return prefab;
        }

        private static GameObject CreateFinishPrefab()
        {
            GameObject finish = GameObject.CreatePrimitive(PrimitiveType.Cube);
            finish.name = "Procedural Finish Zone";
            finish.transform.localScale = new Vector3(10f, 0.1f, 3f);
            finish.GetComponent<Renderer>().sharedMaterial = GetMaterial("FinishMaterial");
            finish.GetComponent<Collider>().isTrigger = true;
            LevelFinishSequence sequence = finish.AddComponent<LevelFinishSequence>();
            GameObject effect = new("Finish Confetti");
            effect.transform.SetParent(finish.transform, false);
            effect.transform.localPosition = new Vector3(0f, 40f, 0f);
            effect.transform.localScale = new Vector3(0.1f, 10f, 0.34f);
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
            particles.GetComponent<ParticleSystemRenderer>().sharedMaterial = GetMaterial("ConfettiMaterial");
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            SetReference(sequence, "celebrationParticles", particles);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(finish, "Assets/Prefabs/Modules/ProceduralFinish.prefab");
            Object.DestroyImmediate(finish);
            return prefab;
        }

        private static void CreateScene(GameObject[] modules, GameObject finishPrefab)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateLighting();
            GameObject player = CreatePlayer();
            CreateCamera(player.transform);
            CreateStartPlatform();
            CreateHud(player, modules, finishPrefab);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene("Assets/Scenes/PortfolioDemo.unity", true)
            };
        }

        private static GameObject CreatePlayer()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/Player.prefab");
            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            player.name = "Player";
            player.transform.position = new Vector3(0f, 1.1f, -4f);
            if (player.GetComponent<Wallet>() == null) player.AddComponent<Wallet>();
            if (player.GetComponent<PlayerHealth>() == null) player.AddComponent<PlayerHealth>();
            return player;
        }

        private static void CreateCamera(Transform player)
        {
            GameObject main = new("Main Camera");
            main.tag = "MainCamera";
            main.AddComponent<Camera>().fieldOfView = 58f;
            main.AddComponent<AudioListener>();
            main.AddComponent<CinemachineBrain>();
            GameObject virtualObject = new("CM Virtual Camera - Procedural Run");
            CinemachineCamera camera = virtualObject.AddComponent<CinemachineCamera>();
            camera.Follow = player;
            camera.LookAt = player;
            camera.Lens.FieldOfView = 58f;
            CinemachineFollow follow = virtualObject.AddComponent<CinemachineFollow>();
            follow.FollowOffset = new Vector3(0f, 6.2f, -8.5f);
            virtualObject.AddComponent<CinemachineRotationComposer>();
        }

        private static void CreateLighting()
        {
            GameObject sun = new("Sun");
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
            RenderSettings.ambientLight = new Color(0.3f, 0.36f, 0.44f);
        }

        private static void CreateStartPlatform()
        {
            GameObject start = GameObject.CreatePrimitive(PrimitiveType.Cube);
            start.name = "Start Platform";
            start.transform.position = new Vector3(0f, -0.5f, -4f);
            start.transform.localScale = new Vector3(12f, 1f, 8f);
            start.GetComponent<Renderer>().sharedMaterial = GetMaterial("GroundMaterial");
        }

        private static void CreateHud(GameObject player, GameObject[] modules, GameObject finishPrefab)
        {
            GameObject canvasObject = new("Procedural HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            Text walletText = CreateHudText(canvasObject.transform, "Wallet", "COINS  00", new Vector2(30f, -30f), new Vector2(300f, 70f), new Vector2(0f, 1f));
            Text healthText = CreateHudText(canvasObject.transform, "Health", "HP  100", new Vector2(-30f, -30f), new Vector2(300f, 70f), new Vector2(1f, 1f));
            Text levelText = CreateHudText(canvasObject.transform, "Level", "LEVEL  1", new Vector2(0f, -25f), new Vector2(260f, 55f), new Vector2(0.5f, 1f));
            Text moduleText = CreateHudText(canvasObject.transform, "Modules", "MODULES  5", new Vector2(0f, -82f), new Vector2(260f, 48f), new Vector2(0.5f, 1f));
            Text difficultyText = CreateHudText(canvasObject.transform, "Difficulty", "EASY  x1.00", new Vector2(0f, -132f), new Vector2(340f, 48f), new Vector2(0.5f, 1f));
            WalletView walletView = canvasObject.AddComponent<WalletView>();
            SetReference(walletView, "wallet", player.GetComponent<Wallet>());
            SetReference(walletView, "coinText", walletText);

            GameObject completion = CreateCompletionPanel(canvasObject.transform, out Button nextButton, out Text countdown);
            GameObject managerObject = new("Procedural Run Manager");
            ProceduralRunManager manager = managerObject.AddComponent<ProceduralRunManager>();
            SerializedObject serialized = new(manager);
            SerializedProperty array = serialized.FindProperty("modulePrefabs");
            array.arraySize = modules.Length;
            for (int i = 0; i < modules.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = modules[i];
            serialized.FindProperty("finishPrefab").objectReferenceValue = finishPrefab;
            serialized.FindProperty("player").objectReferenceValue = player.transform;
            serialized.FindProperty("bridgeMaterial").objectReferenceValue = GetMaterial("GroundMaterial");
            serialized.FindProperty("levelText").objectReferenceValue = levelText;
            serialized.FindProperty("difficultyText").objectReferenceValue = difficultyText;
            serialized.FindProperty("moduleText").objectReferenceValue = moduleText;
            serialized.FindProperty("completionBanner").objectReferenceValue = completion;
            serialized.FindProperty("nextLevelButton").objectReferenceValue = nextButton;
            serialized.FindProperty("countdownText").objectReferenceValue = countdown;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject eventSystem = new("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.transform.SetParent(canvasObject.transform, false);
            HealthView healthView = canvasObject.AddComponent<HealthView>();
            SetReference(healthView, "playerHealth", player.GetComponent<PlayerHealth>());
            SetReference(healthView, "healthText", healthText);
            Image healthFill = healthText.transform.parent.GetComponent<Image>();
            healthFill.type = Image.Type.Filled;
            healthFill.fillMethod = Image.FillMethod.Horizontal;
            healthFill.color = new Color(0.08f, 0.45f, 0.22f, 0.9f);
            SetReference(healthView, "healthFill", healthFill);
        }

        private static Text CreateHudText(Transform parent, string name, string value, Vector2 position, Vector2 size, Vector2 anchor)
        {
            GameObject obj = new(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            obj.GetComponent<Image>().color = new Color(0.02f, 0.05f, 0.07f, 0.84f);
            GameObject textObject = new($"{name} Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(obj.transform, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 28;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = value;
            return text;
        }

        private static GameObject CreateCompletionPanel(Transform parent, out Button button, out Text countdown)
        {
            GameObject panel = new("Next Level Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(700f, 300f);
            panel.GetComponent<Image>().color = new Color(0.02f, 0.07f, 0.08f, 0.94f);
            CreatePanelText(panel.transform, "LEVEL COMPLETE!", new Vector2(0f, 90f), 46);
            countdown = CreatePanelText(panel.transform, "Next level starts in 30", new Vector2(0f, 25f), 25);
            GameObject buttonObject = new("Next Level Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(panel.transform, false);
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = buttonRect.anchorMax = buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = new Vector2(0f, -75f);
            buttonRect.sizeDelta = new Vector2(330f, 75f);
            buttonObject.GetComponent<Image>().color = new Color(0.08f, 0.8f, 0.45f);
            button = buttonObject.GetComponent<Button>();
            CreatePanelText(buttonObject.transform, "NEXT LEVEL", Vector2.zero, 30);
            panel.SetActive(false);
            return panel;
        }

        private static Text CreatePanelText(Transform parent, string value, Vector2 position, int fontSize)
        {
            GameObject obj = new(value, typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(620f, 65f);
            Text text = obj.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = value;
            return text;
        }

        private static void SetReference(Object target, string propertyName, Object value)
        {
            SerializedObject serialized = new(target);
            serialized.FindProperty(propertyName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Material GetMaterial(string name) => AssetDatabase.LoadAssetAtPath<Material>($"Assets/Materials/{name}.mat");

        private static Material CreateMaterial(string name, Color color, bool emission)
        {
            string path = $"Assets/Materials/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader) { color = color };
            if (emission)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 2f);
            }
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
