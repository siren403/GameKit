using System.Collections.Generic;
using GameKit.Common.Results;

namespace GameKit.Assets
{
    using CatalogSnapshot = AsyncHandleSnapshot<List<string>>;

    public readonly struct CatalogResult
    {
        private readonly FastResult<CatalogSnapshot> _result;

        public CatalogResult(FastResult<CatalogSnapshot> result)
        {
            _result = result;
        }

        public bool IsError => _result.IsError;
        public List<string> Value => _result.Value.Result;
        public Error FirstError => _result.FirstError;

        public static implicit operator FastResult<CatalogSnapshot>(CatalogResult result)
            => result._result;

        public static implicit operator CatalogResult(CatalogSnapshot snapshot) =>
            new(FastResult<CatalogSnapshot>.Ok(snapshot));

        public static implicit operator CatalogResult(Error error)
            => new(FastResult<CatalogSnapshot>.Fail(error));

        public override string ToString()
        {
            return _result.ToString();
        }
        
        public FastResult<CatalogSnapshot> Result => _result;
    }
}