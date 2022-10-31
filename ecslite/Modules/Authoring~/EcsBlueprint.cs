#if UNITY_EDITOR
#define EDITOR_ENHANCE
#endif

namespace Saro.Entities.Authoring
{
    using System;
    using System.Collections;
    using Saro.Entities.Serialization;
    using Unity.VisualScripting.YamlDotNet.Core;
    using UnityEngine;

    [Serializable]
    public class EcsBlueprint : MonoBehaviour
    {
#if ODIN_INSPECTOR && EDITOR_ENHANCE
        [Sirenix.OdinInspector.Searchable]
        [Sirenix.OdinInspector.ListDrawerSettings(NumberOfItemsPerPage = 18, OnBeginListElementGUI = nameof(OnBegin), OnEndListElementGUI = nameof(OnEnd))]
#endif
        [SerializeReference]
        public IEcsComponentAuthoring[] components = new IEcsComponentAuthoring[0];

        public static EcsEntity Spawn(EcsBlueprint blueprint, EcsWorld world)
        {
            var ent = world.NewEcsEntity();
            EcsSerializer.Initialize(ent.id, blueprint.components, world);
            return ent;
        }

#if ODIN_INSPECTOR && EDITOR_ENHANCE
        private void OnBegin(int index)
        {
            Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox();
        }

        private void OnEnd(int index)
        {
            Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
        }
#endif
    }
}
