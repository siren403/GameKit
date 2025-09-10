using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GameKit.Assets.Tables
{
    public abstract class TableContext : IDisposable
    {
        private readonly TableSource _source;

        private readonly Dictionary<Type, object> _cache = new();

        protected TableContext(TableSource source)
        {
            _source = source;
        }

        protected async ValueTask<IReadOnlyList<T>> GetAllAsync<T>(CancellationToken ct = default)
        {
            if (_cache.TryGetValue(typeof(T), out var cached) && cached is IReadOnlyList<T> list)
            {
                return list;
            }

            var entries = _source switch
            {
                AddressableSource(var key) =>
                    (await AddressableOperations.LoadAssetsAsync<IEntityFactory<T>>(key, ct: ct))
                    .Select(factory => factory.ToEntity()).ToArray(),
                InMemorySource(var source) =>
                    source.GetValueOrDefault(typeof(T), Array.Empty<T>()) as IReadOnlyList<T> ?? Array.Empty<T>(),
                _ => throw new NotSupportedException("TableSource is not supported")
            };
            _cache[typeof(T)] = entries;
            return entries;
        }

        public void Dispose()
        {
            _cache.Clear();
        }
    }
}