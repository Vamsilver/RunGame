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

            AnimationClip idle = CreateScaleClip("BonusIdle", new[]
            {
                new Keyframe(0f, 1f), new Keyframe(1f, 1f)
            });
            AnimationClip active = CreateScaleClip("BonusActive", new[]
            {
                new Keyframe(0f, 1f), new Keyframe(0.45f, 1.35f), new Keyframe(0.9f, 1f)
            });
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

        private static AnimationClip CreateScaleClip(string name, Keyframe[] keys)
        {
            AnimationClip clip = new() { name = name, frameRate = 60f };
            AnimationCurve curve = new(keys);
            clip.SetCurve("Visual", typeof(Transform), "m_LocalScale.x", curve);
            clip.SetCurve("Visual", typeof(Transform), "m_LocalScale.y", curve);
            clip.SetCurve("Visual", typeof(Transform), "m_LocalScale.z", curve);
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            return clip;
        }

        private static void CreateBonus(AnimatorController controller)
        {
            GameObject root = new("Animated Bonus");
            root.transform.position = new Vector3(-5.5f, 0f, 4f);

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
            visual.transform.localScale = new Vector3(0.85f, 1.2f, 0.85f);
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.GetComponent<Renderer>().sharedMaterial = CreateBonusMaterial();

            Animator animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            GameObject zone = new("Activation Zone");
            zone.transform.SetParent(root.transform, false);
            SphereCollider trigger = zone.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 3.25f;
            BonusTriggerAnimator behaviour = zone.AddComponent<BonusTriggerAnimator>();
            SerializedObject serialized = new(behaviour);
            serialized.FindProperty("bonusAnimator").objectReferenceValue = animator;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EnsureFolder("Assets/Prefabs/Bonuses");
            PrefabUtility.SaveAsPrefabAssetAndConnect(root, "Assets/Prefabs/Bonuses/AnimatedBonus.prefab", InteractionMode.AutomatedAction);
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
