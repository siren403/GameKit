using System;
using System.Collections.Generic;

namespace GameKit.Assets
{
    public record CatalogManifest(IList<string> Keys)
    {
        public bool HasUpdate => Keys is { Count: > 0 };

        private CatalogManifest() : this(Array.Empty<string>())
        {
        }
    }
}