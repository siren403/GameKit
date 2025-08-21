using Cysharp.Threading.Tasks;
using GameKit.Assets;
using GameKit.Common.Results;
using UnityEngine;

namespace GameKit.Navigation.Screens.Page.Internal
{
    public class PageFactory<T> : IPageFactory where T : IPage
    {
        private readonly string _key;
        public Transform Parent { get; init; }

        public PageFactory(string key)
        {
            _key = key;
        }

        public async UniTask<FastResult<IPage>> InstantiateAsync()
        {
            var result = await AddressableOperations.InstantiateAsync<T>(_key, parent: Parent);
            if (result.IsError(out FastResult<IPage> fail))
            {
                return fail;
            }

            var page = result.Value as IPage;
            return FastResult<IPage>.Ok(page);
        }
    }
}