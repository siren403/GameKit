using Cysharp.Threading.Tasks;
using GameKit.Common.Results;

namespace GameKit.Navigation.Screens.Core.Internal
{
    internal interface IScreenFactory<TScreen> where TScreen : IScreen
    {
        UniTask<FastResult<TScreen>> InstantiateAsync();
    }
}