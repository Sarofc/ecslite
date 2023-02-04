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
using Saro.FSnapshot;
using System.Buffers;

namespace Saro.Entities.Tests.SerializeWorld
{
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

    [FSnapshotable]
    internal partial class TestComponent : IEcsComponent, IEcsAutoReset<TestComponent>
    {
        [FSnapshot] public data d;

        public void AutoReset(ref TestComponent c)
        {
            c.d = default;
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder(128);

            sb.AppendLine($"d: {d.ToString()}");

            return sb.ToString();
        }
    }

    internal struct TestUnmanagedComponent : IEcsComponent
    {
        public data d;

        public override string ToString()
        {
            return d.ToString();
        }
    }

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
            var e1 = CreateTestEntity(systems, d1);
            var e2 = CreateTestEntity(systems, d2);

            var world = systems.GetWorld();

            var bufferWriter = new ArrayBufferWriter<byte>(1024);

            EcsSerializeUtility.SerializeEcsWorld(world, bufferWriter);

            // get
            var json1 = JsonHelper.ToJson(bufferWriter.WrittenSpan.ToArray());
            //Log.INFO(json1);
            systems.Run((Single)0.1f);

            // set
            EcsSerializeUtility.DeserializeEcsWorld(world, bufferWriter.WrittenSpan);

            // get
            bufferWriter.Clear();
            EcsSerializeUtility.SerializeEcsWorld(world, bufferWriter);
            var json2 = JsonHelper.ToJson(bufferWriter.WrittenSpan.ToArray());
            //Log.INFO(json2);

            Assert.AreEqual(json1, json2);

            // ====================================================

            //DumpText(world);

            CreateTestEntity(systems, d2);
            ChangeEntityData(e2, d1);

            // get
            bufferWriter.Clear();
            EcsSerializeUtility.SerializeEcsWorld(world, bufferWriter);
            var json3 = JsonHelper.ToJson(bufferWriter.WrittenSpan.ToArray());
            //Log.INFO(json3);

            Assert.AreNotEqual(json2, json3);

            // ======================================================
            // 检测回滚
            var buffer2 = JsonHelper.FromJson<byte[]>(json2);
            EcsSerializeUtility.DeserializeEcsWorld(world, buffer2);

            bufferWriter.Clear();
            EcsSerializeUtility.SerializeEcsWorld(world, bufferWriter);
            var json4 = JsonHelper.ToJson(bufferWriter.WrittenSpan.ToArray());

            //Log.INFO($"rollback. json2: {json2}");
            //Log.INFO($"rollback. json4: {json4}");

            //DumpText(world);

            Assert.AreEqual(json2, json4);
        }


        // ==================================================================

        private void DumpText(EcsWorld world)
        {
            var bufferWriter = new ArrayBufferWriter<char>(1024);
            EcsSerializeUtility.SerializeEcsWorldIntoText(world, bufferWriter);
            Log.INFO(bufferWriter.WrittenSpan.ToString());
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

        private void ChangeEntityData(EcsEntity entity, in data d)
        {
            entity.Get<TestComponent>().d = d;
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






    // TODO unit test 下，代码生成无效，这里拷贝一份来用
    partial class TestComponent : IFSnapshotable
    {
        public void RestoreSnapshot(ref FSnapshotReader reader)
        {

            reader.ReadUnmanaged(ref d);

        }
        public void TakeSnapshot(ref FSnapshotWriter writer)
        {


            writer.WriteUnmanaged(d);


        }

        public void Dump(ref FTextWriter writer)
        {

            writer.WriteUnmanaged(d);
        }

    }

    partial class TestComponent
    {
        static TestComponent()
        {
            if (!FSnapshotFormatterProvider.IsRegistered<global::Saro.Entities.Tests.SerializeWorld.TestComponent>())
            {
                FSnapshotFormatterProvider.Register(new Formatter());
            }
        }

        class Formatter : FSnapshotFormatter<TestComponent>
        {
            public override void TakeSnapshot(ref FSnapshotWriter writer, in global::Saro.Entities.Tests.SerializeWorld.TestComponent @ref)
            {
                if (writer.WriteObjectHeader(@ref))
                    return;

                var refId = writer.WriteObjectReferenceId(@ref);
                if (refId == -1)
                {
                    writer.AddObjectReference(@ref);
                    @ref.TakeSnapshot(ref writer);
                }
            }

            public override void RestoreSnapshot(ref FSnapshotReader reader, ref global::Saro.Entities.Tests.SerializeWorld.TestComponent @ref)
            {
                var isNull = reader.ReadObjectHeader();
                if (isNull)
                {
                    @ref = default; // TODO 销毁实例，池对象怎么办？
                    return;
                }

                var refId = reader.ReadObjectReferenceId();
                if (refId == -1)
                {
                    if (@ref == null)
                        @ref = new global::Saro.Entities.Tests.SerializeWorld.TestComponent(); // TODO 创建实例

                    reader.AddObjectReference(@ref);
                    @ref.RestoreSnapshot(ref reader);
                }
                else
                {
                    @ref = reader.GetObjectReference<global::Saro.Entities.Tests.SerializeWorld.TestComponent>(refId);
                }
            }

            public override void Dump(ref FTextWriter writer, in global::Saro.Entities.Tests.SerializeWorld.TestComponent value)
            {
                value.Dump(ref writer);
            }

        }
    }
}
