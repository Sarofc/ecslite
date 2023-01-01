#if FIXED_POINT_MATH
using ME.ECS.Mathematics;
using Single = sfloat;
#else
using Unity.Mathematics;
using Single = System.Single;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saro.Entities.Tests
{
    internal abstract class MockSystem : IEcsRunSystem, IEcsInitSystem
    {
        public bool Enable { get; set; } = true;

        public EcsWorld world;

        void IEcsInitSystem.Init(EcsSystems systems)
        {
            world = systems.GetWorld();
        }

        void IEcsRunSystem.Run(EcsSystems systems, Single deltaTime)
        {
            RunInternal(deltaTime);
        }

        protected abstract void RunInternal(Single deltaTime);
    }
}
