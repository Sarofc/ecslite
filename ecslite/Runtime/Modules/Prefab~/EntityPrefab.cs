using System;
using System.Collections.Generic;
using Saro.Entities.Transforms;
using Saro.Utility;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Saro.Entities
{
    [Serializable]
    public partial class Prefab
    {
        public string id;
        public int uid;
        public Data data;

        [Serializable]
        public class Data
        {
            [SerializeReference]
            public IEcsComponent[] components = { };

            [HideInInspector]
            public Data[] children = { };
        }
    }

    partial class Prefab
    {
        static Dictionary<int, Prefab> s_Templates = new();

        public static void Load(List<Prefab> list)
        {
            s_Templates.Clear();

            foreach (var item in list)
            {
                s_Templates.Add(item.uid, item);
            }
        }

        public static EcsEntity Instantiate(EcsWorld world, string prefabName)
        {
            var id = Prefab.StringToHash(prefabName);
            return Prefab.Instantiate(world, id);
        }

        public static EcsEntity Instantiate(EcsWorld world, int prefabId)
        {
            if (s_Templates.TryGetValue(prefabId, out var prefab))
            {
                return world.Instantiate(prefab);
            }
            else
            {
                Log.ERROR($"{nameof(prefabId)}: {prefabId} not found");
                return default;
            }
        }

        public static EcsEntity Instantiate(EcsWorld world, Prefab prefab)
        {
            return world.Instantiate(prefab);
        }

        public static int StringToHash(string value)
        {
            return (int)HashUtility.GetCrc32(value);
        }
    }

    partial class EcsWorld
    {
        public EcsEntity Instantiate(Prefab prefab)
        {
            var entity = Instantiate_Internal(prefab.data, 0);
            return this.Pack(entity);
        }

        private int Instantiate_Internal(Prefab.Data data, int parent)
        {
            var entity = NewEntity();
            var components = data.components;
            ProcessTransform(entity, parent);
            AddComponents(entity, components);

            var children = data.children;
            for (int i = 0; i < children.Length; i++)
                Instantiate_Internal(children[i], entity);

            return entity;
        }

        private void ProcessTransform(int child, int parent)
        {
            if (parent != 0)
                EcsTransformUtility.SetParent(child, parent, this);
        }

        private void AddComponents(int entity, IEcsComponent[] components)
        {
            foreach (var comp in components)
            {
                var pool = GetPoolByType(comp.GetType());
                pool.AddRaw(entity, comp);
            }
        }
    }

    public class EntityPrefab : MonoBehaviour
    {
        public Prefab.Data data = new();

        public Prefab Bake()
        {
            var prefab = new Prefab();
            prefab.id = this.name;
            prefab.uid = Prefab.StringToHash(this.name);
            prefab.data = this.data;

            ProcessTransform(this.transform);

            return prefab;
        }

        //[Button]
        //public void Bake_unity()
        //{
        //    ProcessTransform(this.transform);
        //    json = JsonUtility.ToJson(this); // 可以序列化多态，看上去，性能要比 json.net 要强
        //    Log.ERROR("bake:\n" + json);
        //}

        private void ProcessTransform(Transform t)
        {
            var parentPrefab = t.GetComponent<EntityPrefab>();
            parentPrefab.data.children = new Prefab.Data[t.childCount];
            for (int i = 0; i < t.childCount; i++)
            {
                var child = t.GetChild(i);
                var childPrefab = child.GetComponent<EntityPrefab>();
                if (childPrefab == null)
                {
                    Log.ERROR($"EntityPrefab is null. index: {i}");
                    continue;
                }

                parentPrefab.data.children[i] = childPrefab.data;

                ProcessTransform(child);
            }
        }

        [Button]
        public static void BakeAll()
        {
            new EntityPrefab_JsonDataProvider().Save();
        }
    }
}
