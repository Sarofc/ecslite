using System;
using System.IO;

namespace Saro.Entities.Serialization
{
    public static partial class EcsSerializeUtility
    {
        public static unsafe void SerializeEcsWorldIntoYAML(EcsWorld world, StreamWriter writer)
        {
            var yaml = new YamlWriter(writer);
            //WriteYAMLHeader(yaml);

            // TODO ecslite 考虑使用span整理api，span可以替代一部分裸指针功能，性能也强劲，并且更安全

            world.SerializeIntoYAML(yaml);
        }

        static void WriteYAMLHeader(YamlWriter writer)
        {
            if (writer.CurrentIndent != 0)
            {
                throw new InvalidOperationException("The header can only be written as root element");
            }

            writer.WriteLine(@"%YAML 1.1")
                .WriteLine(@"---")
                .WriteLine(@"# EcsLite Debugging file");
        }
    }
}
