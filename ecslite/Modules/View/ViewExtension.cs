using System;
using UnityEngine;

namespace Saro.Entities.View
{
    public static class ViewExtension
    {
        public static void CreateView(this int entity, EcsWorld world, in ViewAssetID view, IEcsViewProvider provider)
        {
            CreateView(entity, world, view.assetID, provider);
        }

        public static void CreateView(this int entity, EcsWorld world, int assetID, IEcsViewProvider provider)
        {
            ref var view = ref entity.Add<View>(world);

            var mono = provider.CreateView(assetID);
            view.mono = mono;
        }

        public static void ReplaceView(this int entity, EcsWorld world, int assetID, IEcsViewProvider provider)
        {
            if (entity.Has<ViewAssetID>(world))
            {
                ref var _assetID = ref entity.Get<ViewAssetID>(world).assetID;
                if (_assetID == assetID)
                {
                    return;
                }

                _assetID = assetID;
            }

            DestroyView(entity, world, provider);

            CreateView(entity, world, assetID, provider);
        }

        public static void DestroyView(this int entity, EcsWorld world, IEcsViewProvider provider)
        {
            if (entity.Has<View>(world))
            {
                ref var view = ref entity.Get<View>(world);

                if (view.mono != null)
                    provider.DestroyView(view.mono);
            }
        }
    }
}
