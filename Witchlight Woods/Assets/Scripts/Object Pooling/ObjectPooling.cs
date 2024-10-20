using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

namespace WitchlightWoods
{
    public static class ObjectPooling
    {
        private static Dictionary<GameObject, PrefabPool> _pools = new ();
        private static GameObject _poolParent;
        public static PrefabPool GetOrCreate(GameObject prefab)
        {
            if (_poolParent == null)
            {
                _pools.Clear();
                _poolParent = new GameObject("Object Pooling");
                _poolParent.OnDestroyAsync().ContinueWith(() =>
                {
                    foreach (var (_, pool) in _pools)
                    {
                        pool.Clear();
                        pool.Dispose();
                    }
                    _pools.Clear();
                });
            }
            
            if (_pools.TryGetValue(prefab, out var value))
                return value;

            var pool = new PrefabPool(prefab, _poolParent.transform);
            _pools.Add(prefab, pool);
            return pool;
        }

        public static PrefabPool Get(GameObject prefab) => _pools.TryGetValue(prefab, out var pool) ? pool : null;

        public static async UniTask TimedRelease(this (GameObject instance, PrefabPool pool) pair, TimeSpan time, DelayType delayType = DelayType.DeltaTime)
        {
            await UniTask.Delay(time, delayType);
            pair.pool.Release(pair.instance);
        }

        public static async UniTaskVoid OneShotAnimation(AnimatedVisualEffect animation, Vector3 position, Quaternion rotation = default)
        {
            if (animation.prefab == null) return;
            var pool = GetOrCreate(animation.prefab.gameObject);
            var effect = pool.GetAt(position, rotation);
            var animator = effect.go.GetComponent<Animator>()!;
            animator.Play(animation.stateName);
            await effect.TimedRelease(TimeSpan.FromSeconds(animator.GetCurrentAnimatorStateInfo(0).length));
        }
    }
}