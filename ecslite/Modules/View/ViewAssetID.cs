using Saro.Core;
using Saro.Entities.Authoring;

namespace Saro.Entities.View
{
    public interface IViewAsset : IEcsComponentAuthoring
    {
    }

    public struct ViewAssetID : IViewAsset
    {
        [AssetTableID(true)]
        public int assetID;
    }
}
