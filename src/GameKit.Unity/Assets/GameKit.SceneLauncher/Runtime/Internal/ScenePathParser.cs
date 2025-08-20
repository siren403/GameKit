using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.Assertions;

namespace GameKit.SceneLauncher.Internal
{
    internal partial class ScenePathParser : IAliases
    {
        private const string AliasPrefix = "$";
        private static readonly char[] _trimChars = {'\\', '/'};
        private static readonly Regex _aliasRegex = new(@$"(\{AliasPrefix}[^\/]+)\/");

        private static bool TryMatchAliases(string input, out string alias)
        {
            var result = _aliasRegex.Match(input);
            if (result is { Success: true, Groups: { Count: > 1 } })
            {
                alias = result.Groups[1].Value;
                return true;
            }

            alias = null!;
            return false;
        }
    }

    internal partial class ScenePathParser
    {
        private readonly Dictionary<string, string> _aliases = new();
        private readonly Dictionary<string, ParseSuccess> _cache = new();

        public IAliases Aliases => this;

        string IAliases.this[string key]
        {
            get
            {
                if (!_aliases.TryGetValue(key, out var value))
                {
                    throw new KeyNotFoundException(key);
                }

                return value;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _aliases.Remove(key);
                    return;
                }

                Assert.IsTrue(key.StartsWith(AliasPrefix));
                value = value.TrimEnd(_trimChars);
                _aliases.Add(key, value);
            }
        }

        void IAliases.Clear()
        {
            _aliases.Clear();
            _cache.Clear();
        }

        public ParseResult Parse(string path)
        {
            if (_cache.TryGetValue(path, out var result))
            {
                return result;
            }

            if (!path.StartsWith(AliasPrefix))
            {
                return new ParseFailure(ParseError.NotStartedAlias);
            }

            if (!TryMatchAliases(path, out string alias))
            {
                return new ParseFailure(ParseError.NotIncludeAliasSymbol);
            }

            if (!_aliases.TryGetValue(alias, out string? aliasValue))
            {
                return new ParseFailure(ParseError.NotFoundAlias);
            }

            string parsedPath = path.Replace(alias, aliasValue);
            result = new ParseSuccess(parsedPath);
            _cache[path] = result;

            return result;
        }
    }

    public enum ParseError
    {
        NotStartedAlias,
        NotIncludeAliasSymbol,
        NotFoundAlias
    }

    public record ParseResult(bool Success);

    public record ParseSuccess(string Value) : ParseResult(true);

    public record ParseFailure(ParseError Error) : ParseResult(false);
}
