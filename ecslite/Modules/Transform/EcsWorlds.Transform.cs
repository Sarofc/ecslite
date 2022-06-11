using System.Runtime.CompilerServices;
using Saro.Entities.Collections;
using Saro.Entities.Transforms;

namespace Saro.Entities
{
    public partial class EcsWorld
    {
        internal EcsPool<Position> PositionPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_PositionPool ??= GetPool<Position>();
        }
        internal EcsPool<Rotation> RotationPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_RotationPool ??= GetPool<Rotation>();
        }
        internal EcsPool<Scale> ScalePool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_ScalePool ??= GetPool<Scale>();
        }
        internal EcsPool<Parent> ParentPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_ParentPool ??= GetPool<Parent>();
        }
        internal EcsPool<Children> ChildrenPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_ChildrenPool ??= GetPool<Children>();
        }
        internal EcsPool<IntrusiveListNode> NodePool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_NodePool ??= GetPool<IntrusiveListNode>();
        }

        private EcsPool<Position> m_PositionPool;
        private EcsPool<Rotation> m_RotationPool;
        private EcsPool<Scale> m_ScalePool;

        private EcsPool<Parent> m_ParentPool;
        private EcsPool<Children> m_ChildrenPool;
        private EcsPool<IntrusiveListNode> m_NodePool;

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

            DelEntity_Internal(entity);
        }
    }
}