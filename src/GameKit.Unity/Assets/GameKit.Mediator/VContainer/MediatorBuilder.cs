// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using VContainer;
using GameKit.Mediator.Abstractions;

namespace GameKit.Mediator
{
    public class MediatorBuilder : IMediatorBuilder
    {
        private readonly IContainerBuilder _builder;

        public MediatorBuilder(IContainerBuilder builder)
        {
            _builder = builder;
        }

        public void RegisterCommand<TCommand, THandler, TResult>()
            where TCommand : struct, ICommand<TResult>
            where THandler : ICommandHandler<TCommand, TResult>
        {
            _builder.Register<THandler>(Lifetime.Singleton).AsSelf();
            _builder.RegisterBuildCallback(static container =>
            {
                var registry = container.Resolve<CommandHandlerRegistry>();
                var handler = container.Resolve<THandler>();
                registry.TryAdd(typeof(TCommand), handler);
            });
        }

        public void RegisterCommand<TCommand, THandler>()
            where TCommand : struct, ICommand
            where THandler : ICommandHandler<TCommand>
        {
            _builder.Register<THandler>(Lifetime.Singleton).AsSelf();
            _builder.RegisterBuildCallback(static container =>
            {
                var registry = container.Resolve<CommandHandlerRegistry>();
                var handler = container.Resolve<THandler>();
                registry.TryAdd(typeof(TCommand), handler);
            });
        }
    }
}
