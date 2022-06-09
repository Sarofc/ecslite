using UnityEngine;

namespace Saro.Entities.Authoring
{
    [System.Obsolete("use GenericEntityAuthoring instead", true)]
    public abstract class SOEntityAuthoring : ScriptableObject, IEcsConvertToEntity
    {
        public abstract int ConvertToEntity(EcsWorld world);
    }
}