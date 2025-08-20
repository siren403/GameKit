using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GameKit.SceneLauncher
{
    public sealed class LaunchOptions
    {
        private static readonly LaunchOptions _default = new();

        public IDictionary<string, object?> Extensions
        {
            get { return _extensions ??= new ConcurrentDictionary<string, object?>(); }
        }

        private ConcurrentDictionary<string, object?>? _extensions;

        private LaunchOptions()
        {
        }

        public static LaunchOptions Create()
        {
            return _default;
        }
    }
}
