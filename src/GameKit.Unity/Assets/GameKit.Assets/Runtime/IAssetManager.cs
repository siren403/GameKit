using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets.ResourceLocators;

namespace GameKit.Assets
{
    public interface IAssetManager
    {
        UniTask<AsyncHandleSnapshot<IResourceLocator>> InitializeAsync(CancellationToken ct = default);
        UniTask<AsyncHandleSnapshot<List<string>>> CheckForCatalogUpdatesAsync(CancellationToken ct = default);
    }
}