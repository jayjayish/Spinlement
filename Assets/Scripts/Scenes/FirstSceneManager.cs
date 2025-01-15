using UnityEngine;
using Utility;
#if UNITY_EDITOR
using Utility.Test;
#endif

namespace Scenes
{
	public class FirstSceneManager : MonoBehaviour
	{
		[Header("Scene to load on \"Start\"")]
		[SerializeField] private GlobalTypes.SceneName sceneToLoadOnStart;
		public GlobalTypes.SceneName SceneToLoad => sceneToLoadOnStart;
		
		private static FirstSceneManager _instance;
		public static FirstSceneManager Instance => _instance;
        // ---------------------------------------------------------UNITY EVENTS
        
		private void Awake()
		{
			if (_instance == null)
			{
				_instance = this;
			}
		}

		private void Start()
		{
#if UNITY_EDITOR
			if (TestMode.IsTestModeActive)
			{
				TestModeManager.StartTestMode();
			}
			else
			{
				SceneManager.LoadSceneAsync(sceneToLoadOnStart);
			}
#else
			SceneManager.LoadSceneAsync(sceneToLoadOnStart);
#endif
		}
	}
}
