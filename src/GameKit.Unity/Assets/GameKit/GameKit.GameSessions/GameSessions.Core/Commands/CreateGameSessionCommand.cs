// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using GameKit.Common.Results;
using VitalMediator.Abstractions;

namespace GameKit.GameSessions.Core.Commands
{
    public struct CreateGameSessionCommand<TGameState>
        : ICommand<FastResult<CreateGameSessionData<TGameState>>> where TGameState : IGameState
    {
        public TGameState State { get; init; }
    }

}
