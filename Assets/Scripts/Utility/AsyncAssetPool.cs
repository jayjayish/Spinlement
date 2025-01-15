using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Utility
{
    /// <summary>
    /// Takes asset references, spawns them asynchronously and pools them for quick and easy (synchronous) access
    /// </summary>
    public class AsyncAssetPool
    {
        private readonly Action<GameObject> _onObjectFirstSpawned;
        
        //Called when all assets have been loaded
        public Action OnPoolWarmComplete;
        //The default size and the amount to increase the pool if it gets low
        private readonly int _poolGrowSize;
        //The obstacle pool which holds the objects
        private readonly Queue<GameObject> _pool = new();
        //Used in initialization to track how many objects have loaded
        protected int InitCounter;
        protected AssetReference CachedReference;
        protected const int MinSizeBeforePoolExpand = 2;
        protected readonly Transform Parent;

        public Transform ParentTransform => Parent;
        
        /// <summary>
        /// Kicks off the asset loads for pool count
        /// </summary>
        public AsyncAssetPool(AssetReference asset, int count, Transform parent, Action<GameObject> onObjectCreated = null)
        {
            _onObjectFirstSpawned = onObjectCreated;
            _poolGrowSize = count;
            Parent = parent;
            
            IncreasePool(asset);
        }
        
        /// <summary>
        /// Increases the pool count by the default amount by loading the addressable
        /// </summary>
        protected void IncreasePool(AssetReference asset)
        {
            CachedReference = asset;
            
            for (int i = 0; i < _poolGrowSize; i++)
            {
                AsyncOperationHandle handle = Parent == null ? 
                    Addressables.InstantiateAsync(asset) : 
                    Addressables.InstantiateAsync(asset, Parent);
                handle.Completed += PrefabLoaded;
                InitCounter++;
            }
        }

        /// <summary>
        /// When loaded set up the new object and place it in the pool
        /// </summary>
        protected virtual void PrefabLoaded(AsyncOperationHandle handle)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded) {return;}
            
            GameObject prefab = (GameObject)handle.Result;
            prefab.SetActive(false);
            _onObjectFirstSpawned?.Invoke(prefab);
                
            //Add to our queue of objects
            _pool.Enqueue(prefab);

            //Decrement init counter
            InitCounter--;
            CheckIfFullyWarmed();
        }

        /// <summary>
        /// Checks if the pool is fully warm so the loading screen can be dropped
        /// </summary>
        protected void CheckIfFullyWarmed()
        {
            if (InitCounter == 0)
            {
                OnPoolWarmComplete?.Invoke();
            }
        }

        /// <summary>
        /// Gives an instance from the pool
        /// </summary>
        public GameObject GetInstance()
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
        public void ReturnToQueue(GameObject instance)
        {
            //Ensure that asset parent is the pool in case it has been moved
            instance.gameObject.transform.parent = ParentTransform;
            instance.SetActive(false);
            _pool.Enqueue(instance);
        }
    }
}
