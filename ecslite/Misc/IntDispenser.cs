using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Saro.Entities
{
    internal sealed class IntDispenser
    {
        private readonly ConcurrentStack<int> m_FreeInts;

        private int m_LastInt;

        public int LastInt => m_LastInt;

        public IntDispenser(int startInt)
        {
            m_FreeInts = new ConcurrentStack<int>();

            m_LastInt = startInt;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Rent()
        {
            if (!m_FreeInts.TryPop(out int freeInt))
            {
                freeInt = Interlocked.Increment(ref m_LastInt);
            }

            return freeInt;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(int releasedInt) => m_FreeInts.Push(releasedInt);
    }
}
