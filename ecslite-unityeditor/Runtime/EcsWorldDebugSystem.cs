﻿// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

#if FIXED_POINT_MATH
using Saro.FPMath;
using Single = Saro.FPMath.sfloat;
#else
using Unity.Mathematics;
using Single = System.Single;
#endif

using System;
using System.Collections.Generic;
using Saro.Diagnostics;
using UnityEngine;
using UObject = UnityEngine.Object;

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
        private Type[] m_TypeCaches;

        public EcsWorldDebugSystem(EcsWorld world, bool bakeComponentsInName = true, string entityNameFormat = EntityNameUtility.k_EntityNameFormat)
        {
            m_World = world;
            m_BakeComponentsInName = bakeComponentsInName;
            m_EntityNameFormat = entityNameFormat;
            var rootGoName = string.IsNullOrEmpty(m_World.worldName) ? "[ECS-WORLD]" : $"[ECS-WORLD] {m_World.worldName}";
            m_RootGo = new GameObject(rootGoName);
            UObject.DontDestroyOnLoad(m_RootGo);
            m_RootGo.hideFlags = HideFlags.NotEditable;
            m_EntitiesRoot = m_RootGo.transform;
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

        void IEcsRunSystem.Run(EcsSystems _, Single deltaTime)
        {
            GProfiler.BeginSample("[Ecs] EcsWorldDebugSystem");

            foreach (var pair in m_DirtyEntities)
            {
                var entity = pair.Key;

                ProcessParent(entity);

                // process entity name
                var entityName = entity.GetEntityName(m_World);

                if (m_World.GetEntityGen(entity) > 0)
                {
                    var count = m_World.GetComponentTypes(entity, ref m_TypeCaches);
                    for (var i = 0; i < count; i++)
                    {
                        entityName = $"{entityName}:{EditorExtensions.GetCleanGenericTypeName(m_TypeCaches[i])}";
                    }
                    entityName += ":";
                }
                m_Entities[entity].name = entityName;
            }
            m_DirtyEntities.Clear();

            // debug transform 
            for (int i = 0; i < m_Entities.Length; i++)
            {
                var entityView = m_Entities[i];
                if (entityView == null) continue;

                var entity = m_Entities[i].entity;
                if (!m_World.IsEntityAlive(entity.id)) continue;

                // sync position
                if (m_World.PositionPool.Has(entity.id))
                    m_Entities[entity.id].transform.localPosition = (Vector3)m_World.PositionPool.Get(entity.id).value;
                else
                    m_Entities[entity.id].transform.localPosition = Vector3.zero;

                // sync rotation
                if (m_World.RotationPool.Has(entity.id))
                {
                    var localRot = m_World.RotationPool.Get(entity.id).value;
                    //if (math.any(math.isnan(localRot.value)))
                    //{
                    //    Log.ERROR($"rotation is NaN? {localRot}");
                    //    localRot = quaternion.identity;
                    //}
                    m_Entities[entity.id].transform.localRotation = (Quaternion)localRot;
                    // TODO // transform.localRotation assign attempt for '00000013:SphereShape:Aabb:BodyMask:ProjectileObj:LifeSpan:Position:Rotation:Parent:Children:CustomMovementType:' is not valid. Input rotation is { NaN, NaN, NaN, NaN }.
                }
                else
                    m_Entities[entity.id].transform.localRotation = Quaternion.identity;

                // sync scale
                if (m_World.ScalePool.Has(entity.id))
                    m_Entities[entity.id].transform.localScale = (Vector3)m_World.ScalePool.Get(entity.id).value;
                else
                    m_Entities[entity.id].transform.localScale = Vector3.one;

                m_Entities[entity.id].transform.hasChanged = false; // TODO 这里修改了hasChanged，看看有没有其他影响？
            }

            GProfiler.EndSample();
        }

        void IEcsWorldEventListener.OnEntityCreated(int entity)
        {
            if (!m_Entities[entity])
            {
                var go = new GameObject();
                go.transform.SetParent(m_EntitiesRoot, false);
                var debugView = go.AddComponent<EcsEntityDebugView>();
                //debugView.entity = m_World.Pack(entity);
                debugView.entityId = entity;
                debugView.worldId = m_World.worldId;
                debugView.debugSystem = this;
                m_Entities[entity] = debugView;
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
            UObject.Destroy(m_RootGo);
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