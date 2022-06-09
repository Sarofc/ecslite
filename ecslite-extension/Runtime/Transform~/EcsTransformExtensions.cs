#if ENABLE_IL2CPP
#define INLINE_METHODS
#endif

#if FIXED_POINT_MATH
using FLOAT = ME.ECS.fp;
using FLOAT2 = ME.ECS.fp2;
using FLOAT3 = ME.ECS.fp3;
using FLOAT4 = ME.ECS.fp4;
using QUATERNION = ME.ECS.fpquaternion;
#else
using FLOAT = System.Single;
using FLOAT2 = UnityEngine.Vector2;
using FLOAT3 = UnityEngine.Vector3;
using FLOAT4 = UnityEngine.Vector4;
using QUATERNION = UnityEngine.Quaternion;
#endif

using System.Runtime.CompilerServices;

namespace Saro.Entities
{
    using Transforms;

    public static class EcsTransformExtensions
    {
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalPosition(this int child, EcsWorld world, in FLOAT3 position)
            => child.Add<Position>(world).value = position;

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalPosition(this in EcsPackedEntityWithWorld child, in FLOAT3 position)
            => SetLocalPosition(child.id, child.world, position);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalRotation(this int child, EcsWorld world, in QUATERNION rotation)
            => child.Add<Rotation>(world).value = rotation;

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalRotation(this in EcsPackedEntityWithWorld child, in QUATERNION rotation)
            => SetLocalRotation(child.id, child.world, rotation);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalScale(this int child, EcsWorld world, in FLOAT3 scale)
            => child.Add<Scale>(world).value = scale;

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalScale(this in EcsPackedEntityWithWorld child, in FLOAT3 scale)
            => SetLocalScale(child.id, child.world, scale);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static FLOAT3 GetLocalPosition(this int child, EcsWorld world)
            => child.Add<Position>(world).value;

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static FLOAT3 GetLocalPosition(this in EcsPackedEntityWithWorld child)
            => GetLocalPosition(child.id, child.world);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static QUATERNION GetLocalRotation(this int child, EcsWorld world) =>
            child.Add<Rotation>(world).value;

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static QUATERNION GetLocalRotation(this in EcsPackedEntityWithWorld child)
            => GetLocalRotation(child.id, child.world);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static FLOAT3 GetLocalScale(this int child, EcsWorld world)
            => child.Add<Scale>(world).value;

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static FLOAT3 GetLocalScale(this in EcsPackedEntityWithWorld child)
            => GetLocalScale(child.id, child.world);

        // [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        // public static Position ToPositionStruct(this in FLOAT3 v) => new Position() { value = v };
        //
        // [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        // public static FLOAT3 ToVector3(this in Position v) => v.value;
        //
        // [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        // public static Rotation ToRotationStruct(this in QUATERNION v) => new Rotation() { value = v };
        //
        // [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        // public static QUATERNION ToQuaternion(this in Rotation v) => v.value;

        // [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        // public static Scale ToScaleStruct(this in FLOAT3 v) => new Scale() { value = v };
        //
        // [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        // public static FLOAT3 ToVector3(this in Scale v) => v.value;

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public static void SetPosition(this int child, EcsWorld world, in FLOAT3 position)
        {
            ref readonly var container = ref child.Add<Parent>(world);
            if (container.entity.IsNull() == false)
            {
                var containerRotation = container.entity.GetRotation();
                var containerPosition = container.entity.GetPosition();
                child.SetLocalPosition(world, QUATERNION.Inverse(containerRotation) *
                                       EcsTransformExtensions.Multiply_INTERNAL(
                                           EcsTransformExtensions.GetInvScale_INTERNAL(
                                               in container.entity),
                                           (position - containerPosition)));
            }
            else
            {
                child.SetLocalPosition(world, position);
            }
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public static void SetPosition(this in EcsPackedEntityWithWorld child, in FLOAT3 position)
        {
            SetPosition(child.id, child.world, position);
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public static void SetRotation(this int child, EcsWorld world, in QUATERNION rotation)
        {
            ref readonly var container = ref child.Add<Parent>(world);
            if (container.entity.IsNull() == false)
            {
                var containerRotation = container.entity.GetRotation();
                var containerRotationInverse = QUATERNION.Inverse(containerRotation);
                child.SetLocalRotation(world, containerRotationInverse * rotation);
            }
            else
            {
                child.SetLocalRotation(world, rotation);
            }
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public static void SetRotation(this in EcsPackedEntityWithWorld child, in QUATERNION rotation)
        {
            SetRotation(child.id, child.world, rotation);
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public static FLOAT3 GetPosition(this int child, EcsWorld world)
        {
            FLOAT3 worldPos;
            if (child.TryGet<Position>(world, out var cPosition) == true)
            {
                worldPos = cPosition.value;
            }
            else
            {
                worldPos = FLOAT3.zero;
            }

            var parent = child.Add<Parent>(world);

            // TODO inspector destroy，报entity已被销毁
            while (parent.entity.IsNull() == false)
            //while (parent.entity.IsAlive() == true)
            {
                QUATERNION worldRot;
                if (parent.entity.TryGet<Rotation>(out var rotation))
                {
                    worldRot = rotation.value;
                }
                else
                {
                    worldRot = QUATERNION.identity;
                }

                worldPos = worldRot * Multiply_INTERNAL(GetScale_INTERNAL(in parent.entity),
                    worldPos);
                worldPos += parent.entity.Add<Position>().value;
                parent = parent.entity.Get<Parent>();
            }

            return worldPos;
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public static FLOAT3 GetPosition(this in EcsPackedEntityWithWorld child)
        {
            return GetPosition(child.id, child.world);
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public static QUATERNION GetRotation(this int child, EcsWorld world)
        {
            QUATERNION worldRot;
            if (child.TryGet<Rotation>(world, out var rotation) == true)
            {
                worldRot = rotation.value;
            }
            else
            {
                worldRot = QUATERNION.identity;
            }
            var parent = child.Add<Parent>(world);

            while (parent.entity.IsNull() == false)
            {
                worldRot = parent.entity.Add<Rotation>().value * worldRot;
                parent = parent.entity.Add<Parent>();
            }

            return worldRot;
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public static QUATERNION GetRotation(this in EcsPackedEntityWithWorld child)
        {
            return GetRotation(child.id, child.world);
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public static FLOAT3 GetScale(this int child, EcsWorld world)
        {
            FLOAT3 worldScale;
            if (child.TryGet<Scale>(world, out var scale) == true)
            {
                worldScale = scale.value;
            }
            else
            {
                worldScale = FLOAT3.one;
            }

            ref var pWorldScale = ref worldScale;

            var parent = child.Add<Parent>(world);

            while (parent.entity.IsNull() == false)
            {
                var localScale = parent.entity.Add<Scale>().value;
                pWorldScale.x *= localScale.x;
                pWorldScale.y *= localScale.y;
                pWorldScale.z *= localScale.z;

                parent = parent.entity.Add<Parent>();
            }

            return worldScale;
        }

#if INLINE_METHODS
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
#endif
        public static FLOAT3 GetScale(this in EcsPackedEntityWithWorld child)
        {
            return GetScale(child.id, child.world);
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        private static FLOAT3 Multiply_INTERNAL(FLOAT3 v1, FLOAT3 v2) =>
            new FLOAT3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        private static FLOAT3 GetInvScale_INTERNAL(in EcsPackedEntityWithWorld entity)
        {
            if (entity.TryGet(out Scale component))
            {
                ref readonly var scale = ref component.value;

                var v = scale;
                ref var pV = ref v;

                if (pV.x != 0f) pV.x = 1f / pV.x;
                if (pV.y != 0f) pV.y = 1f / pV.y;
                if (pV.z != 0f) pV.z = 1f / pV.z;

                return v;
            }

            return FLOAT3.one;
        }

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        private static FLOAT3 GetScale_INTERNAL(in EcsPackedEntityWithWorld entity)
        {
            if (entity.TryGet(out Scale component) == true)
            {
                return component.value;
            }

            return FLOAT3.one;
        }
    }
}