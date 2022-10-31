using System.Runtime.CompilerServices;
using Saro.Entities.Transforms;

namespace Saro.Entities
{
    public partial class EcsWorld
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DelEntity(int entity)
        {
            // 解除层级结构
            if (ChildrenPool.Has(entity))
            {
                EcsTransformUtility.OnEntityDestroy(this.Pack(entity));
            }
            else
            {
                DelEntity_Internal(entity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DelEntity(EcsEntity entity) => DelEntity(entity.id);
    }
}