#if FIXED_POINT_MATH
using Saro.FPMath;
using Single = Saro.FPMath.sfloat;
#else
using Unity.Mathematics;
using Single = System.Single;
#endif

using System.Runtime.CompilerServices;

namespace Saro.Entities.Transforms
{
    public static partial class EcsTransformUtility
    {
        public static readonly float3 _one = new((Single)1f, (Single)1f, (Single)1f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetForward(this int entity, EcsWorld world)
            => math.mul(entity.GetRotation(world), math.forward());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetForward(this EcsEntity entity)
            => entity.id.GetForward(entity.World);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetForward(this int entity, in float3 forward, EcsWorld world)
            => entity.SetRotation(world, quaternion.LookRotationSafe(forward, math.up()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetForward(this EcsEntity entity, in float3 forward)
            => entity.id.SetForward(forward, entity.World);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetRight(this EcsEntity entity)
            => entity.id.GetRight(entity.World);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetRight(this int entity, EcsWorld world)
            => math.mul(entity.GetRotation(world), math.right());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetRight(this EcsEntity entity, in float3 right)
            => entity.id.SetRight(right, entity.World);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetRight(this int entity, in float3 right, EcsWorld world)
            => entity.SetRotation(world, FromToRotation(right, math.right()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetUp(this EcsEntity entity)
            => entity.id.GetUp(entity.World);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetUp(this int entity, EcsWorld world)
            => math.mul(entity.GetRotation(world), math.up());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetUp(this EcsEntity entity, in float3 up)
            => entity.id.SetUp(up, entity.World);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetUp(this int entity, in float3 up, EcsWorld world)
            => entity.SetRotation(world, FromToRotation(up, math.up()));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalPosition(this int entity, EcsWorld world, in float3 position)
            => world.PositionPool.GetOrAdd(entity).value = position;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalPosition(this EcsEntity entity, in float3 position)
            => SetLocalPosition(entity.id, entity.World, position);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalRotation(this int entity, EcsWorld world, in quaternion rotation)
            => world.RotationPool.GetOrAdd(entity).value = rotation;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalRotation(this EcsEntity entity, in quaternion rotation)
            => SetLocalRotation(entity.id, entity.World, rotation);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalScale(this int entity, EcsWorld world, in float3 scale)
            => world.ScalePool.GetOrAdd(entity).value = scale;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalScale(this EcsEntity entity, in float3 scale)
            => SetLocalScale(entity.id, entity.World, scale);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref float3 GetLocalPosition(this int entity, EcsWorld world)
            => ref world.PositionPool.Get(entity).value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref float3 GetLocalPosition(this EcsEntity entity)
            => ref GetLocalPosition(entity.id, entity.World);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref quaternion GetLocalRotation(this int entity, EcsWorld world)
            => ref world.RotationPool.Get(entity).value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref quaternion GetLocalRotation(this EcsEntity entity)
            => ref GetLocalRotation(entity.id, entity.World);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref float3 GetLocalScale(this int entity, EcsWorld world)
            => ref world.ScalePool.Get(entity).value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref float3 GetLocalScale(this EcsEntity entity)
            => ref GetLocalScale(entity.id, entity.World);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPosition(this int entity, EcsWorld world, in float3 position)
        {
            if (world.ParentPool.Has(entity))
            {
                ref readonly var parent = ref world.ParentPool.Get(entity).entity;

#if DEBUG
                Log.Assert(parent.IsNull() == false, $"{EntityName.GetEntityName(entity, world)}'s parent Dont Must be null");
#endif

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetPosition(this EcsEntity entity, in float3 position)
            => SetPosition(entity.id, entity.World, position);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetRotation(this int entity, EcsWorld world, in quaternion rotation)
        {
            if (world.ParentPool.Has(entity))
            {
                ref readonly var parent = ref world.ParentPool.Get(entity).entity;
#if DEBUG
                Log.Assert(parent.IsNull() == false, $"{EntityName.GetEntityName(entity, world)}'s parent Dont Must be null");
#endif
                if (!parent.IsNull())
                {
                    var containerRotation = parent.GetRotation();
                    var containerRotationInverse = math.inverse(containerRotation);
                    entity.SetLocalRotation(world, math.mul(containerRotationInverse, rotation));

                    return;
                }
            }

            entity.SetLocalRotation(world, rotation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetRotation(this EcsEntity entity, in quaternion rotation)
            => SetRotation(entity.id, entity.World, rotation);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetPosition(this int entity, EcsWorld world)
        {
            float3 worldPos;

            if (world.PositionPool.Has(entity))
                worldPos = entity.GetLocalPosition(world);
            else
                worldPos = float3.zero;

#if DEBUG
            Log.Assert(!math.any(math.isnan(worldPos)), $"{entity}'s Position is NaN");
#endif

            if (!world.ParentPool.Has(entity))
                return worldPos;

            var parent = world.ParentPool.Get(entity);

            while (!parent.entity.IsNull())
            {
                if (world.RotationPool.Has(parent.entity.id))
                {
                    var parentRot = parent.entity.GetLocalRotation();

#if DEBUG
                    Log.Assert(!math.any(math.isnan(parentRot.value)), $"{parent.entity.id}'s Rotation is NaN");
#endif

                    worldPos = math.mul(parentRot, Multiply_Internal(GetScale_Internal(parent.entity), worldPos));
                }

                worldPos += parent.entity.GetLocalPosition();

                if (!world.ParentPool.Has(parent.entity.id))
                    break;

                parent = world.ParentPool.Get(parent.entity.id);
            }

            return worldPos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetPosition(this EcsEntity entity)
            => GetPosition(entity.id, entity.World);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion GetRotation(this int entity, EcsWorld world)
        {
            quaternion worldRot;
            if (world.RotationPool.Has(entity))
                worldRot = world.RotationPool.Get(entity).value;
            else
                worldRot = quaternion.identity;

#if DEBUG
            Log.Assert(!math.any(math.isnan(worldRot.value)), $"{entity}'s Rotation is NaN");
#endif

            if (!world.ParentPool.Has(entity))
                return worldRot;

            var parent = world.ParentPool.Get(entity);

            while (!parent.entity.IsNull())
            {
                if (world.RotationPool.Has(parent.entity.id))
                {
                    var parentRot = world.RotationPool.Get(parent.entity.id).value;

#if DEBUG
                    Log.Assert(!math.any(math.isnan(parentRot.value)), $"{parent.entity.id}'s Rotation is NaN");
#endif

                    worldRot = math.mul(parentRot, worldRot);
                }

                if (!world.ParentPool.Has(parent.entity.id))
                    break;

                parent = world.ParentPool.Get(parent.entity.id);
            }

            return worldRot;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion GetRotation(this EcsEntity entity)
            => GetRotation(entity.id, entity.World);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetScale(this EcsEntity entity)
            => GetScale(entity.id, entity.World);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float3 GetInvScale_Internal(int entity, EcsWorld world)
        {
            if (world.ScalePool.Has(entity))
            {
                var scale = world.ScalePool.Get(entity).value;

                //v = 1f / v;

                if (scale.x != (Single)0f) scale.x = (Single)1f / scale.x;
                if (scale.y != (Single)0f) scale.y = (Single)1f / scale.y;
                if (scale.z != (Single)0f) scale.z = (Single)1f / scale.z;

                return scale;
            }

            return _one;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float3 GetInvScale_Internal(EcsEntity entity)
            => GetInvScale_Internal(entity.id, entity.World);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float3 GetScale_Internal(int entity, EcsWorld world)
        {
            if (world.ScalePool.Has(entity))
                return world.ScalePool.Get(entity).value;
            return _one;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float3 GetScale_Internal(EcsEntity entity)
            => GetScale_Internal(entity.id, entity.World);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float3 Multiply_Internal(in float3 v1, in float3 v2) =>
            new(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion FromToRotation(in float3 from, in float3 to)
            => quaternion.AxisAngle(
                angle: math.acos(math.clamp(math.dot(math.normalizesafe(from), math.normalizesafe(to)), -(Single)1f, (Single)1f)),
                axis: math.normalizesafe(math.cross(from, to))
            );
    }
}