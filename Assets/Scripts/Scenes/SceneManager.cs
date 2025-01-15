using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Utility;

namespace Scenes
{
	/// <summary>
	/// Static class in charge of managing all scenes loaded dynamically
	/// and additively.
	/// </summary>
	public static class SceneManager
	{
		/// <summary>
		/// Cached addressable script. obj. containing all scene settings
		/// As this ref is loaded async as a script. obj. if the cached val is null,
		/// it must be obtained by calling "_GetSceneSettingsAsync()"
		/// </summary>
		private static SceneSettingsContainer _sceneSettingsContainer;
		
		//			--- LOADING SCENE VARS ---
		
		/// <summary>
		/// Will contain all scene instances by scene type
		/// </summary>
		private static readonly Dictionary<GlobalTypes.SceneName, SceneInstance> LoadedScenes = new ();
		
		/// <summary>
		/// Collections used to keep track of the loading/unloading in progress
		/// scenes. This is to prevent loading/unloading a new scene if a same scene
		/// type is in the middle of an async process of being loaded/unloaded.
		///  
		/// </summary>
		private static readonly HashSet<GlobalTypes.SceneName> LoadingScenes = new ();
		private static readonly HashSet<GlobalTypes.SceneName> UnloadingScenes = new ();
		
		//			--- UNLOADING SCENE VARS ---
		
		private static GlobalTypes.SceneName _lastSceneLoaded = GlobalTypes.SceneName.None;
		/// <summary>
		/// Returns the last scene loaded
		/// </summary>
		public static GlobalTypes.SceneName LastSceneLoaded => _lastSceneLoaded;
		
		// --------------------------------------------------------PUBLIC EVENTS
		
		/// <summary>
		/// Fired in "LoadSceneAsync()" after successfully loading a scene
		/// and after that method's local "onLoad" event arg.
		/// </summary>
		public static Action<SceneSettings> OnSceneLoad;
		
		/// <summary>
		/// Fired in "UnloadSceneAsync()" after successfully unloading a scene
		/// and after that method's local "onUnload" event arg.
		/// </summary>
		public static Action<GlobalTypes.SceneName> OnSceneUnload;
		
		// -------------------------------------------------------PUBLIC METHODS
		
		/// <summary>
		/// Loads a scene asynchronously by sceneType.
		/// If successful, "onLoad" action will be called on success after
		/// the async operation.
		/// <br /><br />
		/// If "willSceneBeActive" = true, the loaded scene will be the active
		/// scene among all existing scenes
		/// (newly instantiated objects will be created in the active scene
		/// Also, lighting will act upon the active scene as well)
		/// <br /><br />
		/// No scene will be loaded if a scene with the given sceneType
		/// has already been loaded and hasn't been unloaded.
		/// <br /><br />
		/// The scene must exist in the "so-sceneSettings" scriptable
		/// object as an entry and its "SceneType" must be unique among all entries
		/// </summary>
		public static void LoadSceneAsync(GlobalTypes.SceneName sceneName, bool willSceneBeActive = true, Action<GlobalTypes.SceneName> onLoad = null)
		{
			if (LoadingScenes.Contains(sceneName))
			{
				_LogError( 
					$"Cannot load new scene of type {sceneName}. " +
					$"A scene of the same type is already being loaded");
				return;
			}
			
			if (LoadedScenes.ContainsKey(sceneName))
			{
				_LogError($"Cannot load scene of type {sceneName}. It's already loaded");
				return;
			}
			
			// Checks finished. About to load scene
			
			// Delegate the loading scene workload after
			// checking for potential errors above
			_LoadSceneAsync(sceneName, willSceneBeActive, onLoad);
		}

		/// <summary>
		/// Unloads a scene asynchronously by sceneType. The scene must have
		/// been loaded previously with "LoadSceneAsync()".
		/// <br /><br />
		/// This method will return if no scene of the given type was
		/// loaded and currently active.
		/// </summary>
		public static void UnloadSceneAsync(GlobalTypes.SceneName sceneName, Action<GlobalTypes.SceneName> onUnload = null)
		{
			if (UnloadingScenes.Contains(sceneName))
			{
				_LogError( 
					$"Cannot unload new scene of type {sceneName}. " +
					$"A scene of the same type is already being unloaded");
				return;
			}
			
			if (! LoadedScenes.ContainsKey(sceneName))
			{
				_LogError($"Cannot unload scene of type {sceneName}. It's not loaded");
				return;
			}
			
			// Delegate the unloading scene workload after
			// checking for potential errors above
			_UnloadSceneAsync(sceneName, onUnload);
		}
		
		/// <summary>
		/// Retrieves the Scene Settings entry that corresponds to the given
		/// "sceneName" if it exists.
		/// <br /><br />
		/// If no entries are found, an empty list will be returned
		/// </summary>
		public static void GetSceneSettingsAsync(
			GlobalTypes.SceneName sceneName, Action <IList<SceneSettings>> onGet)
		{
			// Set (Caches) the "_sceneSettingsContainer" variable if it's null.
			// Needed before using "_sceneSettingsContainer" below
			_CacheSceneSettingsVarAsync(() =>
			{
				IList<SceneSettings> scenes = _sceneSettingsContainer.GetAllSettings(sceneName);
				onGet?.Invoke(scenes);
			});
		}
		
		/// <summary>
		/// Returns true if the reference of an already loaded scene is found.
		/// The scene itself is returned as "scene" out var 
		/// <br />
		/// If no scene ref is found, false will be returned. 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="scene"></param>
		/// <returns></returns>
		public static bool TryGetLoadedSceneInstance(GlobalTypes.SceneName name, out Scene scene)
		{
			if (LoadedScenes.TryGetValue(name, out var instance))
			{
				scene = instance.Scene;
				return true;
			}
			scene = default;
			return false;
		}

		// ------------------------------------------------------PRIVATE METHODS
		
		/// <summary>
		/// Caches the scene settings script. obj. addressable.
		/// Only if _sceneSettings = null, the setting will be loaded async
		///
		/// This method must be called before using "_sceneSettings" to set it
		/// </summary>
		private static void _CacheSceneSettingsVarAsync(Action onSuccess)
		{
			if (_sceneSettingsContainer == null)
			{
				// Cache the scene settings async
				Addressables.LoadAssetAsync<SceneSettingsContainer>(GlobalAddresses.SceneContainerAddr).Completed += handle =>
				{
					if (handle.Status == AsyncOperationStatus.Succeeded)
					{
						_sceneSettingsContainer = handle.Result;
						onSuccess.Invoke();
					}
					else
					{
						_LogError("SceneSettings couldn't be loaded");
					}
				};
			}
			else
			{
				onSuccess.Invoke();
			}
		}

		/// <summary>
		/// Will do all the scene loading process
		/// </summary>
		private static void _LoadSceneAsync(
			GlobalTypes.SceneName sceneName, 
			bool willSceneBeActive = true,
			Action<GlobalTypes.SceneName> onLoad = null)
		{
			// Set (Caches) the "_sceneSettingsContainer" variable if it's null
			// Needed before using "_sceneSettingsContainer" below
			_CacheSceneSettingsVarAsync(() =>
			{
				if (! _sceneSettingsContainer.TryGetSetting(sceneName, out SceneSettings sceneEntry))
				{
					_LogError( 
						$"Cannot load scene." +
						$"Scene of type {sceneName} could not be found " +
						$"in SceneSettings");
				}
				else
				{
					LoadingScenes.Add(sceneName);
					
					Addressables.LoadSceneAsync(sceneEntry.SceneRef, LoadSceneMode.Additive).Completed += asyncHandle =>
					{
						if (asyncHandle.Status == AsyncOperationStatus.Succeeded)
						{
							SceneInstance tSceneInst = asyncHandle.Result;

							_lastSceneLoaded = sceneName; 
							LoadedScenes.Add(sceneName, tSceneInst);
							_Log($"Scene of type {sceneName} loaded successfully");
							
							// Set this scene as active ?
							if (willSceneBeActive)
							{
								UnityEngine.SceneManagement.SceneManager.SetActiveScene(tSceneInst.Scene);
								_Log($"Scene of type {sceneName} is now the active scene");
							}
							
							onLoad?.Invoke(sceneName); // Local arg event
							OnSceneLoad?.Invoke(sceneEntry); // Static class event
						}
						else
						{
							_LogError( 
								$"Cannot async load scene of type {sceneName}: " +
								$"Status non successful");
						}
					};
				}
			});
		}
		
		/// <summary>
		/// Will do all the scene unloading process
		/// </summary>
		private static void _UnloadSceneAsync(
			GlobalTypes.SceneName sceneName, 
			Action<GlobalTypes.SceneName> onUnload = null)
		{
			SceneInstance tSceneInst = LoadedScenes[sceneName];
			
			UnloadingScenes.Add(sceneName);
			
			Addressables.UnloadSceneAsync(tSceneInst).Completed += asyncHandle =>
			{
				if (asyncHandle.Status == AsyncOperationStatus.Succeeded)
				{
					LoadedScenes.Remove(sceneName);
					_Log($"Scene of type {sceneName} unloaded successfully");
				
					onUnload?.Invoke(sceneName); // Local arg event
					OnSceneUnload?.Invoke(sceneName); // Static class event
				}
				else
				{
					_LogError( 
						$"Cannot async unload scene of type {sceneName}: " +
						$"Status non successful");
				}
				
				UnloadingScenes.Remove(sceneName);
			};
		}

		//			--- DEBUG LOG ---
		
		private static void _Log(string message) => UnityEngine.Debug.Log(message);
		private static void _LogError(string message) => UnityEngine.Debug.LogError(message);
	}
}
