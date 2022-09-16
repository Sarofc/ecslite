using System.Runtime.CompilerServices;
using Saro.Entities.Transforms;

namespace Saro.Entities
{
    public partial class EcsWorld
    {
        internal EcsPool<Destroy> DestroyPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_DestroyPool ??= GetPool<Destroy>();
        }

        private EcsPool<Destroy> m_DestroyPool;

        /// <summary>
        /// entity 标记销毁
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool IsEntityMarkDestroy(int entity)
        {
            return !DestroyPool.Has(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DelEntity(int entity)
        {
            // TODO 测试
            // DelEntity 实际还没销毁 entity，而是加一个组件标记

            // 解除层级结构
            if (ChildrenPool.Has(entity))
            {
                EcsTransformUtility.OnEntityDestroy(this.Pack(entity));
            }

            // 添加标记组件
            DestroyPool.Add(entity);
            //DelEntity_Internal(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DelEntity(in EcsEntity entity) => DelEntity(entity.id);
    }
}