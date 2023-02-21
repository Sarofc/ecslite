#if FIXED_POINT_MATH
using Saro.FPMath;
using Single = Saro.FPMath.sfloat;
#else
using Unity.Mathematics;
using Single = System.Single;
#endif

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

        public IEcsSystem AddSystem(IEcsSystem system)
        {
            m_Systems.Add(system);
            if (system is IEcsRunSystem _system)
            {
                m_RunSystems.Add(_system);
            }
            return this;
        }

        public void RemoveSystem(IEcsSystem system)
        {
            m_Systems.Remove(system);
            if (system is IEcsRunSystem _system)
            {
                m_RunSystems.Remove(_system);
            }
        }

        public virtual void PreInit(EcsSystems systems)
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

        public virtual void Init(EcsSystems systems)
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

        public virtual void Run(EcsSystems systems, Single deltaTime)
        {
            for (int i = 0; i < m_RunSystems.Count; i++)
            {
                var system = m_RunSystems[i];
                system.Run(systems, deltaTime);
            }
        }

        public virtual void Destroy(EcsSystems systems)
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

        public virtual void PostDestroy(EcsSystems systems)
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
