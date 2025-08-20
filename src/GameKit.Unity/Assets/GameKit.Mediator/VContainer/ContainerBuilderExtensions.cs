// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System;
using VContainer;
using GameKit.Mediator.Abstractions;

namespace GameKit.Mediator.VContainer
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterMediator(this IContainerBuilder builder, Action<MediatorBuilder> configuration)
        {
            if (!builder.Exists(typeof(VitalRouter.Router), findParentScopes: true))
            {
                throw new InvalidOperationException(
                    "VitalRouter.Router must be registered before registering the Mediator. " +
                    "Please ensure that you have called RegisterVitalRouter() in your container builder.");
            }

            if (!builder.Exists(typeof(IMediator), includeInterfaceTypes: true, findParentScopes: true))
            {
                builder.Register<IMediator, VitalMediator>(Lifetime.Singleton);
            }

            if (!builder.Exists(typeof(CommandHandlerRegistry), includeInterfaceTypes: true, findParentScopes: true))
            {
                builder.Register<CommandHandlerRegistry>(Lifetime.Singleton);
            }

            var mediator = new MediatorBuilder(builder);
            configuration(mediator);
        }
    }
}