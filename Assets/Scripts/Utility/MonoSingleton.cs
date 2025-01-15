using UnityEngine;
public class MonoSingleton<DerivedType> : MonoBehaviour where DerivedType : MonoSingleton<DerivedType>
{
	protected static DerivedType s_Instance;
	public static DerivedType Instance
	{
		get
		{
			return s_Instance;
		}
	}

	protected virtual void Awake()
	{
		if(s_Instance != null && s_Instance != this)
		{
			Destroy(gameObject);
			Debug.LogError("[MonoSingleton<" + typeof(DerivedType).Name + ">]" + " duplicate instance was deleted");
			return;
		}

		s_Instance = (DerivedType)this;
	}


	protected virtual void OnDestroy()
	{
		s_Instance = null;
	}
}
