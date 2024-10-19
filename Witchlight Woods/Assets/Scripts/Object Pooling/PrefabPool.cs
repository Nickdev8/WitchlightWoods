using System;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace WitchlightWoods
{
    public class PrefabPool : ObjectPool<GameObject>
    {
        public readonly GameObject Prefab;
        public PrefabPool(GameObject prefab, Transform parent = null) : base(() => Create(prefab, parent), Get, ReleaseInstance, Destroy)
        {
            Prefab = prefab;
        }
        private static GameObject Create(GameObject prefab, Transform parent) => Object.Instantiate(prefab, parent);
        private static void Get(GameObject obj) => obj.SetActive(true);
        private static void ReleaseInstance(GameObject obj) => obj.SetActive(false);
        private static void Destroy(GameObject obj) => Object.Destroy(obj);

        public (GameObject go, PrefabPool pool) GetAt(Vector3 position, Quaternion rotation = default)
        {
            var go = Get()!;
            go.transform.position = position;
            go.transform.rotation = rotation;
            return (go, this);
        }
    }
}
