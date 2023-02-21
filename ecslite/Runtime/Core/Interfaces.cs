using System;

namespace Saro.Entities
{
    /// <summary>
    /// Ecs组件必须 *继承* 此接口
    /// </summary>
    public partial interface IEcsComponent { }

    /// <summary>
    /// 单例组件
    /// </summary>
    public interface IEcsComponentSingleton : IEcsComponent { }

    internal delegate void EcsCleanupHandler<T>(ref T component);

    /// <summary>
    /// 第一次add/每次del时，都会调用，用于初始化/清理数据
    /// <code>对struct也有用，struct可能包含native资源</code>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEcsCleanup<T>
    {
        void Cleanup(ref T c);
    }

    public partial interface IEcsPool
    {
        /// <summary>
        /// 组件是否是单例
        /// </summary>
        bool IsSingleton { get; }
        /// <summary>
        /// 改变大小
        /// </summary>
        /// <param name="capacity"></param>
        void Resize(int capacity);
        /// <summary>
        /// <paramref name="entity"/> 是否挂载该组件
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool Has(int entity);
        /// <summary>
        /// 删除 <paramref name="entity"/> 上的该组件
        /// </summary>
        /// <param name="entity"></param>
        void Del(int entity);
        /// <summary>
        /// 给 <paramref name="entity"/> 添加该组件（裸数据，如果是struct，则会装箱）
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dataRaw"></param>
        void AddRaw(int entity, object dataRaw);
        /// <summary>
        /// 获取 <paramref name="entity"/> 上的该组件（裸数据，如果是struct，则会装箱）
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        object GetRaw(int entity);
        /// <summary>
        /// 给 <paramref name="entity"/> 设置该组件（裸数据，如果是struct，则会装箱）
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dataRaw"></param>
        void SetRaw(int entity, object dataRaw);
        /// <summary>
        /// 获取组件id
        /// </summary>
        /// <returns></returns>
        int GetComponentId();
        /// <summary>
        /// 获取组件类型
        /// </summary>
        /// <returns></returns>
        Type GetComponentType();
    }
}