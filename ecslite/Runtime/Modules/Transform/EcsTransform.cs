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
    public struct Position : IEcsComponent
    {
        public float3 value;

        public Position(float3 value)
        {
            this.value = value;
        }
    }

    [Serializable]
    public struct Rotation : IEcsComponent
    {
        public quaternion value;

        public Rotation(quaternion value)
        {
            this.value = value;
        }
    }

    [Serializable]
    public struct Scale : IEcsComponent
    {
        public float3 value;

        public Scale(float3 value)
        {
            this.value = value;
        }
    }
}