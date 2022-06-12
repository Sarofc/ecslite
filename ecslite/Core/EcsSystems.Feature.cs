using System;
using System.Collections.Generic;

namespace Saro.Entities
{
#if ENABLE_IL2CPP
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
    [Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
#endif
    public class EcsSystemFeature : IEcsPreInitSystem, IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem, IEcsPostDestroySystem
    {
        public bool Enable { get; set; } = true;

        public IReadOnlyList<IEcsSystem> Systems => m_Systems;

        private readonly List<IEcsSystem> m_Systems;
        private readonly List<IEcsRunSystem> m_RunSystems;

        public EcsSystemFeature()
        {
            m_Systems = new List<IEcsSystem>();
            m_RunSystems = new List<IEcsRunSystem>();
        }

        public void AddSystem(IEcsSystem system)
        {
            m_Systems.Add(system);
            if (system is IEcsRunSystem _system)
            {
                m_RunSystems.Add(_system);
            }
        }

        public void RemoveSystem(IEcsSystem system)
        {
            m_Systems.Remove(system);
            if (system is IEcsRunSystem _system)
            {
                m_RunSystems.Remove(_system);
            }
        }

        public void PreInit(EcsSystems systems)
        {
            for (int i = 0; i < m_Systems.Count; i++)
            {
                var system = m_Systems[i];
                if (system is IEcsPreInitSystem _system)
                {
                    _system.PreInit(systems);
                }
            }
        }

        public void Init(EcsSystems systems)
        {
            for (int i = 0; i < m_Systems.Count; i++)
            {
                var system = m_Systems[i];
                if (system is IEcsInitSystem _system)
                {
                    _system.Init(systems);
                }
            }
        }

        public void Run(EcsSystems systems)
        {
            if (Enable)
            {
                for (int i = 0; i < m_RunSystems.Count; i++)
                {
                    var system = m_RunSystems[i];
                    system.Run(systems);
                }
            }
        }

        public void Destroy(EcsSystems systems)
        {
            for (int i = 0; i < m_Systems.Count; i++)
            {
                var system = m_Systems[i];
                if (system is IEcsDestroySystem _system)
                {
                    _system.Destroy(systems);
                }
            }
        }

        public void PostDestroy(EcsSystems systems)
        {
            for (int i = 0; i < m_Systems.Count; i++)
            {
                var system = m_Systems[i];
                if (system is IEcsPostDestroySystem _system)
                {
                    _system.PostDestroy(systems);
                }
            }
        }
    }
}
