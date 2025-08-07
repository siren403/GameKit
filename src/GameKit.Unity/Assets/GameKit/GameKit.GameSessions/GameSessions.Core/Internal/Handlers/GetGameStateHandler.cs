// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Common.Results;
using GameKit.GameSessions.Core.Commands;
using VitalMediator.Abstractions;

namespace GameKit.GameSessions.Core.Internal.Handlers
{
    internal class GetGameStateHandler<TGameState>
        : ICommandHandler<GetGameStateCommand<TGameState>, FastResult<TGameState>>
        where TGameState : IGameState
    {
        private readonly GameSessionManager<TGameState> _manager;

        public GetGameStateHandler(GameSessionManager<TGameState> manager)
        {
            _manager = manager;
        }

        public UniTask<FastResult<TGameState>> ExecuteAsync(
            GetGameStateCommand<TGameState> command,
            CancellationToken ct
        )
        {
            var result = _manager.GetSession(command.SessionId);

            return result.IsError(out FastResult<TGameState> fail)
                ? fail
                : FastResult<TGameState>.Ok(result.Value.State);
        }
    }
}
