// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace GameKit.Common.Results
{
    public readonly struct FastResult<T>
    {
        private readonly T? _value;
        private readonly Error _error1;
        private readonly List<Error>? _additionalErrors;

        public bool IsError => _error1.IsValid
                               || _additionalErrors is not null
                               && _additionalErrors.Count > 0;

        public T Value
        {
            get
            {
                if (IsError || _value is null)
                {
                    throw new InvalidOperationException("Cannot access Value on an error result.");
                }

                return _value!;
            }
        }

        public ReadOnlySpan<Error> GetErrors()
        {
            if (!IsError)
            {
                return Array.Empty<Error>();
            }

            if (_additionalErrors is null || _additionalErrors.Count == 0)
            {
                return new[] { _error1 }.AsSpan();
            }

            var errors = new Error[_additionalErrors.Count + 1];
            errors[0] = _error1;
            _additionalErrors.CopyTo(errors, 1);
            return errors.AsSpan();
        }

        private FastResult(T value)
        {
            _value = value;
            _error1 = Error.Invalid;
            _additionalErrors = null;
        }

        private FastResult(T value, Error error1, List<Error>? additionalErrors)
        {
            _value = value;
            _error1 = error1;
            _additionalErrors = additionalErrors ?? new List<Error>();
        }

        private FastResult(T value, Error error1)
        {
            _value = value;
            _error1 = error1;
            _additionalErrors = null;
        }

        private FastResult(T value, Error error1, Error error2)
        {
            _value = value;
            _error1 = error1;
            _additionalErrors = new List<Error> { error2 };
        }

        private FastResult(T value, Error error1, Error error2, Error error3)
        {
            _value = value;
            _error1 = error1;
            _additionalErrors = new List<Error> { error2, error3 };
        }

        public FastResult<TCast> Cast<TCast>()
        {
            return new FastResult<TCast>(
                default!,
                _error1,
                _additionalErrors
            );
        }

        public override string ToString()
        {
            return IsError
                ? $"FastResult<T> Errors: {string.Join(",\n", GetErrors().ToArray())}"
                : $"FastResult<T> Ok: {_value}";
        }

        public static FastResult<T> Ok(T value) => new(value);
        public static FastResult<T> Fail(Error error) => new(default!, error);

        public static FastResult<T> Fail(string code)
        {
            return new FastResult<T>(default!, new Error { Code = code, Description = string.Empty });
        }

        public static FastResult<T> Fail(string code, string description)
        {
            return new FastResult<T>(default!, new Error { Code = code, Description = description });
        }

        public static FastResult<T> Fail(Error error1, Error error2)
        {
            return new FastResult<T>(default!, error1, error2);
        }

        public static FastResult<T> Fail(Error error1, Error error2, Error error3)
        {
            return new FastResult<T>(default!, error1, error2, error3);
        }

        public static implicit operator UniTask<FastResult<T>>(FastResult<T> result)
        {
            return UniTask.FromResult(result);
        }
    }

    public static class FastResult
    {
        public static readonly FastResult<Void> Ok = FastResult<Void>.Ok(new Void());
        public static readonly FastResult<Void> Failure = FastResult<Void>.Fail($"General.{nameof(Failure)}");

        public static FastResult<Void> Fail(string code, string description)
        {
            return FastResult<Void>.Fail(code, description);
        }
    }

    public readonly struct Void
    {
    }
}
