using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace Scenes
{
	/// <summary>
	/// Represents a collection of levels to be loaded additively.
	/// </summary>
	[CreateAssetMenu(fileName = "so-sceneSettingsContainer", menuName = "ScriptableObjects/so-sceneSettingsContainer", order = 1)]
	public class SceneSettingsContainer : ScriptableObject, IReadOnlyList<SceneSettings>
	{
		[SerializeField]
		private List<SceneSettings> sceneSettingsEntries;
		
		/// <summary>
		/// Returns true if it finds the entry in "SceneEntries"
		/// matching the given "sceneName". That entry will also be returned as an out var.
		/// Will return False if no entry is found of the given type
		/// </summary>
		/// <returns></returns>
		public bool TryGetSetting(GlobalTypes.SceneName sceneName, out SceneSettings @out)
		{
			foreach (SceneSettings setting in sceneSettingsEntries)
			{
				if (setting != null && setting.SceneName == sceneName)
				{
					@out = setting;
					return true;
				}
			}
			@out = null;
			return false;
		}
		
		/// <summary>
		/// Returns all scene settings containing the given "sceneName".
		/// If no settings are found, an empty list will be returned
		/// </summary>
		/// <returns></returns>
		public IList<SceneSettings> GetAllSettings(GlobalTypes.SceneName sceneName)
		{
			List<SceneSettings> tSettings = new List<SceneSettings>();
			foreach (SceneSettings setting in sceneSettingsEntries)
			{
				if (setting.SceneName == sceneName)
				{
					tSettings.Add(setting);
				}
			}
			return tSettings;
		}

		#region List Implementation
		public IEnumerator<SceneSettings> GetEnumerator()
		{
			return sceneSettingsEntries.GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)sceneSettingsEntries).GetEnumerator();
		}

		public bool Contains(SceneSettings item)
		{
			return sceneSettingsEntries.Contains(item);
		}

		public void CopyTo(SceneSettings[] array, int arrayIndex)
		{
			sceneSettingsEntries.CopyTo(array, arrayIndex);
		}

		public int Count => sceneSettingsEntries.Count;

		public bool IsReadOnly => true;

		public int IndexOf(SceneSettings item)
		{
			return sceneSettingsEntries.IndexOf(item);
		}

		public SceneSettings this[int index] => sceneSettingsEntries[index];

		#endregion
	}
}
