// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameKit.Mediator.Abstractions
{
    public interface IMediator
    {
        UniTask<TResult> ExecuteAsync<TCommand, TResult>(TCommand command, CancellationToken ct = default)
            where TCommand : ICommand<TResult>;

        UniTask ExecuteAsync<TCommand>(TCommand command, CancellationToken ct = default)
            where TCommand : ICommand;
    }
}
