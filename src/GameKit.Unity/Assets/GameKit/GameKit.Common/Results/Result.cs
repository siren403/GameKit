// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace GameKit.Common.Results
{
    public record Result
    {
        private readonly List<Error>? _errors = null;

        public IReadOnlyList<Error> Errors
        {
            get
            {
                if (!IsError || _errors is null)
                {
                    throw new System.InvalidOperationException(
                        "Cannot access Errors on a successful result or when errors are null.");
                }

                return _errors.AsReadOnly();
            }
        }

        internal List<Error> GetErrors()
        {
            if (_errors is null)
            {
                throw new System.InvalidOperationException("Cannot access Errors when there are no errors.");
            }

            return _errors;
        }

        public bool IsError => _errors is not null;

        public bool IsOk => !IsError;

        protected Result(string code)
        {
            _errors = new List<Error>
            {
                new Error
                {
                    Code = code
                }
            };
        }

        protected Result(Error error)
        {
            _errors = new List<Error>
            {
                error
            };
        }

        protected Result(List<Error> errors)
        {
            _errors = errors;
        }

        protected Result()
        {
        }

        public static readonly Result Ok = new();

        public static readonly Result Failure = new("General.Failure");

        public static Result Fail(string code, string description)
        {
            return new Result(new Error()
            {
                Code = code, Description = description
            });
        }
    }

    public record Result<T> : Result
    {
        public static new Result<T> Ok(T data)
        {
            return new Result<T>(data);
        }

        public static Result<T> Fail(List<Error> errors)
        {
            return new Result<T>(errors);
        }

        public static new Result<T> Fail(string code)
        {
            return new Result<T>(code);
        }

        public static new Result<T> Fail(string code, string description)
        {
            return new Result<T>(new Error()
            {
                Code = code, Description = description
            });
        }

        public static implicit operator UniTask<Result<T>>(Result<T> result)
        {
            return UniTask.FromResult(result);
        }


        private readonly T? _value = default;

        public T Value
        {
            get
            {
                if (IsError || _value is null)
                {
                    throw new System.InvalidOperationException(
                        "Cannot access Value on an error result or when value is null.");
                }

                return _value;
            }
        }

        protected Result(T value)
        {
            _value = value;
        }

        protected Result(List<Error> errors) : base(errors)
        {
            _value = default;
        }

        protected Result(string code) : base(code)
        {
            _value = default;
        }

        protected Result(Error error) : base(error)
        {

        }

        public override string ToString()
        {
            return IsError
                ? $"Result<{typeof(T).Name}>: {string.Join(", ", Errors)}"
                : $"Result<{typeof(T).Name}>: {Value}";
        }
    }
}
