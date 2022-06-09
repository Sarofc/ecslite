namespace Saro.Entities
{
    public sealed partial class EcsFilter
    {
        public int this[int index] => m_DenseEntities[index];

        public int EntitiesCount => m_EntitiesCount;
    }
}