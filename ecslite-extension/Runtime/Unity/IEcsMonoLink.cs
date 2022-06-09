﻿namespace Saro.Entities.Extension
{
    public interface IEcsMonoLink
    {
        ref EcsPackedEntityWithWorld Entity { get; }
        bool IsAlive { get; }
        void Link(in EcsPackedEntityWithWorld ent);
    }
}
