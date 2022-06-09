
namespace Saro.Entities.Extension
{
    // TODO world partial 新增 ecspool 对象缓存？减少大量 字典操作

    public static class WorldExtensions
    {
        public static void DelEntity(this EcsWorld world, EcsPackedEntityWithWorld entity, bool hierarchy)
        {
            if (hierarchy)
            {
                EcsTransformUtility.OnEntityDestroy(entity);
            }

            world.DelEntity(entity.id);
        }

        public static void DelEntity(this EcsWorld world, int entity, bool hierarchy)
        {
            if (hierarchy)
            {
                EcsTransformUtility.OnEntityDestroy(world.PackEntityWithWorld(entity));
            }

            world.DelEntity(entity);
        }
    }
}
