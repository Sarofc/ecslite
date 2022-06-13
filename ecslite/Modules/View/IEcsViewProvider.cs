using System;

namespace Saro.Entities.View
{
    public interface IEcsViewProvider
    {
        ViewMono CreateView(string assetID);
        ViewMono CreateView(int assetID);

        void DestroyView(ViewMono mono);
    }
}
