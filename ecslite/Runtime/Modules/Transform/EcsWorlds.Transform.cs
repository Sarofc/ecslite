using System.Runtime.CompilerServices;
using Saro.Entities.Collections;
using Saro.Entities.Transforms;

namespace Saro.Entities
{
    public partial class EcsWorld
    {
        internal EcsPoolUnmanaged<Position> PositionPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_PositionPool ??= GetPoolUnmanaged<Position>();
        }
        internal EcsPoolUnmanaged<Rotation> RotationPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_RotationPool ??= GetPoolUnmanaged<Rotation>();
        }
        internal EcsPoolUnmanaged<Scale> ScalePool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_ScalePool ??= GetPoolUnmanaged<Scale>();
        }
        internal EcsPoolUnmanaged<Parent> ParentPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_ParentPool ??= GetPoolUnmanaged<Parent>();
        }
        internal EcsPoolUnmanaged<Children> ChildrenPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_ChildrenPool ??= GetPoolUnmanaged<Children>();
        }
        internal EcsPoolUnmanaged<IntrusiveListNode> NodePool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_NodePool ??= GetPoolUnmanaged<IntrusiveListNode>();
        }

        private EcsPoolUnmanaged<Position> m_PositionPool;
        private EcsPoolUnmanaged<Rotation> m_RotationPool;
        private EcsPoolUnmanaged<Scale> m_ScalePool;

        private EcsPoolUnmanaged<Parent> m_ParentPool;
        private EcsPoolUnmanaged<Children> m_ChildrenPool;
        private EcsPoolUnmanaged<IntrusiveListNode> m_NodePool;
    }
}