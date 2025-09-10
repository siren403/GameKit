using System;
using System.Collections.Generic;

namespace GameKit.Assets.Tables
{
    public record InMemorySource(IReadOnlyDictionary<Type, object> Entries) : TableSource
    {
    }
}