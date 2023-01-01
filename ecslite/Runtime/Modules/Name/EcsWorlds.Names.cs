using System.Runtime.CompilerServices;

namespace Saro.Entities
{
    public partial class EcsWorld
    {
        public EcsPoolManaged<Name> NamePool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_NamePool ??= GetPool<Name>();
        }

        private EcsPoolManaged<Name> m_NamePool;

        public int NewEntity(string name)
        {
            var ent = NewEntity();
            NamePool.GetOrAdd(ent).name = name;
            return ent;
        }
    }
}