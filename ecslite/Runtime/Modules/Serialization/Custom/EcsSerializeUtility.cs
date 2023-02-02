using System;
using System.Buffers;
using Saro.FSnapshot;

namespace Saro.Entities.Serialization
{
    public static partial class EcsSerializeUtility
    {
        public static void SerializeEcsWorld(EcsWorld world, IBufferWriter<byte> bufferWriter)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));

            world.Serialize(bufferWriter);
        }

        public static void DeserializeEcsWorld(EcsWorld world, ReadOnlySpan<byte> buffer)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));

            world.Deserialize(buffer);
        }
    }
}
