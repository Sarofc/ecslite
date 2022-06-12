// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Saro.Entities
{
    public interface IEcsSystem { }

    public interface IEcsPreInitSystem : IEcsSystem
    {
        void PreInit(EcsSystems systems);
    }

    public interface IEcsInitSystem : IEcsSystem
    {
        void Init(EcsSystems systems);
    }

    public interface IEcsRunSystem : IEcsSystem
    {
        bool Enable { get; set; }

        void Run(EcsSystems systems);
    }

    public interface IEcsDestroySystem : IEcsSystem
    {
        void Destroy(EcsSystems systems);
    }

    public interface IEcsPostDestroySystem : IEcsSystem
    {
        void PostDestroy(EcsSystems systems);
    }

#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
#endif
    public class EcsSystems
    {
        public string SystemsName { get; private set; }
        private readonly EcsWorld m_DefaultWorld;
        private readonly Dictionary<string, EcsWorld> m_Worlds;
        private readonly List<IEcsSystem> m_AllSystems;
        private readonly object m_Shared;
        private IEcsRunSystem[] m_RunSystems;
        private int m_RunSystemsCount;

        public EcsSystems(string systemsName, EcsWorld defaultWorld, object shared = null)
        {
            SystemsName = systemsName;
            m_DefaultWorld = defaultWorld;
            m_Shared = shared;
            m_Worlds = new Dictionary<string, EcsWorld>(32);
            m_AllSystems = new List<IEcsSystem>(128);

#if DEBUG || LEOECSLITE_WORLD_EVENTS
            defaultWorld.ecsSystemsList.Add(this);
#endif
        }

        public EcsSystems(EcsWorld defaultWorld, object shared = null) : this(null, defaultWorld, shared)
        { }

        public Dictionary<string, EcsWorld> GetAllNamedWorlds() => m_Worlds;

        public T GetSystem<T>() where T : class, IEcsSystem
        {
            var itemsCount = m_AllSystems.Count;
            if (itemsCount == 0)
                return null;

            var type = typeof(T);

            for (int i = 0, iMax = itemsCount; i < iMax; i++)
            {
                var system = m_AllSystems[i];

                if (type.IsAssignableFrom(system.GetType()))
                    return system as T;
            }
            return null;
        }

        public int GetAllSystems(ref IEcsSystem[] list)
        {
            var itemsCount = m_AllSystems.Count;
            if (itemsCount == 0) { return 0; }
            if (list == null || list.Length < itemsCount)
            {
                list = new IEcsSystem[m_AllSystems.Capacity];
            }
            for (int i = 0, iMax = itemsCount; i < iMax; i++)
            {
                list[i] = m_AllSystems[i];
            }
            return itemsCount;
        }

        public int GetRunSystems(ref IEcsRunSystem[] list)
        {
            var itemsCount = m_RunSystemsCount;
            if (itemsCount == 0) { return 0; }
            if (list == null || list.Length < itemsCount)
            {
                list = new IEcsRunSystem[m_RunSystems.Length];
            }
            for (int i = 0, iMax = itemsCount; i < iMax; i++)
            {
                list[i] = m_RunSystems[i];
            }
            return itemsCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetShared<T>() where T : class => m_Shared as T;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EcsWorld GetWorld(string name = null)
        {
            if (name == null)
            {
                return m_DefaultWorld;
            }
            m_Worlds.TryGetValue(name, out var world);
            return world;
        }

        public void Destroy()
        {
            for (var i = m_AllSystems.Count - 1; i >= 0; i--)
            {
                if (m_AllSystems[i] is IEcsDestroySystem destroySystem)
                {
                    destroySystem.Destroy(this);
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                    var worldName = CheckForLeakedEntities();
                    if (worldName != null) { throw new System.Exception($"Empty entity detected in world \"{worldName}\" after {destroySystem.GetType().Name}.Destroy()."); }
#endif
                }
            }
            for (var i = m_AllSystems.Count - 1; i >= 0; i--)
            {
                if (m_AllSystems[i] is IEcsPostDestroySystem postDestroySystem)
                {
                    postDestroySystem.PostDestroy(this);
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                    var worldName = CheckForLeakedEntities();
                    if (worldName != null) { throw new System.Exception($"Empty entity detected in world \"{worldName}\" after {postDestroySystem.GetType().Name}.PostDestroy()."); }
#endif
                }
            }
            m_AllSystems.Clear();
            m_RunSystems = null;
        }

        public EcsSystems AddWorld(EcsWorld world, string name)
        {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (string.IsNullOrEmpty(name)) { throw new System.Exception("World name cant be null or empty."); }
#endif
            m_Worlds[name] = world;
            return this;
        }

        public EcsSystems Add(IEcsSystem system)
        {
            m_AllSystems.Add(system);
            if (system is IEcsRunSystem)
            {
                m_RunSystemsCount++;
            }
            return this;
        }

        public void Init()
        {
            if (m_RunSystemsCount > 0)
            {
                m_RunSystems = new IEcsRunSystem[m_RunSystemsCount];
            }
            foreach (var system in m_AllSystems)
            {
                if (system is IEcsPreInitSystem initSystem)
                {
                    initSystem.PreInit(this);
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                    var worldName = CheckForLeakedEntities();
                    if (worldName != null) { throw new System.Exception($"Empty entity detected in world \"{worldName}\" after {initSystem.GetType().Name}.PreInit()."); }
#endif
                }
            }
            var runIdx = 0;
            foreach (var system in m_AllSystems)
            {
                if (system is IEcsInitSystem initSystem)
                {
                    initSystem.Init(this);
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                    var worldName = CheckForLeakedEntities();
                    if (worldName != null) { throw new System.Exception($"Empty entity detected in world \"{worldName}\" after {initSystem.GetType().Name}.Init()."); }
#endif
                }
                if (system is IEcsRunSystem runSystem)
                {
                    m_RunSystems[runIdx++] = runSystem;
                }
            }
        }

        public void Run()
        {
            for (int i = 0, iMax = m_RunSystemsCount; i < iMax; i++)
            {
                m_RunSystems[i].Run(this);
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
                var worldName = CheckForLeakedEntities();
                if (worldName != null) { throw new System.Exception($"Empty entity detected in world \"{worldName}\" after {m_RunSystems[i].GetType().Name}.Run()."); }
#endif
            }
        }

#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
        public string CheckForLeakedEntities()
        {
            if (m_DefaultWorld.CheckForLeakedEntities()) return m_DefaultWorld.worldID.ToString();
            foreach (var pair in m_Worlds)
            {
                if (pair.Value.CheckForLeakedEntities())
                {
                    return pair.Value.worldID.ToString();
                }
            }
            return null;
        }
#endif
    }
}