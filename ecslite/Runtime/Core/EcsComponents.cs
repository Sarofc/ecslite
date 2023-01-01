// ----------------------------------------------------------------------------
// The Proprietary or MIT-Red License
// Copyright (c) 2012-2022 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System;

namespace Saro.Entities
{
    // TODO 数据不可变的资源，避免struct copy，类似unity blobasset

    public partial interface IEcsPool
    {
        bool Singleton { get; }
        void Resize(int capacity);
        bool Has(int entity);
        void Del(int entity);
        void AddRaw(int entity, object dataRaw);
        object GetRaw(int entity);
        void SetRaw(int entity, object dataRaw);
        int GetId();
        Type GetComponentType();
    }

    /// <summary>
    /// 重置数据，取代 struct = default 操作
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEcsAutoReset<T> where T : class
    {
        void AutoReset(ref T c);
    }
}