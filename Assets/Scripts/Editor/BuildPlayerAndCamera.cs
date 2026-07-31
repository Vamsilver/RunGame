#if UNITY_EDITOR
using RunGame.Player;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace RunGame.EditorTools
{
    public static class BuildPlayerAndCamera
    {
        private const string ScenePath = "Assets/Scenes/PortfolioDemo.unity";

        [MenuItem("RunGame/Build/01 Player and Camera")]
        public static void Build()
        {
            PlayerSettings.productName = "RunGame";
            PlayerSettings.companyName = "Vamsilver";
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateLighting();
            CreateEnvironment();
            GameObject player = CreatePlayer();
            CreateCamera(player.transform);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("RunGame stage 01 complete: player and Cinemachine camera created.");
        }

        public static void BuildFromCommandLine()
        {
            Build();
            EditorApplication.Exit(0);
        }

        private static void CreateLighting()
        {
            GameObject lightObject = new("Sun");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.color = new Color(1f, 0.91f, 0.78f);
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.32f, 0.46f, 0.68f);
            RenderSettings.ambientEquatorColor = new Color(0.24f, 0.28f, 0.34f);
            RenderSettings.ambientGroundColor = new Color(0.12f, 0.13f, 0.15f);
        }

        private static void CreateEnvironment()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0f, -0.5f, 14f);
            ground.transform.localScale = new Vector3(22f, 1f, 46f);
            ground.GetComponent<Renderer>().sharedMaterial = CreateMaterial("GroundMaterial", new Color(0.11f, 0.18f, 0.22f), 0.05f, 0.55f);
            CreateWall(new Vector3(-11f, 0.75f, 14f), new Vector3(0.5f, 2.5f, 46f));
            CreateWall(new Vector3(11f, 0.75f, 14f), new Vector3(0.5f, 2.5f, 46f));
            CreateWall(new Vector3(0f, 0.75f, 36.75f), new Vector3(22f, 2.5f, 0.5f));
            CreateRouteMarker(new Vector3(-5.8f, 0.025f, 14f));
            CreateRouteMarker(new Vector3(5.8f, 0.025f, 14f));
            CreateStartGate();
        }

        private static void CreateRouteMarker(Vector3 position)
        {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.name = "Route Edge Light";
            line.transform.position = position;
            line.transform.localScale = new Vector3(0.12f, 0.05f, 43f);
            Object.DestroyImmediate(line.GetComponent<Collider>());
            line.GetComponent<Renderer>().sharedMaterial = CreateMaterial("RouteLightMaterial", new Color(0.04f, 0.65f, 0.88f), 0.2f, 0.75f);
        }

        private static void CreateStartGate()
        {
            Material material = CreateMaterial("StartGateMaterial", new Color(0.04f, 0.85f, 0.72f), 0.25f, 0.72f);
            foreach (float x in new[] { -4.5f, 4.5f })
            {
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pillar.name = "Start Gate Pillar";
                pillar.transform.position = new Vector3(x, 2f, -7f);
                pillar.transform.localScale = new Vector3(0.45f, 4f, 0.45f);
                pillar.GetComponent<Renderer>().sharedMaterial = material;
            }
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "Start Gate Header";
            top.transform.position = new Vector3(0f, 4f, -7f);
            top.transform.localScale = new Vector3(9.4f, 0.45f, 0.45f);
            top.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void CreateWall(Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "ArenaWall";
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().sharedMaterial = CreateMaterial("WallMaterial", new Color(0.18f, 0.27f, 0.33f), 0.1f, 0.35f);
        }

        private static GameObject CreatePlayer()
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 1.1f, -5f);
            player.GetComponent<Renderer>().sharedMaterial = CreateMaterial("PlayerMaterial", new Color(0.08f, 0.72f, 0.94f), 0.25f, 0.62f);
            Rigidbody body = player.AddComponent<Rigidbody>();
            body.mass = 1.3f;
            body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            player.AddComponent<PlayerController>();

            GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visor.name = "Visor";
            visor.transform.SetParent(player.transform, false);
            visor.transform.localPosition = new Vector3(0f, 0.35f, 0.46f);
            visor.transform.localScale = new Vector3(0.58f, 0.25f, 0.08f);
            Object.DestroyImmediate(visor.GetComponent<Collider>());
            visor.GetComponent<Renderer>().sharedMaterial = CreateMaterial("VisorMaterial", new Color(0.03f, 0.07f, 0.1f), 0.7f, 0.82f);
            EnsureFolder("Assets/Prefabs/Player");
            PrefabUtility.SaveAsPrefabAssetAndConnect(player, "Assets/Prefabs/Player/Player.prefab", InteractionMode.AutomatedAction);
            return player;
        }

        private static void CreateCamera(Transform player)
        {
            GameObject mainCameraObject = new("Main Camera");
            mainCameraObject.tag = "MainCamera";
            Camera camera = mainCameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 55f;
            mainCameraObject.AddComponent<AudioListener>();
            mainCameraObject.AddComponent<CinemachineBrain>();

            GameObject cameraObject = new("CM Virtual Camera - Player Follow");
            CinemachineCamera virtualCamera = cameraObject.AddComponent<CinemachineCamera>();
            virtualCamera.Follow = player;
            virtualCamera.LookAt = player;
            virtualCamera.Lens.FieldOfView = 58f;
            CinemachineFollow follow = cameraObject.AddComponent<CinemachineFollow>();
            follow.FollowOffset = new Vector3(0f, 6.2f, -8.5f);
            var tracker = follow.TrackerSettings;
            tracker.PositionDamping = new Vector3(0.45f, 0.65f, 0.55f);
            follow.TrackerSettings = tracker;
            cameraObject.AddComponent<CinemachineRotationComposer>();
        }

        private static Material CreateMaterial(string name, Color color, float metallic, float smoothness)
        {
            EnsureFolder("Assets/Materials");
            string path = $"Assets/Materials/{name}.mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material material = new(shader) { name = name, color = color };
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
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
