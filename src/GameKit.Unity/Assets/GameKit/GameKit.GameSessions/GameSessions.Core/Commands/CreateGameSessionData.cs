// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System;

namespace GameKit.GameSessions.Core.Commands
{
    public struct CreateGameSessionData<TGameState> where TGameState : IGameState
    {
        public Ulid SessionId { get; init; }
        public TGameState State { get; init; }

        public void Deconstruct(out Ulid sessionId, out TGameState state)
        {
            sessionId = SessionId;
            state = State;
        }
    }
}
