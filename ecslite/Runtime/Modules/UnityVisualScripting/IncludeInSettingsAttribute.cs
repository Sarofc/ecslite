#if !UNITY_VISUAL_SCRIPTING

using System;

namespace Unity.VisualScripting
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    internal sealed class IncludeInSettingsAttribute : Attribute
    {
        public IncludeInSettingsAttribute(bool include)
        {
            this.include = include;
        }

        public bool include { get; private set; }
    }
}

#endif
