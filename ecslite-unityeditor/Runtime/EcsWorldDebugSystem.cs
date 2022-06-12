// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Saro.Diagnostics;
using Saro.Entities.Extension;
using Saro.Entities.Transforms;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Saro.Entities.UnityEditor
{
    public sealed class EcsWorldDebugSystem : IEcsPreInitSystem, IEcsRunSystem, IEcsWorldEventListener
    {
        public bool Enable { get; set; } = true;

        private readonly GameObject m_RootGo;
        private readonly Transform m_EntitiesRoot;
        private readonly bool m_BakeComponentsInName;
        private readonly string m_EntityNameFormat;
        private readonly EcsWorld m_World;
        private EcsEntityDebugView[] m_Entities;
        private Dictionary<int, byte> m_DirtyEntities;
        private Type[] m_TypesCache;

        public EcsWorldDebugSystem(EcsWorld world, bool bakeComponentsInName = true, string entityNameFormat = Name.k_EntityNameFormat)
        {
            m_World = world;
            m_BakeComponentsInName = bakeComponentsInName;
            m_EntityNameFormat = entityNameFormat;
            var rootGoName = string.IsNullOrEmpty(m_World.worldName) ? "[ECS-WORLD]" : $"[ECS-WORLD] {m_World.worldName}";
            m_RootGo = new GameObject(rootGoName);
            Object.DontDestroyOnLoad(m_RootGo);
            m_RootGo.hideFlags = HideFlags.NotEditable;
            m_EntitiesRoot = new GameObject("Entities").transform;
            m_EntitiesRoot.gameObject.hideFlags = HideFlags.NotEditable;
            m_EntitiesRoot.SetParent(m_RootGo.transform, false);
        }

        void IEcsPreInitSystem.PreInit(EcsSystems _)
        {
            // world
            if (m_World == null)
            {
                throw new Exception("Cant find required world.");
            }
            var worldDebugView = m_RootGo.AddComponent<EcsWorldDebugView>();
            worldDebugView.ecsWorld = m_World;
            worldDebugView.debugSystem = this;

            // systems
            // var systemRootGo = new GameObject("Systems");
            // systemRootGo.transform.parent = m_RootGo.transform;
            // systemRootGo.hideFlags = HideFlags.NotEditable;
            var view = m_RootGo.AddComponent<EcsSystemsDebugView>();
            view.hideFlags = HideFlags.DontSave;
            view.ecsSystemsList = m_World.ecsSystemsList;
            view.debugSystem = this;

            // entities
            m_EntitiesRoot.transform.hierarchyCapacity = 20480;
            m_Entities = new EcsEntityDebugView[m_World.GetWorldSize()];
            m_DirtyEntities = new Dictionary<int, byte>(m_Entities.Length);
            m_World.AddEventListener(this);
            var entities = Array.Empty<int>();
            var entitiesCount = m_World.GetAllEntities(ref entities);
            for (var i = 0; i < entitiesCount; i++)
            {
                ((IEcsWorldEventListener)this).OnEntityCreated(entities[i]);
            }
        }

        void IEcsRunSystem.Run(EcsSystems _)
        {
            GProfiler.BeginSample("[Ecs] EcsWorldDebugSystem");

            foreach (var pair in m_DirtyEntities)
            {
                var entity = pair.Key;

                ProcessParent(entity);

                // process entity name
                var entityName = Name.GetEntityName(entity, m_World, m_EntityNameFormat);

                if (m_World.GetEntityGen(entity) > 0)
                {
                    var count = m_World.GetComponentTypes(entity, ref m_TypesCache);
                    for (var i = 0; i < count; i++)
                    {
                        entityName = $"{entityName}:{EditorExtensions.GetCleanGenericTypeName(m_TypesCache[i])}";
                    }
                    entityName += ":";
                }
                m_Entities[entity].name = entityName;
            }
            m_DirtyEntities.Clear();

            GProfiler.EndSample();
        }

        void IEcsWorldEventListener.OnEntityCreated(int entity)
        {
            if (!m_Entities[entity])
            {
                var go = new GameObject();
                go.transform.SetParent(m_EntitiesRoot, false);
                var entityObserver = go.AddComponent<EcsEntityDebugView>();
                entityObserver.entity = entity;
                entityObserver.world = m_World;
                entityObserver.debugSystem = this;
                m_Entities[entity] = entityObserver;
                if (m_BakeComponentsInName)
                {
                    m_DirtyEntities[entity] = 1;
                }
                else
                {
                    go.name = entity.ToString(m_EntityNameFormat);
                }
            }
            m_Entities[entity].gameObject.SetActive(true);
        }

        void IEcsWorldEventListener.OnEntityDestroyed(int entity)
        {
            if (m_Entities[entity])
            {
                m_Entities[entity].gameObject.SetActive(false);
            }
        }

        void IEcsWorldEventListener.OnEntityChanged(int entity)
        {
            if (m_BakeComponentsInName)
            {
                m_DirtyEntities[entity] = 1;
            }
        }

        void IEcsWorldEventListener.OnFilterCreated(EcsFilter filter)
        {
        }

        void IEcsWorldEventListener.OnWorldResized(int newSize)
        {
            Array.Resize(ref m_Entities, newSize);
        }

        void IEcsWorldEventListener.OnWorldDestroyed(EcsWorld world)
        {
            m_World.RemoveEventListener(this);
            Object.Destroy(m_RootGo);
        }

        public EcsEntityDebugView GetEntityView(int entity)
        {
            return entity >= 0 && entity < m_Entities.Length ? m_Entities[entity] : null;
        }

        void ProcessParent(int entity)
        {
            var entityView = m_Entities[entity];
            if (entityView != null)
            {
                var entityTransform = entityView.gameObject.transform;
                if (m_World.IsEntityAlive(entity) && m_World.ParentPool.Has(entity))
                {
                    var eParent = m_World.ParentPool.Get(entity).entity;
                    if (eParent.IsAlive())
                    {
                        var parentTransform = m_Entities[eParent.id].gameObject.transform;
                        entityTransform.SetParent(parentTransform);
                    }
                }
                else
                {
                    entityTransform.SetParent(m_EntitiesRoot);
                }
            }
        }
    }
}