using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Homebrew
{
    public class PoolStash
    {
        public bool globalPool = false;
        public List<GameObject> activeObjs = new List<GameObject>(100);

        protected Transform parent;
        protected Dictionary<int, Stack<GameObject>> cachedObjects = new Dictionary<int, Stack<GameObject>>(100, new FastComparable());
        protected Dictionary<int, IPoolable> cachedPoolables = new Dictionary<int, IPoolable>(100, new FastComparable());
        protected Dictionary<int, int> cachedIds = new Dictionary<int, int>(100, new FastComparable());
 
        public void SetPoolParent(Transform parent)
        {
            this.parent = parent;
        }

        public void RegisterObject(GameObject prefab)
        {
            var key = prefab.GetInstanceID();
            Stack<GameObject> stack;
            var hasValue = cachedObjects.TryGetValue(key, out stack);
            if (!hasValue) cachedObjects.Add(key, new Stack<GameObject>());
        }

        public void AddToPool(GameObject go, GameObject prefab)
        {
            cachedIds.Add(go.GetInstanceID(), prefab.GetInstanceID());
        }

        public PoolStash PopulateWith(GameObject prefab, int amount, int amountPerTick = 10, int timeRate = 1)
        {
            var key = prefab.GetInstanceID();
            Stack<GameObject> stack;
            var hasValue = cachedObjects.TryGetValue(key, out stack);
            if (!hasValue) cachedObjects.Add(key, new Stack<GameObject>(amount));


            Timer.Add(Time.DeltaTime * timeRate, () =>
            {
                for (var i = 0; i < amountPerTick; i++)
                {
                    if (amount == 0) break;
                    Populate(prefab, key);
                    amount--;
                }

                if (amount > 0)
                {
                    Timer.Add(Time.DeltaTime * timeRate, () => PopulateWith(prefab, amount, amountPerTick, timeRate));
                }
            });


            return this;
        }

        public GameObject Spawn(GameObject prefab, Vector3 position = default(Vector3),
            Quaternion rotation = default(Quaternion), Transform parent = null, bool isPosLocal = false)
        {
            if (parent == null)
            {
                parent = this.parent;
            }

            var key = prefab.GetInstanceID();
            Stack<GameObject> objs;
            var stacked = cachedObjects.TryGetValue(key, out objs);

            if (stacked && objs.Count > 0)
            {
                var transform = objs.Pop().transform;
                transform.SetParent(parent);
                transform.rotation = rotation;
                transform.gameObject.SetActive(true);
                if (isPosLocal) transform.localPosition = position;
                else transform.position = position;

                IPoolable poolable;
                if (cachedPoolables.TryGetValue(transform.gameObject.GetInstanceID(), out poolable))
                    poolable.Spawn(true);

                return transform.gameObject;
            }


            if (!stacked)
            {
                cachedObjects.Add(key, new Stack<GameObject>(100));
            }

            var createdPrefab = Populate(prefab, position, rotation, parent, isPosLocal);
            var k = createdPrefab.GetInstanceID();

            var p = createdPrefab.GetComponent<IPoolable>();
            if (p != null)
            {
                if (!cachedPoolables.ContainsKey(k))
                {
                    cachedPoolables.Add(k, p);
                }
            }


            cachedIds.Add(k, key);
            return createdPrefab;
        }

        public void Despawn(GameObject go)
        {
            go.SetActive(false);

            cachedObjects[cachedIds[go.GetInstanceID()]].Push(go);

            IPoolable poolable;
            if (cachedPoolables.TryGetValue(go.GetInstanceID(), out poolable))
            {
                poolable.Spawn(false);
            }
        }

        public void DespawnAll()
        {
            for (var i = 0; i < activeObjs.Count; i++)
            {
                Despawn(activeObjs[i]);
            }

            activeObjs.Clear();
        }

        public void Dispose()
        {
            cachedPoolables.Clear();
            cachedObjects.Clear();
            cachedIds.Clear();
        }

        private void Populate(GameObject prefab, int key)
        {
            var go = Populate(prefab, Vector3.zero, Quaternion.identity, parent);
            go.SetActive(false);
            cachedIds.Add(go.GetInstanceID(), key);

            cachedObjects[key].Push(go);
        }

        private GameObject Populate(GameObject prefab, Vector3 position = default(Vector3),
            Quaternion rotation = default(Quaternion),
            Transform parent = null, bool isPosLocal = false)
        {
            if (parent == null)
            {
                parent = this.parent;
            }


            var go = Object.Instantiate(prefab, position, rotation, parent).transform;
            // go.name += "_" + index; 
            if (isPosLocal) go.localPosition = position;
            else go.position = position;

            return go.gameObject;
        }
 
    }
}