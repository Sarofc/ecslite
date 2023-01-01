using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Saro.IO.Test
{
    public class TestBinaryExtension : MonoBehaviour
    {
        [Serializable]
        public struct TestStruct
        {
            public int a;
            public float b;
            public long c;
        }

        public TestStruct obj;
        public TestStruct[] obj_list;

        public int a;

        public byte[] bytes;

        [Button]
        public void Serialize()
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            unsafe
            {
                writer.WriteUnmanaged(obj);
                writer.WriteArrayUnmanaged(ref obj_list, obj_list.Length);
                writer.Write(a);
            }

            bytes = ms.ToArray();
        }

        [Button]
        public void Deserialize()
        {
            using var ms = new MemoryStream(bytes);
            using var reader = new BinaryReader(ms);

            unsafe
            {
                reader.ReadUnmanaged(ref obj);
                var length = reader.ReadArrayUnmanaged(ref obj_list);
                a = reader.ReadInt32();
            }
        }
    }
}
