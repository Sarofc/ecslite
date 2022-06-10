
namespace Saro.Entities.Serialization
{
    /// <summary>
    /// 此组件不会被添加！<see cref="EcsEntityInitializer"/>
    /// </summary>
    public interface IEcsComponentNotAdd
    { }

    /// <summary>
    /// 添加全部组件后初始化！建议单个组件不要依赖过多组件！<see cref="EcsEntityInitializer"/>
    /// </summary>
    public interface IEcsComponentPostInit
    {
        void PostInitialize(EcsWorld world, int entity);
    }

    public interface IEcsSerializer<T>
    {
        T Serialize(int entity, EcsWorld world);

        int Deserialize(T data, EcsWorld world);
    }
}
