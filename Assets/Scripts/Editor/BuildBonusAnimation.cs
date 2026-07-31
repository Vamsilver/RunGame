#if UNITY_EDITOR
using RunGame.Bonus;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RunGame.EditorTools
{
    public static class BuildBonusAnimation
    {
        private const string ScenePath = "Assets/Scenes/PortfolioDemo.unity";
        private const string ControllerPath = "Assets/Animations/Bonus/Bonus.controller";

        [MenuItem("RunGame/Build/02 Bonus Animation")]
        public static void Build()
        {
            EditorSceneManager.OpenScene(ScenePath);
            GameObject previous = GameObject.Find("Animated Bonus");
            if (previous != null) Object.DestroyImmediate(previous);

            AnimatorController controller = CreateAnimatorController();
            CreateBonus(controller);
            Scene scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("RunGame stage 02 complete: trigger-driven bonus state machine created.");
        }

        public static void BuildFromCommandLine()
        {
            Build();
            EditorApplication.Exit(0);
        }

        private static AnimatorController CreateAnimatorController()
        {
            EnsureFolder("Assets/Animations/Bonus");
            AssetDatabase.DeleteAsset(ControllerPath);
            AssetDatabase.DeleteAsset("Assets/Animations/Bonus/BonusIdle.anim");
            AssetDatabase.DeleteAsset("Assets/Animations/Bonus/BonusActive.anim");

            AnimationClip idle = CreateBonusClip("BonusIdle", false);
            AnimationClip active = CreateBonusClip("BonusActive", true);
            AssetDatabase.CreateAsset(idle, "Assets/Animations/Bonus/BonusIdle.anim");
            AssetDatabase.CreateAsset(active, "Assets/Animations/Bonus/BonusActive.anim");

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("PlayerNearby", AnimatorControllerParameterType.Bool);
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            AnimatorState idleState = stateMachine.AddState("Idle", new Vector3(260f, 80f));
            AnimatorState activeState = stateMachine.AddState("Active", new Vector3(520f, 80f));
            idleState.motion = idle;
            activeState.motion = active;
            stateMachine.defaultState = idleState;

            AnimatorStateTransition activate = idleState.AddTransition(activeState);
            activate.hasExitTime = false;
            activate.duration = 0.18f;
            activate.AddCondition(AnimatorConditionMode.If, 0f, "PlayerNearby");

            AnimatorStateTransition deactivate = activeState.AddTransition(idleState);
            deactivate.hasExitTime = false;
            deactivate.duration = 0.2f;
            deactivate.AddCondition(AnimatorConditionMode.IfNot, 0f, "PlayerNearby");
            return controller;
        }

        private static AnimationClip CreateBonusClip(string name, bool active)
        {
            AnimationClip clip = new() { name = name, frameRate = 60f };
            float duration = active ? 0.8f : 1.8f;
            float peakScale = active ? 1.45f : 1.08f;
            float baseHeight = active ? 2f : 1.5f;
            float peakHeight = active ? 2.6f : 1.72f;
            AnimationCurve scale = new(
                new Keyframe(0f, 1f), new Keyframe(duration * 0.5f, peakScale), new Keyframe(duration, 1f));
            AnimationCurve height = new(
                new Keyframe(0f, baseHeight), new Keyframe(duration * 0.5f, peakHeight), new Keyframe(duration, baseHeight));
            AnimationCurve rotation = new(
                new Keyframe(0f, 0f), new Keyframe(duration, active ? 360f : 90f));
            clip.SetCurve("Visual", typeof(Transform), "m_LocalScale.x", scale);
            clip.SetCurve("Visual", typeof(Transform), "m_LocalScale.y", scale);
            clip.SetCurve("Visual", typeof(Transform), "m_LocalScale.z", scale);
            clip.SetCurve("Visual", typeof(Transform), "m_LocalPosition.y", height);
            clip.SetCurve("Visual", typeof(Transform), "localEulerAnglesRaw.y", rotation);
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            return clip;
        }

        private static void CreateBonus(AnimatorController controller)
        {
            GameObject root = new("Animated Bonus");
            root.transform.position = new Vector3(0f, 0f, 4f);

            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Pedestal";
            pedestal.transform.SetParent(root.transform, false);
            pedestal.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            pedestal.transform.localScale = new Vector3(1.35f, 0.2f, 1.35f);
            pedestal.GetComponent<Renderer>().sharedMaterial = GetMaterial("WallMaterial");

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            visual.transform.localScale = Vector3.one;
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.GetComponent<Renderer>().sharedMaterial = CreateBonusMaterial();

            Animator animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            CreateActivationZoneVisuals(root.transform);
            ParticleSystem particles = CreateActivationParticles(root.transform);
            Light activationLight = CreateActivationLight(root.transform);

            GameObject zone = new("Activation Zone");
            zone.transform.SetParent(root.transform, false);
            SphereCollider trigger = zone.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 3.25f;
            BonusTriggerAnimator behaviour = zone.AddComponent<BonusTriggerAnimator>();
            SerializedObject serialized = new(behaviour);
            serialized.FindProperty("bonusAnimator").objectReferenceValue = animator;
            serialized.FindProperty("activationParticles").objectReferenceValue = particles;
            serialized.FindProperty("activationLight").objectReferenceValue = activationLight;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EnsureFolder("Assets/Prefabs/Bonuses");
            PrefabUtility.SaveAsPrefabAssetAndConnect(root, "Assets/Prefabs/Bonuses/AnimatedBonus.prefab", InteractionMode.AutomatedAction);
        }

        private static void CreateActivationZoneVisuals(Transform root)
        {
            Material zoneMaterial = CreateZoneMaterial();
            for (int i = 0; i < 16; i++)
            {
                float angle = i * Mathf.PI * 2f / 16f;
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                marker.name = "Trigger Zone Marker";
                marker.transform.SetParent(root, false);
                marker.transform.localPosition = new Vector3(Mathf.Cos(angle) * 3.25f, 0.08f, Mathf.Sin(angle) * 3.25f);
                marker.transform.localRotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg, 0f);
                marker.transform.localScale = new Vector3(0.85f, 0.08f, 0.18f);
                Object.DestroyImmediate(marker.GetComponent<Collider>());
                marker.GetComponent<Renderer>().sharedMaterial = zoneMaterial;
            }

            GameObject labelObject = new("Bonus Zone Label");
            labelObject.transform.SetParent(root, false);
            labelObject.transform.localPosition = new Vector3(0f, 0.18f, -3.6f);
            labelObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text = "BONUS  ACTIVATION  ZONE";
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.22f;
            label.fontSize = 72;
            label.color = new Color(0.88f, 0.55f, 1f);
        }

        private static ParticleSystem CreateActivationParticles(Transform root)
        {
            GameObject effect = new("Bonus Active Particles");
            effect.transform.SetParent(root, false);
            effect.transform.localPosition = new Vector3(0f, 1.3f, 0f);
            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.8f, 0.25f, 1f), Color.white);
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 32f;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 1.1f;
            effect.GetComponent<ParticleSystemRenderer>().sharedMaterial = CreateBonusParticleMaterial();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return particles;
        }

        private static Light CreateActivationLight(Transform root)
        {
            GameObject lightObject = new("Bonus Active Light");
            lightObject.transform.SetParent(root, false);
            lightObject.transform.localPosition = new Vector3(0f, 2f, 0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.72f, 0.2f, 1f);
            light.intensity = 4.5f;
            light.range = 7f;
            light.enabled = false;
            return light;
        }

        private static Material CreateZoneMaterial()
        {
            const string path = "Assets/Materials/BonusZoneMaterial.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader) { color = new Color(0.56f, 0.12f, 0.85f) };
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(0.5f, 0.08f, 0.9f) * 3f);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Material CreateBonusParticleMaterial()
        {
            const string path = "Assets/Materials/BonusParticleMaterial.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit");
            material = new Material(shader) { color = new Color(0.82f, 0.28f, 1f, 0.75f) };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Material CreateBonusMaterial()
        {
            const string path = "Assets/Materials/BonusMaterial.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader) { color = new Color(0.75f, 0.18f, 1f) };
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(0.48f, 0.04f, 0.8f) * 2.2f);
            material.SetFloat("_Metallic", 0.35f);
            material.SetFloat("_Smoothness", 0.8f);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Material GetMaterial(string name) => AssetDatabase.LoadAssetAtPath<Material>($"Assets/Materials/{name}.mat");

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
