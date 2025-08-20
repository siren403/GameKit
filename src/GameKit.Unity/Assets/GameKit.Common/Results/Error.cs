// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System;

namespace GameKit.Common.Results
{
    public readonly struct Error : IEquatable<Error>
    {
        public static readonly Error Invalid = new() { Code = string.Empty, Description = string.Empty };

        public string Code { get; init; }
        public string Description { get; init; }

        public bool IsValid => !Equals(Invalid);

        public static implicit operator Error(string code) => new() { Code = code, Description = string.Empty };

        public override string ToString()
        {
            return string.IsNullOrEmpty(Description)
                ? $"Code: {Code}"
                : $"Code: {Code}, Desc: {Description}";
        }


        public bool Equals(Error other)
        {
            return Code == other.Code && Description == other.Description;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Code, Description);
        }
    }
}
