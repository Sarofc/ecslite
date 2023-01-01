#if FIXED_POINT_MATH
using ME.ECS.Mathematics;
using Single = sfloat;
#else
using Unity.Mathematics;
using Single = System.Single;
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;

namespace Saro.Entities.Tests
{
    public class TestEcsPoolUnmanaged
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void TestUnmanagedEcsPool()
        {
            var systems = EcsMockUtility.CreateMockSystem();

            var d1 = new data
            {
                a = 1,
                c = 1f,
            };

            var d2 = new data
            {
                a = 2,
                c = 3f,
            };
            CreateTestEntity(systems, d1);
            CreateTestEntity(systems, d2);

            systems.Add(new TestSystemUnmanaged(new data[] { d1, d2 }));
            systems.Init();

            systems.Run(0.1f);
        }


        // ==================================================================
        public struct data
        {
            public int a;
            public float c;
            //public object d;
        }

        private EcsEntity CreateTestEntity(EcsSystems systems, in data d)
        {
            var world = systems.GetWorld();
            var e = world.NewEcsEntity();
            ref var c1 = ref e.GetOrAdd<TestComponent>();
            ref var c2 = ref e.GetOrAddUnmanaged<TestUnmanagedComponent>();
            c1.d = c2.d = d;
            return e;
        }

        private struct TestComponent : IEcsComponent
        {
            public data d;
        }

        private struct TestUnmanagedComponent : IEcsComponent
        {
            public data d;
        }

        private struct TestUnmanagedComponentSingleton : IEcsComponentSingleton
        {
            public data d;
        }

        private class TestSystemUnmanaged : MockSystem
        {
            public TestSystemUnmanaged(data[] d)
            {
                this.d = d;
            }

            public data[] d;

            protected override void RunInternal(Single deltaTime)
            {
                var filter = world.Filter().IncUnmanaged<TestUnmanagedComponent>().Inc<TestComponent>().End();
                int index = 0;
                foreach (var ent in filter)
                {
                    Assert.IsTrue(ent.Has<TestUnmanagedComponent>(world), "EcsEntity:Has should equal true");

                    ref var c = ref ent.GetUnmanaged<TestUnmanagedComponent>(world);

                    Assert.AreEqual(c.d, d[index]);

                    //Debug.Log($" {c.d.c} == {d[index].c} ");

                    index++;
                }
            }
        }
    }
}
