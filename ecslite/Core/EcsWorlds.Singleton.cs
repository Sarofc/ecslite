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
        public ref T GetSingleton<T>() where T : struct, IEcsComponentSingleton
        {
            var singletonID = GetSingletonEntity();

            return ref GetPool<T>(2, 2, 1).GetOrAdd(singletonID);
        }

        private void InitSingletonEntity()
        {
            GetSingleton<Dummy>();
        }

        /// <summary>
        /// 0号entity是没用的，占个位
        /// </summary>
        private void InitDummyEntity()
        {
            var dummy = NewEntity();
            GetPool<Dummy>(2, 2, 1).GetOrAdd(dummy);
        }
    }
}