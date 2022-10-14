#if ENABLE_IL2CPP
#define INLINE_METHODS
#endif

using FLOAT3 = UnityEngine.Vector3;
using QUATERNION = UnityEngine.Quaternion;

using System.Runtime.CompilerServices;

namespace Saro.Entities.Transforms
{
    public static class EcsTransformExtensions
    {
        public static FLOAT3 GetForward(this int entity, EcsWorld world)
            => world.RotationPool.Get(entity).value * FLOAT3.forward;

        public static FLOAT3 GetForward(in this EcsEntity entity)
            => entity.id.GetForward(entity.World);

        public static void SetForward(this int entity, FLOAT3 forward, EcsWorld world)
            => world.RotationPool.Get(entity).value = QUATERNION.LookRotation(forward);

        public static void SetForward(in this EcsEntity entity, FLOAT3 forward)
            => entity.id.SetForward(forward, entity.World);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalPosition(this int entity, EcsWorld world, in FLOAT3 position)
            => world.PositionPool.Add(entity).value = position;

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalPosition(this in EcsEntity entity, in FLOAT3 position)
            => SetLocalPosition(entity.id, entity.World, position);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalRotation(this int entity, EcsWorld world, in QUATERNION rotation)
            => world.RotationPool.Add(entity).value = rotation;

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalRotation(this in EcsEntity entity, in QUATERNION rotation)
            => SetLocalRotation(entity.id, entity.World, rotation);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalScale(this int entity, EcsWorld world, in FLOAT3 scale)
            => world.ScalePool.Add(entity).value = scale;

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalScale(this in EcsEntity entity, in FLOAT3 scale)
            => SetLocalScale(entity.id, entity.World, scale);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static ref FLOAT3 GetLocalPosition(this int entity, EcsWorld world)
            => ref world.PositionPool.Get(entity).value;

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static ref FLOAT3 GetLocalPosition(this in EcsEntity entity)
            => ref GetLocalPosition(entity.id, entity.World);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static ref QUATERNION GetLocalRotation(this int entity, EcsWorld world)
            => ref world.RotationPool.Get(entity).value;

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static ref QUATERNION GetLocalRotation(this in EcsEntity entity)
            => ref GetLocalRotation(entity.id, entity.World);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static ref FLOAT3 GetLocalScale(this int entity, EcsWorld world)
            => ref world.ScalePool.Get(entity).value;

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static ref FLOAT3 GetLocalScale(this in EcsEntity entity)
            => ref GetLocalScale(entity.id, entity.World);

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public static void SetPosition(this int entity, EcsWorld world, in FLOAT3 position)
        {
            if (world.ParentPool.Has(entity))
            {
                ref readonly var parent = ref world.ParentPool.Get(entity).entity;

                Log.Assert(parent.IsNull() == false, $"{Name.GetEntityName(entity, world)}'s parent Dont Must be null");

                if (!parent.IsNull())
                {
                    var parentPosition = parent.GetPosition();
                    var parentRotation = parent.GetRotation();
                    entity.SetLocalPosition(world, QUATERNION.Inverse(parentRotation) *
                                           Multiply_Internal(GetInvScale_Internal(parent), (position - parentPosition)));

                    return;
                }
            }

            entity.SetLocalPosition(world, position);
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public static void SetPosition(this in EcsEntity entity, in FLOAT3 position)
        {
            SetPosition(entity.id, entity.World, position);
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public static void SetRotation(this int entity, EcsWorld world, in QUATERNION rotation)
        {
            if (world.ParentPool.Has(entity))
            {
                ref readonly var parent = ref world.ParentPool.Get(entity).entity;

                Log.Assert(parent.IsNull() == false, $"{Name.GetEntityName(entity, world)}'s parent Dont Must be null");

                if (!parent.IsNull())
                {
                    var containerRotation = parent.GetRotation();
                    var containerRotationInverse = QUATERNION.Inverse(containerRotation);
                    entity.SetLocalRotation(world, containerRotationInverse * rotation);

                    return;
                }
            }

            entity.SetLocalRotation(world, rotation);
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public static void SetRotation(this in EcsEntity entity, in QUATERNION rotation)
        {
            SetRotation(entity.id, entity.World, rotation);
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public static FLOAT3 GetPosition(this int entity, EcsWorld world)
        {
            FLOAT3 worldPos;

            if (world.PositionPool.Has(entity))
            {
                worldPos = entity.GetLocalPosition(world);
            }
            else
            {
                worldPos = FLOAT3.zero;
            }

            if (!world.ParentPool.Has(entity))
            {
                return worldPos;
            }

            var parent = world.ParentPool.Get(entity);

            while (!parent.entity.IsNull())
            {
                QUATERNION worldRot;
                if (world.RotationPool.Has(parent.entity.id))
                {
                    worldRot = parent.entity.GetLocalRotation();
                }
                else
                {
                    worldRot = QUATERNION.identity;
                }

                worldPos = worldRot * Multiply_Internal(GetScale_Internal(in parent.entity),
                    worldPos);

                worldPos += parent.entity.GetLocalPosition();

                if (!world.ParentPool.Has(parent.entity.id))
                {
                    break;
                }

                parent = world.ParentPool.Get(parent.entity.id);
            }

            return worldPos;
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public static FLOAT3 GetPosition(this in EcsEntity entity)
        {
            return GetPosition(entity.id, entity.World);
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public static QUATERNION GetRotation(this int entity, EcsWorld world)
        {
            QUATERNION worldRot;
            if (world.RotationPool.Has(entity))
            {
                worldRot = world.RotationPool.Get(entity).value;
            }
            else
            {
                worldRot = QUATERNION.identity;
            }

            if (!world.ParentPool.Has(entity))
            {
                return worldRot;
            }

            var parent = world.ParentPool.Get(entity);

            while (!parent.entity.IsNull())
            {
                worldRot = world.RotationPool.Add(parent.entity.id).value * worldRot;

                if (!world.ParentPool.Has(parent.entity.id))
                {
                    break;
                }

                parent = world.ParentPool.Get(parent.entity.id);
            }

            return worldRot;
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public static QUATERNION GetRotation(this in EcsEntity entity)
        {
            return GetRotation(entity.id, entity.World);
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public static FLOAT3 GetScale(this int entity, EcsWorld world)
        {
            FLOAT3 worldScale;
            if (world.ScalePool.Has(entity))
            {
                worldScale = world.ScalePool.Get(entity).value;
            }
            else
            {
                worldScale = FLOAT3.one;
            }

            if (!world.ParentPool.Has(entity))
            {
                return worldScale;
            }

            var parent = world.ParentPool.Get(entity);

            ref var pWorldScale = ref worldScale;

            while (!parent.entity.IsNull())
            {
                ref readonly var localScale = ref world.ScalePool.Add(parent.entity.id).value;
                pWorldScale.x *= localScale.x;
                pWorldScale.y *= localScale.y;
                pWorldScale.z *= localScale.z;

                if (!world.ParentPool.Has(parent.entity.id))
                {
                    break;
                }

                parent = world.ParentPool.Get(parent.entity.id);
            }

            return worldScale;
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public static FLOAT3 GetScale(this in EcsEntity entity)
        {
            return GetScale(entity.id, entity.World);
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        private static FLOAT3 Multiply_Internal(FLOAT3 v1, FLOAT3 v2) =>
            new FLOAT3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        private static FLOAT3 GetInvScale_Internal(int entity, EcsWorld world)
        {
            if (world.ScalePool.Has(entity))
            {
                var scale = world.ScalePool.Get(entity).value;

                ref var v = ref scale;

                if (v.x != 0f) v.x = 1f / v.x;
                if (v.y != 0f) v.y = 1f / v.y;
                if (v.z != 0f) v.z = 1f / v.z;

                return v;
            }

            return FLOAT3.one;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        private static FLOAT3 GetInvScale_Internal(in EcsEntity entity)
        {
            return GetInvScale_Internal(entity.id, entity.World);
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        private static FLOAT3 GetScale_Internal(int entity, EcsWorld world)
        {
            if (world.ScalePool.Has(entity))
            {
                return world.ScalePool.Get(entity).value;
            }

            return FLOAT3.one;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        private static FLOAT3 GetScale_Internal(in EcsEntity entity)
        {
            return GetScale_Internal(entity.id, entity.World);
        }
    }
}