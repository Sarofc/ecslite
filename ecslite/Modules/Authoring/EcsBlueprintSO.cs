#if UNITY_EDITOR
#define EDITOR_ENHANCE
#endif

namespace Saro.Entities.Authoring
{
    using System;
    using Saro.Entities.Serialization;
    using UnityEngine;

    [CreateAssetMenu(menuName = "ECS/" + nameof(EcsBlueprintSO))]
    public sealed class EcsBlueprintSO : ScriptableObject
    {
        public EcsBlueprint blueprint;

        public static implicit operator EcsBlueprint(EcsBlueprintSO so) => so.blueprint;
    }

    [Serializable]
    public class EcsBlueprint
    {
#if ODIN_INSPECTOR && EDITOR_ENHANCE
        [Sirenix.OdinInspector.Searchable]
        [Sirenix.OdinInspector.ListDrawerSettings(NumberOfItemsPerPage = 18, OnBeginListElementGUI = nameof(OnBegin), OnEndListElementGUI = nameof(OnEnd))]
#endif
        [SerializeReference]
        public IEcsComponentAuthoring[] components = new IEcsComponentAuthoring[0];

        public static EcsEntity Instantiate(EcsBlueprint blueprint, EcsWorld world)
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
