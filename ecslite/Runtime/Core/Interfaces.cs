using System;

namespace Saro.Entities
{
    /// <summary>
    /// Ecs托管组件必须 *继承* 此接口
    /// </summary>
    public interface IEcsComponent { }

    public interface IEcsComponentSingleton : IEcsComponent { }

    /// <summary>
    /// 第一次add/每次del时，都会调用，用于初始化/清理数据，仅限managed组件使用
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEcsAutoReset<T> where T : class
    {
        void AutoReset(ref T c);
    }

    public partial interface IEcsPool
    {
        /// <summary>
        /// 组件是否是单例
        /// </summary>
        bool IsSingleton { get; }
        void Resize(int capacity);
        bool Has(int entity);
        void Del(int entity);
        void AddRaw(int entity, object dataRaw);
        object GetRaw(int entity);
        void SetRaw(int entity, object dataRaw);
        int GetId();
        Type GetComponentType();
    }
}