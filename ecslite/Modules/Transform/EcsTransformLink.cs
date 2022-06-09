using System;
using Saro.Entities.Authoring;

namespace Saro.Entities.Transforms
{
    internal struct Parent : IEcsComponent
    {
        public EcsPackedEntityWithWorld entity;
    }

    internal struct Children : IEcsComponent
    {
        public Collections.IntrusiveList items;
    }

    [Serializable]
    public struct ChildrenForAuthoring : IEcsComponentAuthoring, IEcsComponentPostInit, IEcsComponentNotAdd
    {
        public GenericEntityAuthoring[] children;

        void IEcsComponentPostInit.PostInitialize(EcsWorld world, int entity)
        {
            // TODO ºÏ≤‚—≠ª∑“˝”√

            if (children == null || children.Length == 0) return;

            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];

                var eChild = ((IEcsConvertToEntity)child).ConvertToEntity(world);

                EcsTransformUtility.SetParent(eChild, entity, world);
            }
        }
    }
}