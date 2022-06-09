using System.Runtime.CompilerServices;

namespace Saro.Entities
{
    public partial class EcsWorld
    {
        public EcsPool<Name> NamePool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_NamePool ??= GetPool<Name>();
        }

        private EcsPool<Name> m_NamePool;

        public int NewEntity(string name)
        {
            var ent = NewEntity();
            NamePool.Add(ent).name = name;
            return ent;
        }
    }
}