using Cysharp.Threading.Tasks;
using GameKit.Assets;
using GameKit.Common.Results;
using UnityEngine;

namespace GameKit.Navigation.Screens.Core.Internal
{
    internal class AddressableScreenFactory<TDomain, TScreen> : IScreenFactory<TDomain>
        where TDomain : IScreen
        where TScreen : TDomain
    {
        private readonly string _key;
        public Transform? Parent { get; init; } = null;

        public AddressableScreenFactory(string key)
        {
            _key = key;
        }

        public async UniTask<FastResult<TDomain>> InstantiateAsync()
        {
            var result = await AddressableOperations.InstantiateAsync<TScreen>(_key, parent: Parent!);
            if (result.IsError(out FastResult<TDomain> fail))
            {
                return fail;
            }

            var screen = result.Value;
            return FastResult<TDomain>.Ok(screen);
        }
    }
}