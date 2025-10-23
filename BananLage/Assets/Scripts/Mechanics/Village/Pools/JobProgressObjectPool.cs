using System;
using Mono.Cecil;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace Mechanics.Village.Pools
{
    public class JobProgressObjectPool: MonoBehaviour
    {

        private static readonly string Path = "UI/ItemOutputProgress"; 
        
        private ObjectPool<ItemOutputProgress> _pool;
        private ItemOutputProgress _original;

        private void Awake()
        {
            _original = Resources.Load<ItemOutputProgress>(Path);
            Assert.IsNotNull(_original);
            
            _pool = new ObjectPool<ItemOutputProgress>(
                CreateItemOutputProgress,
                OnGetItemOutputProgress, 
                OnReleaseItemOutputProgress,
                OnDestroyItemOutputProgress
            );
        }

        private void OnReleaseItemOutputProgress(ItemOutputProgress obj)
        {
            obj.gameObject.SetActive(false);
            obj.Resource = null;
            obj.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        private void OnGetItemOutputProgress(ItemOutputProgress obj)
        {
            obj.gameObject.SetActive(true);
        }

        private ItemOutputProgress CreateItemOutputProgress()
        {
            return Instantiate(_original);
        }

        private void OnDestroyItemOutputProgress(ItemOutputProgress obj)
        {
            Destroy(obj.gameObject);
        }
        
        public ItemOutputProgress Request() => _pool.Get();

        public void Release(ItemOutputProgress itemOutputProgress) => _pool.Release(itemOutputProgress);
    }
}