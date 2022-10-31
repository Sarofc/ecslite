using FLOAT3 = UnityEngine.Vector3;
using QUATERNION = UnityEngine.Quaternion;

using System;

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