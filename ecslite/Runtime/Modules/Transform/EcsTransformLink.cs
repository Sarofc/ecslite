using System;

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
}