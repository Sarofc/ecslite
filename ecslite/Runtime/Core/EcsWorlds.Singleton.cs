using System.Runtime.CompilerServices;

namespace Saro.Entities
{
    public partial class EcsWorld
    {
        private struct Dummy : IEcsComponentSingleton { }

        private int m_SingletonEntityId = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSingletonEntity()
        {
            if (m_SingletonEntityId <= 0)
                m_SingletonEntityId = NewEntity();

            if (m_SingletonEntityId != 1)
                throw new EcsException($"SingletonEntityId: {m_SingletonEntityId} must be 1");

            return m_SingletonEntityId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetSingleton<T>() where T : class, IEcsComponentSingleton, new()
        {
            var singletonID = GetSingletonEntity();
            return ref GetOrAddPool<T>(2, 2, 1).GetOrAddInternal(singletonID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref T GetSingletonUnmanaged<T>() where T : unmanaged, IEcsComponentSingleton
        {
            var singletonID = GetSingletonEntity();
            return ref GetOrAddPoolUnmanaged<T>(2, 2, 1).GetOrAddInternal(singletonID);
        }

        /// <summary>
        /// 0号entity是没用的，占个位
        /// 1号entity是singleton
        /// </summary>
        private void InitDummyAndSingletonEntity()
        {
            // dummy
            var dummy = NewEntity();
            GetOrAddPoolUnmanaged<Dummy>(2, 2, 1).GetOrAddInternal(dummy);

            // singleton
            GetSingletonUnmanaged<Dummy>();
        }
    }
}