using System.Runtime.CompilerServices;

namespace Saro.Entities
{
    public sealed partial class EcsFilter
    {
        public int this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_DenseEntities[index];
        }

        public int EntitiesCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_EntitiesCount;
        }
    }
}