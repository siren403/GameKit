// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameKit.Mediator.Abstractions
{
    public interface ICommandHandler
    {
    }

    public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, Unit> where TCommand : ICommand
    {
        new UniTask ExecuteAsync(TCommand command, CancellationToken ct);

        async UniTask<Unit> ICommandHandler<TCommand, Unit>.ExecuteAsync(TCommand command, CancellationToken ct)
        {
            await ExecuteAsync(command, ct);
            return Unit.Value;
        }
    }

    public interface ICommandHandler<in TCommand, TResult> : ICommandHandler where TCommand : ICommand<TResult>
    {
        UniTask<TResult> ExecuteAsync(TCommand command, CancellationToken ct);
    }
}
