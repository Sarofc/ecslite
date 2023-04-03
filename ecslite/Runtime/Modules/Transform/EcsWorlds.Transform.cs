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
            get => m_PositionPool ??= GetOrAddPoolUnmanaged<Position>();
        }
        internal EcsPoolUnmanaged<Rotation> RotationPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_RotationPool ??= GetOrAddPoolUnmanaged<Rotation>();
        }
        internal EcsPoolUnmanaged<Scale> ScalePool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_ScalePool ??= GetOrAddPoolUnmanaged<Scale>();
        }
        internal EcsPoolUnmanaged<Parent> ParentPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_ParentPool ??= GetOrAddPoolUnmanaged<Parent>();
        }
        internal EcsPoolUnmanaged<Children> ChildrenPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_ChildrenPool ??= GetOrAddPoolUnmanaged<Children>();
        }
        internal EcsPoolUnmanaged<IntrusiveListNode> NodePool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_NodePool ??= GetOrAddPoolUnmanaged<IntrusiveListNode>();
        }

        private EcsPoolUnmanaged<Position> m_PositionPool;
        private EcsPoolUnmanaged<Rotation> m_RotationPool;
        private EcsPoolUnmanaged<Scale> m_ScalePool;

        private EcsPoolUnmanaged<Parent> m_ParentPool;
        private EcsPoolUnmanaged<Children> m_ChildrenPool;
        private EcsPoolUnmanaged<IntrusiveListNode> m_NodePool;
    }
}