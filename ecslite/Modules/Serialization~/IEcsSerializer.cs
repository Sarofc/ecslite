
//namespace Saro.Entities.Serialization
//{
//    /// <summary>
//    /// 此组件不会被添加！<see cref="EcsEntityInitializer"/>
//    /// </summary>
//    public interface IEcsComponentNotAdd
//    { }

//    /// <summary>
//    /// 添加全部组件后初始化！建议单个组件不要依赖过多组件！<see cref="EcsSerializer.Initialize{T}(int, System.Collections.Generic.IList{T}, EcsWorld, bool)"/>
//    /// <code>只有序列化、反序列化才会被调用！</code>
//    /// <code>TODO 思考合适的方式，来简化创建Entity</code>
//    /// </summary>
//    public interface IEcsComponentPostInit
//    {
//        void PostInitialize(EcsWorld world, int entity) { }

//        /// <summary>
//        /// 在postinit后，调用
//        /// </summary>
//        void AfterPostInit(EcsWorld world, int entity) { }
//    }

//    public interface IEcsSerializer<T>
//    {
//        T Serialize(int entity, EcsWorld world);

//        int Deserialize(T data, EcsWorld world);
//    }
//}
