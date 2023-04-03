using System.Runtime.CompilerServices;

namespace Saro.Entities
{
    public partial class EcsWorld
    {
        public EcsPoolManaged<EntityName> EntityNamePool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_EntityNamePool ??= GetOrAddPool<EntityName>();
        }

        private EcsPoolManaged<EntityName> m_EntityNamePool;

        public int NewEntity(string name)
        {
            var ent = NewEntity();
            EntityNamePool.GetOrAdd(ent).name = name;
            return ent;
        }
    }
}