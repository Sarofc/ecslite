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
        /// entity 是否将被销毁
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
            //  1. DelEntity 节点时，是否正确删除
            // 2. world.OnDestroy 时，是否正确删除
            // 3. 所有 DelEntity 的地方都要注意！
            // 4. 层级化entity，必须在拥有Children组件的时候，调用DelEntity，否则无法递归删除子节点

            // 有children组件的entity，需要递归调用，保证层级正确
            if (ChildrenPool.Has(entity))
            {
                EcsTransformUtility.OnEntityDestroy(this.Pack(entity));
            }

            // 只标记，真正删除是在 system 里
            DestroyPool.Add(entity);
            //DelEntity_Internal(entity);
        }
    }
}