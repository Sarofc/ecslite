
using System.Collections.Generic;
using Saro.Entities.Authoring;
using Saro.Pool;

namespace Saro.Entities.Serialization
{
    /* 
     * TODO 
     * 
     * 1. 目前只做了一个简单的单个entity的序列化
     * 
     * 
     * 问题点：
     * 1. 如何序列化层级关系，即entity的引用
     * 
     */
    public static class EcsSerializer
    {
        public static void Initialize<T>(int entity, IList<T> components, EcsWorld world, bool postInit = true)
        {
            if (components != null && components.Count > 0)
            {
                List<IEcsComponentPostInit> postInitializeList = null;

                for (int i = 0; i < components.Count; i++)
                {
                    var component = components[i];
                    if (component == null)
                    {
                        Log.ERROR($"null component to add. index: {i}. entity: {entity}.");
                        continue;
                    }

                    if (component is not IEcsComponent)
                    {
                        Log.ERROR($"component MUST impl IEcsComponent. index: {i}. entity: {entity}.");
                        continue;
                    }

                    var pool = world.GetPoolByType(component.GetType());
                    if (pool != null)
                    {
                        if (component is not IEcsComponentNotAdd)
                        {
                            pool.AddRaw(entity, component);
                        }

                        if (postInit && component is IEcsComponentPostInit initializable)
                        {
                            if (postInitializeList == null)
                                postInitializeList = ListPool<IEcsComponentPostInit>.Rent();

                            postInitializeList.Add(initializable);
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

                if (postInitializeList != null)
                {
                    foreach (var initializable in postInitializeList)
                    {
                        initializable.PostInitialize(world, entity);
                    }

                    ListPool<IEcsComponentPostInit>.Return(postInitializeList);
                }
            }
        }
    }
}
