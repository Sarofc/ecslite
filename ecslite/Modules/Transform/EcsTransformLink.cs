using System;
using Saro.Entities.Authoring;
using Saro.Entities.Serialization;

namespace Saro.Entities.Transforms
{
    internal struct Parent : IEcsComponent
    {
        public EcsEntity entity;
    }

    internal struct Children : IEcsComponent
    {
        public Collections.IntrusiveList items;
    }

    [Serializable]
    public struct ChildrenForAuthoring : IEcsComponentAuthoring, IEcsComponentPostInit, IEcsComponentNotAdd
    {
        public EcsBlueprint[] children;

        readonly void IEcsComponentPostInit.PostInitialize(EcsWorld world, int entity)
        {
            // TODO 怎么方便使用

            if (children == null || children.Length == 0) return;

            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                var eChild = EcsBlueprint.Instantiate(child, world);
                EcsTransformUtility.SetParent(eChild.id, entity, world);
            }
        }
    }
}