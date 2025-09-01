using System;
using System.Threading;
using System.Threading.Tasks;
using R3;
using VitalRouter;

namespace GameKit.Navigation.Screens.Core
{
    public interface IBindingContext
    {
        void Subscribe<T, TState>(
            Observable<T> source,
            TState state,
            Func<T, TState, CancellationToken, ValueTask> action
        );

        void Subscribe<T>(
            Observable<T> source,
            Func<T, CancellationToken, ValueTask> action
        );

        void Subscribe<T, TCommand>(Observable<T> source, TCommand command) where TCommand : struct, ICommand;
    }
}