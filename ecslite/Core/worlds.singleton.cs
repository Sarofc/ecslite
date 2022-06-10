using System.Runtime.CompilerServices;

namespace Saro.Entities
{
    public partial class EcsWorld
    {
        private int m_SingletonEntityId = -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSingletonEntity()
        {
            if (m_SingletonEntityId < 0)
                m_SingletonEntityId = NewEntity();

            return m_SingletonEntityId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetSingleton<T>() where T : struct, IEcsComponent
        {
            var singletonID = GetSingletonEntity();

            return ref GetPool<T>(0, 1).Add(singletonID);
        }
    }
}