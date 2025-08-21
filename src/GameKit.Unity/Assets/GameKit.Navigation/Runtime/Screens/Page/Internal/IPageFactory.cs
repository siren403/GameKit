using Cysharp.Threading.Tasks;
using GameKit.Common.Results;

namespace GameKit.Navigation.Screens.Page.Internal
{
    public interface IPageFactory
    {
        UniTask<FastResult<IPage>> InstantiateAsync();
    }
}