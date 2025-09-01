using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using VitalRouter;

namespace GameKit.Navigation.Screens.Core
{
    public interface IScreenLauncher<out TScreen, in TProps> where TScreen : IScreen
    {
        UniTask PublishAsync<TCommand>(
            TCommand command, TProps props,
            CancellationToken ct = default
        ) where TCommand : ICommand;

        UniTask PublishAsync<TCommand, TState>(
            TCommand command, TProps props,
            TState state,
            Action<TState, TScreen, IBindingContext> binding,
            CancellationToken ct = default
        ) where TCommand : ICommand;

        UniTask PublishAsync<TCommand>(
            TCommand command, TProps props,
            Action<TScreen, IBindingContext> binding,
            CancellationToken ct = default
        ) where TCommand : ICommand;
    }
}