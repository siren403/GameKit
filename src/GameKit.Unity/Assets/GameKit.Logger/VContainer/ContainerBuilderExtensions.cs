using System;
using Microsoft.Extensions.Logging;
using UnityEngine;
using VContainer;
using ZLogger;
using ZLogger.Unity;

namespace GameKit.Logger.VContainer
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterLogger(this IContainerBuilder builder, Action<ZLoggerBuilder> configuration = null)
        {
            var logger = new ZLoggerBuilder(builder);
            configuration?.Invoke(logger);
            logger.Build();
        }

        public class ZLoggerBuilder
        {
            private readonly IContainerBuilder _builder;

            private Action<ILoggingBuilder>? _loggingConfiguration;

            public ZLoggerBuilder(IContainerBuilder builder)
            {
                _builder = builder;
            }

            /// <summary>
            /// LogLevel.Trace, ZLoggerUnityDebug with plain text formatter (Timestamp, LogLevel, Category)
            /// </summary>
            public void UseDefaultSettings()
            {
                _loggingConfiguration = ConfigureDefaultSettings;
            }

            private void ConfigureDefaultSettings(ILoggingBuilder logging)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                logging.SetMinimumLevel(LogLevel.Trace);
#else
                    logging.SetMinimumLevel(LogLevel.Information);
                    // 릴리즈에서는 파일 로깅만 사용
                    // logging.AddZLoggerRollingFile((dt, index) => $"Logs/{dt:yyyy-MM-dd}_{index}.log", 1024 * 1024);
#endif
                logging.AddZLoggerUnityDebug(options =>
                {
                    options.UsePlainTextFormatter(formatter =>
                    {
                        formatter.SetPrefixFormatter($"{0} | {1:short} | ({2}) | ",
                            (in MessageTemplate template, in LogInfo info) =>
                                template.Format(info.Timestamp, info.LogLevel, info.Category));
                    });
                });
            }

            internal void Build()
            {
                if (_builder.Exists(typeof(ILoggerFactory), findParentScopes: true, includeInterfaceTypes: true))
                {
#if UNITY_EDITOR
                    Debug.LogWarning(
                        "ILoggerFactory is already registered in the container. Skipping ZLogger registration.");
#endif
                    return;
                }

                var loggerFactory = LoggerFactory.Create(logging =>
                {
                    if (_loggingConfiguration == null)
                    {
                        ConfigureDefaultSettings(logging);
                    }
                    else
                    {
                        _loggingConfiguration.Invoke(logging);
                    }
                });
                _builder.RegisterInstance(loggerFactory);
                _builder.Register(typeof(Logger<>), Lifetime.Singleton).As(typeof(ILogger<>));
            }
        }
    }
}