
using Saro.Core;
using UnityEngine;

namespace Saro.Entities.View
{
    public sealed class EcsViewProvider_DefaultAssetLoader : IEcsViewProvider
    {
        private readonly IAssetLoader m_AssetLoader;

        public EcsViewProvider_DefaultAssetLoader()
        {
            m_AssetLoader = new DefaultAssetLoader();
        }

        ViewMono IEcsViewProvider.CreateView(string assetID)
        {
            var prefab = m_AssetLoader.LoadAssetRef<ViewMono>(assetID);
            return GameObject.Instantiate(prefab);
        }

        ViewMono IEcsViewProvider.CreateView(int assetID)
        {
            var prefab = m_AssetLoader.LoadAssetRef<ViewMono>(assetID);
            return GameObject.Instantiate(prefab);
        }

        void IEcsViewProvider.DestroyView(ViewMono mono)
        {
            if (mono.gameObject != null)
                GameObject.Destroy(mono.gameObject);
        }
    }
}
