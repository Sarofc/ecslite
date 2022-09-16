namespace Saro.Entities.Transforms
{
    public struct Parent : IEcsComponent
    {
        public EcsPackedEntityWithWorld entity;
    }

    public struct Children : IEcsComponent
    {
        public Collections.IntrusiveList items;
    }
}