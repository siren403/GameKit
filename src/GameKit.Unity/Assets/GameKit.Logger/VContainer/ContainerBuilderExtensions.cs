using Microsoft.Extensions.Logging;
using VContainer;

namespace GameKit.Logger.VContainer
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterLogger(this IContainerBuilder builder)
        {
#if !USE_ZLOGGER
            if (builder.Exists(typeof(ILogger<>), findParentScopes: true) == false)
            {
                builder.Register(typeof(UnityLogger<>), Lifetime.Singleton).As(typeof(ILogger<>));
            }
#endif
        }
    }
}