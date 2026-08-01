#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace RunGame.EditorTools
{
    [InitializeOnLoad]
    public static class OpenDefaultScene
    {
        private const string ScenePath = "Assets/Scenes/ProceduralRun.unity";
        private const string SessionKey = "RunGame.DefaultSceneChecked";

        static OpenDefaultScene() => EditorApplication.delayCall += OpenOnFreshSession;

        private static void OpenOnFreshSession()
        {
            if (SessionState.GetBool(SessionKey, false)) return;
            SessionState.SetBool(SessionKey, true);
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            Scene activeScene = SceneManager.GetActiveScene();
            // A fresh clone opens an empty untitled scene. Never replace a scene
            // that the user has already opened or modified.
            if (activeScene.isDirty || !string.IsNullOrEmpty(activeScene.path)) return;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
    }
}
#endif
