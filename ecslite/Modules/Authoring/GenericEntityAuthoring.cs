#if UNITY_EDITOR
#define ODIN_ENHANCE
#endif

using System.Collections.Generic;
using Saro.Pool;
using UnityEngine;

namespace Saro.Entities.Authoring
{
    [CreateAssetMenu(menuName = "ECS/" + nameof(GenericEntityAuthoring))]
    public sealed class GenericEntityAuthoring : ScriptableObject, IEcsConvertToEntity
    {
#if ODIN_INSPECTOR && ODIN_ENHANCE
        [Sirenix.OdinInspector.Searchable]
        [Sirenix.OdinInspector.ListDrawerSettings(NumberOfItemsPerPage = 18, OnBeginListElementGUI = nameof(OnBegin), OnEndListElementGUI = nameof(OnEnd))]
#endif
        [SerializeReference]
        public IEcsComponentAuthoring[] components = new IEcsComponentAuthoring[0];

        int IEcsConvertToEntity.ConvertToEntity(EcsWorld world)
        {
            int ent = world.NewEntity();

            if (components != null && components.Length > 0)
            {
                List<IEcsComponentPostInit> postInits = null;

                for (int i = 0; i < components.Length; i++)
                {
                    var component = components[i];
                    if (component == null)
                    {
                        Log.ERROR($"[{this.name}]. null component. index: {i}");
                        continue;
                    }
                    var pool = world.GetPoolByType(component.GetType());
                    if (pool != null)
                    {
                        if (component is not IEcsComponentNotAdd)
                        {
                            pool.AddRaw(ent, component);
                        }

                        if (component is IEcsComponentPostInit initializable)
                        {
                            if (postInits == null)
                                postInits = ListPool<IEcsComponentPostInit>.Rent();

                            postInits.Add(initializable);
                        }
                    }
                    else
                    {
#if UNITY_EDITOR
                        Log.ERROR($"please ensure pooltype: call EcsWorld::GetPool<{component.GetType()}>(). or Use {nameof(EcsAuthoringGenerator)} to gen script.");
#else
                        Log.ERROR($"please ensure pooltype: call EcsWorld::GetPool<{component.GetType()}>().");
#endif
                    }
                }

                if (postInits != null)
                {
                    foreach (var initializable in postInits)
                    {
                        initializable.PostInitialize(world, ent);
                    }

                    ListPool<IEcsComponentPostInit>.Return(postInits);
                }
            }

            return ent;
        }

#if ODIN_INSPECTOR && ODIN_ENHANCE
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