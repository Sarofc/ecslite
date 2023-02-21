// #if FIXED_POINT_MATH
// using Saro.FMath;
// using Single = Saro.FMath.sfloat;
// #else
// using Unity.Mathematics;
// using Single = System.Single;
// #endif

//using System;

//namespace Saro.Entities
//{
//    /// <summary>
//    /// 标记位此component的entity，视为被销毁，将在 <see cref="DestroyEntitySystem"/> 统一真正销毁entity
//    /// </summary>
//    internal readonly struct Destroy : IEcsComponent
//    { }

//    /// <summary>
//    /// 默认要在帧末尾，销毁entity
//    /// </summary>
//    internal sealed class DestroyEntitySystem : IEcsInitSystem, IEcsRunSystem
//    {
//        private EcsWorld m_World;
//        private EcsFilter m_Destroyeds;

//        bool IEcsRunSystem.Enable { get; set; } = true;

//        void IEcsInitSystem.Init(EcsSystems systems)
//        {
//            m_World = systems.GetWorld();
//            m_Destroyeds = m_World.Filter().Inc<Destroy>().End();
//        }

//        void IEcsRunSystem.Run(EcsSystems systems, Single deltaTime)
//        {
//            foreach (var entity in m_Destroyeds)
//            {
//                m_World.DelEntity_Internal(entity);
//            }
//        }
//    }
//}
