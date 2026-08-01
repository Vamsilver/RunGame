#if UNITY_EDITOR
using RunGame.Bonus;
using RunGame.Player;
using UnityEditor;
using UnityEngine;

namespace RunGame.EditorTools
{
    public static class BuildGameplayEffects
    {
        public static void BuildFromCommandLine()
        {
            EnsureFolder("Assets/Resources/Effects");
            Material fireMaterial = GetOrCreateMaterial("Assets/Resources/Effects/FireParticleMaterial.mat", "RunGame/Round Particle", Color.white);
            Material healMaterial = GetOrCreateMaterial("Assets/Resources/Effects/HealPlusMaterial.mat", "RunGame/Plus Particle", Color.white);
            ConfigurePlayerDamageEffect(fireMaterial);
            ConfigureHealingEffect(healMaterial);
            AssetDatabase.SaveAssets();
            Debug.Log("Fire explosion, healing pluses, and player damage effects configured.");
            EditorApplication.Exit(0);
        }

        private static void ConfigurePlayerDamageEffect(Material material)
        {
            const string path = "Assets/Prefabs/Player/Player.prefab";
            GameObject player = PrefabUtility.LoadPrefabContents(path);
            Transform previous = player.transform.Find("Damage Hit Effect");
            if (previous != null) Object.DestroyImmediate(previous.gameObject);
            GameObject effect = new("Damage Hit Effect");
            effect.transform.SetParent(player.transform, false);
            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.46f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 5.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.14f, 0.42f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.08f, 0.015f), new Color(1f, 0.48f, 0.04f));
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 24) });
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.65f;
            ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
            color.enabled = true;
            color.color = FadeGradient(new Color(1f, 0.22f, 0.02f), new Color(0.38f, 0.01f, 0.01f));
            effect.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            PrefabUtility.SaveAsPrefabAsset(player, path);
            PrefabUtility.UnloadPrefabContents(player);
        }

        private static void ConfigureHealingEffect(Material material)
        {
            const string path = "Assets/Prefabs/Bonuses/AnimatedBonus.prefab";
            GameObject bonus = PrefabUtility.LoadPrefabContents(path);
            BonusTriggerAnimator trigger = bonus.GetComponentInChildren<BonusTriggerAnimator>(true);
            SerializedObject serialized = new(trigger);
            ParticleSystem particles = serialized.FindProperty("activationParticles").objectReferenceValue as ParticleSystem;
            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.45f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.55f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.22f, 0.42f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.25f, 1f, 0.3f), new Color(0.7f, 1f, 0.72f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 13f;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.8f;
            ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.y = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
            ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
            color.enabled = true;
            color.color = FadeGradient(new Color(0.2f, 1f, 0.28f), new Color(0.05f, 0.48f, 0.12f));
            particles.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            PrefabUtility.SaveAsPrefabAsset(bonus, path);
            PrefabUtility.UnloadPrefabContents(bonus);
        }

        private static Gradient FadeGradient(Color start, Color end)
        {
            Gradient gradient = new();
            gradient.SetKeys(
                new[] { new GradientColorKey(start, 0f), new GradientColorKey(end, 1f) },
                new[] { new GradientAlphaKey(0.95f, 0f), new GradientAlphaKey(0.65f, 0.55f), new GradientAlphaKey(0f, 1f) });
            return gradient;
        }

        private static Material GetOrCreateMaterial(string path, string shaderName, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find(shaderName);
            if (material == null)
            {
                material = new Material(shader) { color = color };
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
                material.color = color;
                EditorUtility.SetDirty(material);
            }
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
