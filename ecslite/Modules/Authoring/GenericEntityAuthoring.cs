#if UNITY_EDITOR
#define EDITOR_ENHANCE
#endif

using System.Collections.Generic;
using Saro.Entities.Serialization;
using Saro.Pool;
using UnityEngine;

namespace Saro.Entities.Authoring
{
    [CreateAssetMenu(menuName = "ECS/" + nameof(GenericEntityAuthoring))]
    public sealed class GenericEntityAuthoring : ScriptableObject, IEcsConvertToEntity
    {
#if ODIN_INSPECTOR && EDITOR_ENHANCE
        [Sirenix.OdinInspector.Searchable]
        [Sirenix.OdinInspector.ListDrawerSettings(NumberOfItemsPerPage = 18, OnBeginListElementGUI = nameof(OnBegin), OnEndListElementGUI = nameof(OnEnd))]
#endif
        [SerializeReference]
        public IEcsComponentAuthoring[] components = new IEcsComponentAuthoring[0];

        int IEcsConvertToEntity.ConvertToEntity(EcsWorld world)
        {
            int ent = world.NewEntity();

            EcsSerializer.Initialize(ent, components, world);

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