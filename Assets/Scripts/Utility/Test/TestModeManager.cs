#if UNITY_EDITOR
using System.Linq;
using Scenes;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Utility.Test
{
    public static class TestModeManager
    {
        /// <summary>
        /// Loads the active editor scene or the scene the bootstrap targets
        /// </summary>
        public static void StartTestMode()
        {
            // Get the active scene when play was pressed
            var playModeStartSceneName = EditorPrefs.GetString(EditorInitializer.ActiveEditorScene);
            
            // If the active scene was the bootstrap scene, load the scene bootstrap wants to goto
            if (playModeStartSceneName == EditorInitializer.FirstSceneName)
            {
                SceneManager.LoadSceneAsync(FirstSceneManager.Instance.SceneToLoad);
            }
            else
            {
                // Load the scene settings data
                var sceneHandle = Addressables.LoadAssetAsync<SceneSettingsContainer>(GlobalAddresses.SceneContainerAddr);
                sceneHandle.Completed += (handle) =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        var sceneSettingsContainer = handle.Result;
                        // Search the scene settings data for the SceneName enum so that SceneManager can load the scene with the correct flow
                        var sceneSetting = sceneSettingsContainer.FirstOrDefault((x) => x.SceneRef != null && x.SceneRef.editorAsset != null && x.SceneRef.editorAsset.name == playModeStartSceneName);
                    
                        if (!sceneSetting)
                        {
                            Debug.LogError("Playmode start scene " + playModeStartSceneName + " could not be found in scene data");
                            return;
                        }
                        
                        SceneManager.LoadSceneAsync(sceneSetting.SceneName);
                    }
                    else
                    {
                        Debug.LogError( "Failed loading scene data container");
                    }
                };
            }
        }
    }
}
#endif
