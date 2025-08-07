// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System;

namespace GameKit.GameSessions.Core.Internal
{
    internal class GameSession<TGameState> where TGameState : IGameState
    {
        public Ulid Id { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? EndedAt { get; private set; }

        public TGameState State { get; private set; }

        private GameSession(Ulid id, DateTime createdAt, TGameState state)
        {
            Id = id;
            CreatedAt = createdAt;
            EndedAt = null;
            State = state;
        }

        public static GameSession<TGameState> Create(Ulid sessionId, TGameState state)
        {
            return new GameSession<TGameState>(sessionId, DateTime.UtcNow, state);
        }

        public override string ToString()
        {
            return
                $"{nameof(GameSession<TGameState>)}(Id: {Id}, {nameof(CreatedAt)}: {CreatedAt}, {nameof(EndedAt)}: {EndedAt})";
        }
    }
}
