namespace Saro.Entities.Extension
{
    public interface IEcsMonoLink
    {
        ref EcsEntity Entity { get; }
        bool IsAlive { get; }
        void Link(EcsEntity ent);
    }
}
