#if UNITY_EDITOR

using System.IO;
using System.Text;
using Saro.Utility;
using UnityEditor;

namespace Saro.Entities.Authoring
{
    public static class EcsAuthoringGenerator
    {
        public static string AuthoringScriptPath { get; set; } = "Assets/Scripts/Gen/EcsAuthoringExtensions.gen.cs";

        [MenuItem("Ecs/GenAuthoringComponents")]
        public static void GenAuthoringComponents()
        {
            var sb = new StringBuilder(1024);
            var authoringComponentTypes = TypeUtility.GetSubClassTypesAllAssemblies(typeof(IEcsComponentAuthoring));

            sb.AppendLine("// auto gen. don't modify.");
            sb.AppendLine("namespace Saro.Entities");
            sb.AppendLine("{");
            sb.AppendLine("\tpublic static class EcsAuthoringExtensions");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tpublic static void InitAuthoringPools(this EcsWorld world)");
            sb.AppendLine("\t\t{");
            foreach (var componentType in authoringComponentTypes)
            {
                sb.AppendLine($"\t\t\tworld.GetPool<{componentType.FullName.Replace("+", ".")}>();");
            }
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            File.WriteAllText(AuthoringScriptPath, sb.ToString());
        }
    }
}

#endif