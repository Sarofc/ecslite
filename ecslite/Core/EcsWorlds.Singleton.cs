using System.Runtime.CompilerServices;

namespace Saro.Entities
{
    public partial class EcsWorld
    {
        private struct SingletonDummy : IEcsComponentSingleton { }

        private int m_SingletonEntityId = -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSingletonEntity()
        {
            if (m_SingletonEntityId < 0)
                m_SingletonEntityId = NewEntity();

            if (m_SingletonEntityId != 0)
                throw new EcsException($"{m_SingletonEntityId} must be 0");

            return m_SingletonEntityId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetSingleton<T>() where T : struct, IEcsComponentSingleton
        {
            var singletonID = GetSingletonEntity();

            return ref GetPool<T>(1, 1, 1).Add(singletonID);
        }

        private void InitSingletonEntity()
        {
            GetSingleton<SingletonDummy>();
        }
    }
}