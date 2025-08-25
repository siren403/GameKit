using System;
using Microsoft.Extensions.Logging;
using UnityEngine;
using VContainer;
#if USE_ZLOGGER
using ZLogger;
using ZLogger.Unity;
#endif

namespace GameKit.Logger.VContainer
{
    public static class ContainerBuilderExtensions
    {
#if !USE_ZLOGGER
        public static void RegisterLogger(this IContainerBuilder builder)
        {
            if (builder.Exists(typeof(ILogger<>), findParentScopes: true) == false)
            {
                builder.Register(typeof(UnityLogger<>), Lifetime.Singleton).As(typeof(ILogger<>));
            }
        }
#else
        public static void RegisterLogger(this IContainerBuilder builder, Action<ZLoggerBuilder> configuration)
        {
            var logger = new ZLoggerBuilder(builder);
            configuration.Invoke(logger);
            logger.Build();
        }

        public class ZLoggerBuilder
        {
            private readonly IContainerBuilder _builder;

            private Action<ILoggingBuilder> _loggingConfiguration = _ => { };

            public ZLoggerBuilder(IContainerBuilder builder)
            {
                _builder = builder;
            }

            public void UseDefaultSettings()
            {
                _loggingConfiguration = logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Trace);
// #if UNITY_EDITOR
                    // logging.AddZLoggerRollingFile((dt, index) => $"Logs/{dt:yyyy-MM-dd}_{index}.log", 1024 * 1024);
// #endif
                    logging.AddZLoggerUnityDebug(options =>
                    {
                        options.UsePlainTextFormatter(formatter =>
                        {
                            formatter.SetPrefixFormatter($"{0} | {1:short} | ({2}) | ",
                                (in MessageTemplate template, in LogInfo info) =>
                                    template.Format(info.Timestamp, info.LogLevel, info.Category));
                        });
                    });
                };
            }

            public void Build()
            {
                if (_builder.Exists(typeof(ILoggerFactory), findParentScopes: true))
                {
#if UNITY_EDITOR
                    Debug.LogWarning(
                        "ILoggerFactory is already registered in the container. Skipping ZLogger registration.");
#endif
                    return;
                }

                var loggerFactory = LoggerFactory.Create(logging => { _loggingConfiguration.Invoke(logging); });
                _builder.RegisterInstance(loggerFactory);
                _builder.Register(typeof(Logger<>), Lifetime.Singleton).As(typeof(ILogger<>));
            }
        }
#endif
    }
}