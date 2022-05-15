
using System.Runtime.CompilerServices;

namespace Leopotam.EcsLite
{
    public partial class EcsWorld
    {
        public sealed partial class Mask
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Mask Inc<T1, T2>()
                where T1 : struct
                where T2 : struct
            {
                return Inc<T1>().Inc<T2>();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Mask Inc<T1, T2, T3>()
                where T1 : struct
                where T2 : struct
                where T3 : struct
            {
                return Inc<T1>().Inc<T2>().Inc<T3>();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Mask Inc<T1, T2, T3, T4>()
               where T1 : struct
               where T2 : struct
               where T3 : struct
               where T4 : struct
            {
                return Inc<T1>().Inc<T2>().Inc<T3>().Inc<T4>();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Mask Inc<T1, T2, T3, T4, T5>()
               where T1 : struct
               where T2 : struct
               where T3 : struct
               where T4 : struct
               where T5 : struct
            {
                return Inc<T1>().Inc<T2>().Inc<T3>().Inc<T4>().Inc<T5>();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Mask Exc<T1, T2>()
               where T1 : struct
               where T2 : struct
            {
                return Exc<T1>().Exc<T2>();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Mask Exc<T1, T2, T3>()
                where T1 : struct
                where T2 : struct
                where T3 : struct
            {
                return Exc<T1>().Exc<T2>().Exc<T3>();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Mask Exc<T1, T2, T3, T4>()
               where T1 : struct
               where T2 : struct
               where T3 : struct
               where T4 : struct
            {
                return Exc<T1>().Exc<T2>().Exc<T3>().Exc<T4>();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Mask Exc<T1, T2, T3, T4, T5>()
               where T1 : struct
               where T2 : struct
               where T3 : struct
               where T4 : struct
               where T5 : struct
            {
                return Exc<T1>().Exc<T2>().Exc<T3>().Exc<T4>().Exc<T5>();
            }
        }
    }
}
