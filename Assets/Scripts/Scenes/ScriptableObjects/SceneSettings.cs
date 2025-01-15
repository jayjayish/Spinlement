using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utility;

namespace Scenes
{
	/// <summary>
	/// Represents an entry in the SceneSettings scriptable object
	/// </summary>
	[CreateAssetMenu(fileName = "so-sceneSettings", menuName = "ScriptableObjects/so-sceneSettings", order = 1)]

	[Serializable]
	public class SceneSettings : ScriptableObject
	{
		[SerializeField]
		private GlobalTypes.SceneName sceneName;

		public GlobalTypes.SceneName SceneName => sceneName;

		[Header("Scene Addressable Address Reference")] 
		[SerializeField]
		private AssetReference sceneRef;

		public AssetReference SceneRef => sceneRef;
	}
}
