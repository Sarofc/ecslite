#if FIXED_POINT_MATH
using Saro.FPMath;
using Single = Saro.FPMath.sfloat;
#else
using Unity.Mathematics;
using Single = System.Single;
#endif

using System;

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

        public override string ToString()
        {
            return value.ToString();
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

        public override string ToString()
        {
            return value.ToString();
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

        public override string ToString()
        {
            return value.ToString();
        }
    }
}