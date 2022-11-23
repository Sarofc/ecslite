using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Saro.Entities.Transforms
{
    //private static readonly Vector3 zeroVector = new Vector3(0f, 0f, 0f);
    //private static readonly Vector3 oneVector = new Vector3(1f, 1f, 1f);
    //private static readonly Vector3 upVector = new Vector3(0f, 1f, 0f);
    //private static readonly Vector3 downVector = new Vector3(0f, -1f, 0f);
    //private static readonly Vector3 leftVector = new Vector3(-1f, 0f, 0f);
    //private static readonly Vector3 rightVector = new Vector3(1f, 0f, 0f);
    //private static readonly Vector3 forwardVector = new Vector3(0f, 0f, 1f);

    public static class EcsTransformExtensions
    {
        public static readonly float3 _one = new(1f, 1f, 1f);

        public static float3 GetForward(this int entity, EcsWorld world)
            => math.mul(entity.GetRotation(world), math.forward());

        public static float3 GetForward(this EcsEntity entity)
            => entity.id.GetForward(entity.World);

        public static void SetForward(this int entity, float3 forward, EcsWorld world)
            => entity.SetRotation(world, quaternion.LookRotationSafe(forward, math.up()));

        public static void SetForward(this EcsEntity entity, float3 forward)
            => entity.id.SetForward(forward, entity.World);

        public static void GetRight(this EcsEntity entity)
            => entity.id.GetRight(entity.World);

        public static float3 GetRight(this int entity, EcsWorld world)
            => math.mul(entity.GetRotation(world), math.right());

        public static void SetRight(this EcsEntity entity, float3 right)
            => entity.id.SetRight(right, entity.World);

        public static void SetRight(this int entity, float3 right, EcsWorld world)
            => entity.SetRotation(world, FromToRotation(right, math.right()));

        public static void GetUp(this EcsEntity entity)
            => entity.id.GetUp(entity.World);

        public static float3 GetUp(this int entity, EcsWorld world)
            => math.mul(entity.GetRotation(world), math.up());

        public static void SetUp(this EcsEntity entity, float3 up)
            => entity.id.SetUp(up, entity.World);

        public static void SetUp(this int entity, float3 up, EcsWorld world)
            => entity.SetRotation(world, FromToRotation(up, math.up()));


        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalPosition(this int entity, EcsWorld world, in float3 position)
            => world.PositionPool.GetOrAdd(entity).value = position;

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalPosition(this EcsEntity entity, in float3 position)
            => SetLocalPosition(entity.id, entity.World, position);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalRotation(this int entity, EcsWorld world, in quaternion rotation)
            => world.RotationPool.GetOrAdd(entity).value = rotation;

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalRotation(this EcsEntity entity, in quaternion rotation)
            => SetLocalRotation(entity.id, entity.World, rotation);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalScale(this int entity, EcsWorld world, in float3 scale)
            => world.ScalePool.GetOrAdd(entity).value = scale;

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalScale(this EcsEntity entity, in float3 scale)
            => SetLocalScale(entity.id, entity.World, scale);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static ref float3 GetLocalPosition(this int entity, EcsWorld world)
            => ref world.PositionPool.Get(entity).value;

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static ref float3 GetLocalPosition(this EcsEntity entity)
            => ref GetLocalPosition(entity.id, entity.World);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static ref quaternion GetLocalRotation(this int entity, EcsWorld world)
            => ref world.RotationPool.Get(entity).value;

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static ref quaternion GetLocalRotation(this EcsEntity entity)
            => ref GetLocalRotation(entity.id, entity.World);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static ref float3 GetLocalScale(this int entity, EcsWorld world)
            => ref world.ScalePool.Get(entity).value;

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static ref float3 GetLocalScale(this EcsEntity entity)
            => ref GetLocalScale(entity.id, entity.World);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetPosition(this int entity, EcsWorld world, in float3 position)
        {
            if (world.ParentPool.Has(entity))
            {
                ref readonly var parent = ref world.ParentPool.Get(entity).entity;

                Log.Assert(parent.IsNull() == false, $"{Name.GetEntityName(entity, world)}'s parent Dont Must be null");

                if (!parent.IsNull())
                {
                    var parentPosition = parent.GetPosition();
                    var parentRotation = parent.GetRotation();

                    entity.SetLocalPosition(world, math.mul(math.inverse(parentRotation),
                                           math.mul(GetInvScale_Internal(parent), (position - parentPosition))));
                    //entity.SetLocalPosition(world, quaternion.Inverse(parentRotation) *
                    //                       Multiply_Internal(GetInvScale_Internal(parent), (position - parentPosition)));

                    return;
                }
            }

            entity.SetLocalPosition(world, position);
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetPosition(this EcsEntity entity, in float3 position)
            => SetPosition(entity.id, entity.World, position);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetRotation(this int entity, EcsWorld world, in quaternion rotation)
        {
            if (world.ParentPool.Has(entity))
            {
                ref readonly var parent = ref world.ParentPool.Get(entity).entity;

                Log.Assert(parent.IsNull() == false, $"{Name.GetEntityName(entity, world)}'s parent Dont Must be null");

                if (!parent.IsNull())
                {
                    var containerRotation = parent.GetRotation();
                    var containerRotationInverse = math.inverse(containerRotation);
                    //var containerRotationInverse = quaternion.Inverse(containerRotation);
                    entity.SetLocalRotation(world, math.mul(containerRotationInverse, rotation));

                    return;
                }
            }

            entity.SetLocalRotation(world, rotation);
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetRotation(this EcsEntity entity, in quaternion rotation)
            => SetRotation(entity.id, entity.World, rotation);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static float3 GetPosition(this int entity, EcsWorld world)
        {
            float3 worldPos;

            if (world.PositionPool.Has(entity))
                worldPos = entity.GetLocalPosition(world);
            else
                worldPos = float3.zero;

            if (!world.ParentPool.Has(entity))
                return worldPos;

            var parent = world.ParentPool.Get(entity);

            while (!parent.entity.IsNull())
            {
                quaternion worldRot;
                if (world.RotationPool.Has(parent.entity.id))
                    worldRot = parent.entity.GetLocalRotation();
                else
                    worldRot = quaternion.identity;

                worldPos = math.mul(worldRot, math.mul(GetScale_Internal(parent.entity),
                    worldPos));

                worldPos += parent.entity.GetLocalPosition();

                if (!world.ParentPool.Has(parent.entity.id))
                    break;

                parent = world.ParentPool.Get(parent.entity.id);
            }

            return worldPos;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static float3 GetPosition(this EcsEntity entity)
            => GetPosition(entity.id, entity.World);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static quaternion GetRotation(this int entity, EcsWorld world)
        {
            quaternion worldRot;
            if (world.RotationPool.Has(entity))
                worldRot = world.RotationPool.Get(entity).value;
            else
                worldRot = quaternion.identity;

            if (!world.ParentPool.Has(entity))
                return worldRot;

            var parent = world.ParentPool.Get(entity);

            while (!parent.entity.IsNull())
            {
                worldRot = math.mul(world.RotationPool.GetOrAdd(parent.entity.id).value, worldRot);

                if (!world.ParentPool.Has(parent.entity.id))
                {
                    break;
                }

                parent = world.ParentPool.Get(parent.entity.id);
            }

            return worldRot;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static quaternion GetRotation(this EcsEntity entity) 
            => GetRotation(entity.id, entity.World);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static float3 GetScale(this int entity, EcsWorld world)
        {
            float3 worldScale;
            if (world.ScalePool.Has(entity))
                worldScale = world.ScalePool.Get(entity).value;
            else
                worldScale = _one;

            if (!world.ParentPool.Has(entity))
                return worldScale;

            var parent = world.ParentPool.Get(entity);

            ref var pWorldScale = ref worldScale;

            while (!parent.entity.IsNull())
            {
                if (world.ScalePool.Has(parent.entity.id))
                {
                    ref var localScale = ref world.ScalePool.Get(parent.entity.id).value;
                    pWorldScale.x *= localScale.x;
                    pWorldScale.y *= localScale.y;
                    pWorldScale.z *= localScale.z;
                }

                if (!world.ParentPool.Has(parent.entity.id))
                    break;

                parent = world.ParentPool.Get(parent.entity.id);
            }

            return worldScale;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static float3 GetScale(this EcsEntity entity)
            => GetScale(entity.id, entity.World);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        private static float3 GetInvScale_Internal(int entity, EcsWorld world)
        {
            if (world.ScalePool.Has(entity))
            {
                var scale = world.ScalePool.Get(entity).value;

                //v = 1f / v;
                if (scale.x != 0f) scale.x = 1f / scale.x;
                if (scale.y != 0f) scale.y = 1f / scale.y;
                if (scale.z != 0f) scale.z = 1f / scale.z;

                return scale;
            }

            return _one;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        private static float3 GetInvScale_Internal(EcsEntity entity)
            => GetInvScale_Internal(entity.id, entity.World);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        private static float3 GetScale_Internal(int entity, EcsWorld world)
        {
            if (world.ScalePool.Has(entity))
                return world.ScalePool.Get(entity).value;
            return _one;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        private static float3 GetScale_Internal(EcsEntity entity)
            => GetScale_Internal(entity.id, entity.World);

        public static quaternion FromToRotation(in float3 from, in float3 to)
            => quaternion.AxisAngle(
                angle: math.acos(math.clamp(math.dot(math.normalizesafe(from), math.normalizesafe(to)), -1f, 1f)),
                axis: math.normalizesafe(math.cross(from, to))
            );
    }
}