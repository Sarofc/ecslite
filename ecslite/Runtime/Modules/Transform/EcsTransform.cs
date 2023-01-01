using System;

#if FIXED_POINT_MATH
using ME.ECS.Mathematics;
using Single = sfloat;
#else
using Unity.Mathematics;
using Single = System.Single;
#endif

namespace Saro.Entities.Transforms
{
    [Serializable]
    public struct Position : IEcsUnmanagedComponent<Position>
    {
        public float3 value;

        public Position(float3 value)
        {
            this.value = value;
        }
    }

    [Serializable]
    public struct Rotation : IEcsUnmanagedComponent<Rotation>
    {
        public quaternion value;

        public Rotation(quaternion value)
        {
            this.value = value;
        }
    }

    [Serializable]
    public struct Scale : IEcsUnmanagedComponent<Scale>
    {
        public float3 value;

        public Scale(float3 value)
        {
            this.value = value;
        }
    }
}