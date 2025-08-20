using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Mediator.Abstractions;
using VitalRouter;
using ICommand = GameKit.Mediator.Abstractions.ICommand;

namespace GameKit.Mediator
{
    internal class VitalMediator : IMediator
    {
        private readonly Router _router;
        private readonly CommandHandlerRegistry _commandHandlerRegistry;

        public VitalMediator(Router router, CommandHandlerRegistry commandHandlerRegistry)
        {
            _router = router;
            _commandHandlerRegistry = commandHandlerRegistry;
        }

        private void ThrowIfNotFoundHandler<T>(out object handler)
        {
            if (!_commandHandlerRegistry.TryGetValue(typeof(T), out handler))
            {
                throw new InvalidOperationException(
                    $"No command handler registered for command type {typeof(T).FullName}");
            }
        }

        public async UniTask<TResult> ExecuteAsync<TCommand, TResult>(TCommand command, CancellationToken ct = default)
            where TCommand : ICommand<TResult>
        {
            ThrowIfNotFoundHandler<TCommand>(out object handler);

            TResult? result = default;
            using var subscription =
                _router.SubscribeAwait<TCommand>(async (cmd, ctx) =>
                {
                    result = await ((ICommandHandler<TCommand, TResult>)handler)
                        .ExecuteAsync(cmd, ctx.CancellationToken);
                }, CommandOrdering.Sequential);

            await _router.PublishAsync(command, ct);

            if (result == null)
            {
                throw new InvalidOperationException(
                    $"Command handler for {typeof(TCommand).FullName} did not return a result.");
            }

            return result;
        }

        public async UniTask ExecuteAsync<TCommand>(TCommand command, CancellationToken ct = default)
            where TCommand : ICommand
        {
            ThrowIfNotFoundHandler<TCommand>(out object handler);

            using var subscription =
                _router.SubscribeAwait<TCommand>(
                    async (cmd, ctx) =>
                    {
                        await ((ICommandHandler<TCommand>)handler).ExecuteAsync(cmd, ctx.CancellationToken);
                    }, CommandOrdering.Sequential);

            await _router.PublishAsync(command, ct);
        }
    }
}
