
using System.Runtime.CompilerServices;

namespace Leopotam.EcsLite
{
    public partial class EcsWorld
    {
        private int m_SingletonEntityId = -1;

        public ref readonly int GetSingletenEntity()
        {
            if (m_SingletonEntityId < 0)
                m_SingletonEntityId = NewEntity();

            return ref m_SingletonEntityId;
        }

        public ref T GetSingleton<T>() where T : struct
        {
            ref readonly var singletonID = ref GetSingletenEntity();

            return ref GetPool<T>(0, 1).Add(singletonID);
        }
    }
}
