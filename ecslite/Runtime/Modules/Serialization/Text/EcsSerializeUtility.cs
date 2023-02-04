using System;
using System.Buffers;
using Saro.FSnapshot;

namespace Saro.Entities.Serialization
{
    public static partial class EcsSerializeUtility
    {
        public static void SerializeEcsWorldIntoText(EcsWorld world, IBufferWriter<char> bufferWriter)
        {
            var writer = new FTextWriter(bufferWriter);

            try
            {
                world.Dump(ref writer);
            }
            catch (Exception e)
            {
                Log.ERROR(e);
            }
            finally
            {
                writer.Dispose();
            }
        }
    }
}
