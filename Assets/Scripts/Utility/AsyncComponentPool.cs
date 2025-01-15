using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Utility
{
    public class AsyncComponentPool<T> : AsyncAssetPool where T : MonoBehaviour
    {
        private readonly Action<T> _onObjectFirstSpawned;
        private readonly Queue<T> _pool = new();
        
        /// <summary>
        /// Kicks off the asset loads for pool count
        /// </summary>
        public AsyncComponentPool (AssetReference asset, int count, Transform parent, Action<T> onCompCreated = null, Action<GameObject> onObjectCreated = null) : base(asset, count, parent, onObjectCreated)
        {
            _onObjectFirstSpawned = onCompCreated;
        }
        
        /// <summary>
        /// When loaded set up the new object and place it in the pool
        /// </summary>
        protected override void PrefabLoaded(AsyncOperationHandle handle)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded) {return;}
            
            GameObject prefab = (GameObject)handle.Result;
            
            prefab.SetActive(false);
            T componentRef = prefab.GetComponent<T>();

            if (componentRef == null)
            {
                Debug.LogError("Asset reference is not of the appropriate type");
            }

            _onObjectFirstSpawned?.Invoke(componentRef);
                
            //Add to our queue of objects
            _pool.Enqueue(componentRef);

            //Decrement init counter
            InitCounter--;
            CheckIfFullyWarmed();
        }
        
        /// <summary>
        /// Gives an instance from the pool
        /// </summary>
        public T GetInstanceComponent()
        {
            //Increase the pool if below the threshold
            if (_pool.Count <= MinSizeBeforePoolExpand)
            {
                IncreasePool(CachedReference);
            }

            return _pool.Dequeue();
        }

        /// <summary>
        /// Returns an instance from the pool
        /// </summary>
        public void ReturnComponentToQueue(T instance)
        { 
            //Ensure that asset parent is the pool in case it has been moved
            instance.gameObject.transform.parent = ParentTransform;
            instance.gameObject.SetActive(false);
            _pool.Enqueue(instance);
        }
    }
}
