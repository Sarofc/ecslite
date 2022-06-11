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
            // TODO ����
            //  1. DelEntity �ڵ�ʱ���Ƿ���ȷɾ��
            // 2. world.OnDestroy ʱ���Ƿ���ȷɾ��
            // 3. ���� DelEntity �ĵط���Ҫע�⣡
            // 4. �㼶��entity��������ӵ��Children�����ʱ�򣬵���DelEntity�������޷��ݹ�ɾ���ӽڵ�

            // ��children�����entity����Ҫ�ݹ���ã���֤�㼶��ȷ
            if (ChildrenPool.Has(entity))
            {
                EcsTransformUtility.OnEntityDestroy(this.Pack(entity));
            }

            DelEntity_Internal(entity);
        }
    }
}