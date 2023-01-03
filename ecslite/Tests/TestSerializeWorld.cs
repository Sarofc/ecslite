#if FIXED_POINT_MATH
using ME.ECS.Mathematics;
using Single = sfloat;
#else
using Unity.Mathematics;
using Single = System.Single;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Saro.Entities.Serialization;
using Saro.Utility;
using System.Diagnostics;
using System.IO;

namespace Saro.Entities.Tests
{
    public class TestSerializeWorld
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void TestSerializeDeserializeWorld()
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

            var world = systems.GetWorld();

            using var ms = new MemoryStream(2048);

            var writer = new BinaryEcsWriter(ms);
            var reader = new BinaryEcsReader(ms);

            // get
            world.Serialize(writer);
            var json1 = JsonHelper.ToJson(ms.GetBuffer());
            //Log.INFO(json1);

            systems.Run(0.1f);

            // set
            reader.Reset();
            world.Deserialize(reader);

            // get
            writer.Reset();
            world.Serialize(writer);
            var json2 = JsonHelper.ToJson(ms.GetBuffer());
            //Log.INFO(json2);

            Assert.AreEqual(json1, json2);

            // ====================================================
            CreateTestEntity(systems, d2);

            // get
            writer.Reset();
            world.Serialize(writer);
            var json3 = JsonHelper.ToJson(ms.GetBuffer());
            //Log.INFO(json3);

            //Log.INFO($"position: {ms.Position}");

            Assert.AreNotEqual(json2, json3);
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
            ref var c2 = ref e.GetOrAdd<TestUnmanagedComponent>();
            c1.d = c2.d = d;
            return e;
        }

        private class TestComponent : IEcsComponent, IEcsAutoReset<TestComponent>, ICloneable
        {
            public data d;

            public void AutoReset(ref TestComponent c)
            {
                c.d = default;
            }

            public object Clone()
            {
                return new TestComponent { d = d };
            }
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
                var filter = world.Filter().Inc<TestUnmanagedComponent>().Inc<TestComponent>().End();
                int index = 0;
                foreach (var ent in filter)
                {
                    Assert.IsTrue(ent.Has<TestUnmanagedComponent>(world), "EcsEntity:Has should equal true");

                    ref var c = ref ent.Get<TestUnmanagedComponent>(world);
                    Assert.AreEqual(c.d, d[index]);

                    ref var c1 = ref ent.Get<TestComponent>(world);
                    Assert.AreEqual(c1.d, d[index]);

                    //Debug.Log($" {c.d.c} == {d[index].c} ");
                    //Debug.Log($" {c1.d.c} == {d[index].c} ");

                    index++;
                }
            }
        }
    }
}
