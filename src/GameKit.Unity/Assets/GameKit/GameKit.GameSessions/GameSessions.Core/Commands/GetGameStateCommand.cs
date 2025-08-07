// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System;
using GameKit.Common.Results;
using VitalMediator.Abstractions;

namespace GameKit.GameSessions.Core.Commands
{
    public struct GetGameStateCommand<TGameState> : ICommand<FastResult<TGameState>> where TGameState : IGameState
    {
        public Ulid SessionId { get; init; }
    }
}
