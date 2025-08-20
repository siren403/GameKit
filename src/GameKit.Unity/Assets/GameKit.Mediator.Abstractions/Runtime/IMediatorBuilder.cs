// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

namespace GameKit.Mediator.Abstractions
{
    public interface IMediatorBuilder
    {
        public void RegisterCommand<TCommand, THandler, TResult>()
            where TCommand : struct, ICommand<TResult>
            where THandler : ICommandHandler<TCommand, TResult>;

        public void RegisterCommand<TCommand, THandler>()
            where TCommand : struct, ICommand
            where THandler : ICommandHandler<TCommand>;
    }
}
