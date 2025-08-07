// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Common.Results;
using GameKit.GameSessions.Core;
using GameKit.GameSessions.Core.Commands;
using GameKit.GameSessions.Core.Internal;
using GameKit.GameSessions.Core.Internal.Handlers;
using VContainer;
using VitalMediator;
using VitalMediator.Abstractions;

namespace GameKit.GameSessions.VContainer
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterGameSessions<TGameState>(this IContainerBuilder builder)
            where TGameState : IGameState
        {
            builder.Register<GameSessionManager<TGameState>>(Lifetime.Singleton).AsSelf();
            builder.RegisterMediator(mediator =>
            {
                mediator.RegisterCommand<
                    CreateGameSessionCommand<TGameState>,
                    CreateGameSessionHandler<TGameState>,
                    FastResult<CreateGameSessionData<TGameState>>
                >();

                mediator.RegisterCommand<
                    GetGameStateCommand<TGameState>,
                    GetGameStateHandler<TGameState>,
                    FastResult<TGameState>
                >();
            });
        }
    }

    public static class MediatorExtensions
    {
        public static UniTask<FastResult<CreateGameSessionData<TGameState>>> ExecuteCreateGameSession<TGameState>(
            this IMediator mediator,
            CreateGameSessionCommand<TGameState> command,
            CancellationToken ct = default)
            where TGameState : IGameState
        {
            return mediator.ExecuteAsync<
                CreateGameSessionCommand<TGameState>,
                FastResult<CreateGameSessionData<TGameState>>
            >(command, ct);
        }

        public static UniTask<FastResult<TGameState>> ExecuteGetGameState<TGameState>(
            this IMediator mediator,
            GetGameStateCommand<TGameState> command,
            CancellationToken ct = default)
            where TGameState : IGameState
        {
            return mediator.ExecuteAsync<GetGameStateCommand<TGameState>, FastResult<TGameState>>(command, ct);
        }
    }
}
