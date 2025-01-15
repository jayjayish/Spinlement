#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Utility.Test
{
    [InitializeOnLoad]
    public class EditorInitializer
    {
        // Editor pref save name, no need to change
        public const string ActiveEditorScene = "PreviousScenePath";
        public const string FirstSceneName = "_Start";
        // your first scene path:
        private const string FirstScenePath = "Assets/Scenes/_Start.unity";
        private const string IsEditorInitialization = "EditorIntialization";
        
        static EditorInitializer()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            // On first stage of play mode enter, save the active scene
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                EditorPrefs.SetString(ActiveEditorScene, SceneManager.GetActiveScene().name);
                EditorPrefs.SetBool(IsEditorInitialization, true);
                SetStartScene();
            }
            else if(state == PlayModeStateChange.EnteredEditMode)
            {
                EditorPrefs.SetBool(IsEditorInitialization, false);
            }
        }
        private static void SetStartScene()
        {
            // If Test mode is active, load the bootstrap scene
            if (TestMode.IsTestModeActive)
            {
                SceneAsset firstSceneToLoad = AssetDatabase.LoadAssetAtPath<SceneAsset>(FirstScenePath);

                if (firstSceneToLoad != null)
                {
                    EditorSceneManager.playModeStartScene = firstSceneToLoad;
                }
            }
            else
            {
                SceneAsset originalScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneManager.GetActiveScene().path);
                
                if (originalScene != null)
                {
                    EditorSceneManager.playModeStartScene = originalScene;
                }
            }
        }
    }
}
#endif
