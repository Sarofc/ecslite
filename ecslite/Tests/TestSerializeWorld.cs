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
using UnityEngine;
using System.Runtime.InteropServices.ComTypes;

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

            var ms = new MemoryStream(1024);

            using var writer = new BinaryEcsWriter(ms);
            using var reader = new BinaryEcsReader(ms);

            // get
            world.Serialize(writer);
            var json1 = JsonHelper.ToJson(new ArraySegment<byte>(ms.GetBuffer(), 0, (int)ms.Position));
            //Log.INFO(json1);
            systems.Run(0.1f);

            // set
            reader.Reset();
            world.Deserialize(reader);

            // text 2
            {
                var textMs2 = new FileStream("text2.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                textMs2.SetLength(0);
                using var textWriter2 = new TextEcsWriter(textMs2);
                world.Serialize(textWriter2);
            }

            // get
            writer.Reset();
            world.Serialize(writer);
            var json2 = JsonHelper.ToJson(new ArraySegment<byte>(ms.GetBuffer(), 0, (int)ms.Position));
            //Log.INFO(json2);

            Assert.AreEqual(json1, json2);

            // ====================================================
            CreateTestEntity(systems, d2);

            // get
            writer.Reset();
            world.Serialize(writer);
            var json3 = JsonHelper.ToJson(new ArraySegment<byte>(ms.GetBuffer(), 0, (int)ms.Position));
            //Log.INFO(json3);

            //Log.INFO($"position: {ms.Position}");

            Assert.AreNotEqual(json2, json3);

            // ======================================================
            // 检测回滚
            var buffer2 = JsonHelper.FromJson<byte[]>(json2);
            var checkMs = new MemoryStream(buffer2);
            using var checkReader = new BinaryEcsReader(checkMs);
            world.Deserialize(checkReader);

            writer.Reset();
            world.Serialize(writer);
            var json4 = JsonHelper.ToJson(new ArraySegment<byte>(ms.GetBuffer(), 0, (int)ms.Position));

            //Log.INFO($"json2: {json2}");
            //Log.INFO($"json4: {json4}");

            // text 4
            {
                var textMs4 = new FileStream("text4.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                textMs4.SetLength(0);
                using var textWriter4 = new TextEcsWriter(textMs4);
                world.Serialize(textWriter4);
            }

            Assert.AreEqual(json2, json4);
        }


        // ==================================================================
        public struct data
        {
            public int a;
            public float c;
            //public object d;

            public override string ToString()
            {
                return $"{a} {c}";
            }
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

        private class TestComponent : IEcsComponent, IEcsAutoReset<TestComponent>, IEcsSerialize
        {
            public data d;

            public void AutoReset(ref TestComponent c)
            {
                c.d = default;
            }

            public void Deserialize(IEcsReader reader)
            {
                reader.ReadUnmanaged(ref d);
            }

            public void Serialize(IEcsWriter writer)
            {
                writer.WriteUnmanaged(ref d);
            }
        }

        private struct TestUnmanagedComponent : IEcsComponent
        {
            public data d;

            public override string ToString()
            {
                return d.ToString();
            }
        }

        private struct TestUnmanagedComponentSingleton : IEcsComponentSingleton
        {
            public data d;

            public override string ToString()
            {
                return d.ToString();
            }
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
