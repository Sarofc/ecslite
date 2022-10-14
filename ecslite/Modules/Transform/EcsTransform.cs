using FLOAT3 = UnityEngine.Vector3;
using QUATERNION = UnityEngine.Quaternion;

using System;
using Saro.Entities.Authoring;

namespace Saro.Entities.Transforms
{
    [Serializable]
    public struct Position : IEcsComponentAuthoring
    {
        public FLOAT3 value;

        public Position(FLOAT3 value)
        {
            this.value = value;
        }
    }

    [Serializable]
    public struct Rotation : IEcsComponentAuthoring
    {
        public QUATERNION value;

        public Rotation(QUATERNION value)
        {
            this.value = value;
        }
    }

    [Serializable]
    public struct Scale : IEcsComponentAuthoring
    {
        public FLOAT3 value;

        public Scale(FLOAT3 value)
        {
            this.value = value;
        }
    }
}