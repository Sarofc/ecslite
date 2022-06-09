#if FIXED_POINT_MATH
using FLOAT = ME.ECS.fp;
using FLOAT2 = ME.ECS.fp2;
using FLOAT3 = ME.ECS.fp3;
using FLOAT4 = ME.ECS.fp4;
using QUATERNION = ME.ECS.fpquaternion;
#else
using System;
using FLOAT = System.Single;
using FLOAT2 = UnityEngine.Vector2;
using FLOAT3 = UnityEngine.Vector3;
using FLOAT4 = UnityEngine.Vector4;
using QUATERNION = UnityEngine.Quaternion;
#endif

namespace Saro.Entities.Transforms
{
    [Serializable]
    public struct Position : IEcsComponent
    {
        public FLOAT3 value;

        public Position(FLOAT3 value)
        {
            this.value = value;
        }
    }

    [Serializable]
    public struct Rotation : IEcsComponent
    {
        public QUATERNION value;

        public Rotation(QUATERNION value)
        {
            this.value = value;
        }
    }

    [Serializable]
    public struct Scale : IEcsComponent
    {
        public FLOAT3 value;

        public Scale(FLOAT3 value)
        {
            this.value = value;
        }
    }
}