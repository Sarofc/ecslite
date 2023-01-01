using System;

namespace Saro.Entities.Transforms
{
    internal struct Parent : IEcsUnmanagedComponent<Parent>
    {
        public EcsEntity entity;
    }

    internal struct Children : IEcsUnmanagedComponent<Children>
    {
        public Collections.IntrusiveList items;
    }
}