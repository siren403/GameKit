using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameKit.SceneLauncher
{
    public static class StartupLauncher
    {
        private static readonly InitializableLazy<UniTaskCompletionSource<LaunchedContext>> _launchedSource =
            new(() => new UniTaskCompletionSource<LaunchedContext>());

        private static bool _isExecutedLaunch;

        public static UniTask<LaunchedContext> LaunchedTask => _launchedSource.Value.Task;

        // Domain reload support
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnSubsystemRegistration()
        {
            _launchedSource.Initialize();
            _isExecutedLaunch = false;
        }

        private static void ThrowIfAlreadyLaunched()
        {
            if (_isExecutedLaunch)
            {
                throw new InvalidOperationException("Already launched.");
            }
        }

        public static void Launch(LaunchOptions options, Action configuration)
        {
            ThrowIfAlreadyLaunched();
            _isExecutedLaunch = true;
            configuration();
            LaunchedContext context = LaunchedContext.FromOptions(options);
            _launchedSource.Value.TrySetResult(context);
        }

        public static async UniTask Launch(LaunchOptions options, Func<CancellationToken, UniTask> configuration)
        {
            ThrowIfAlreadyLaunched();
            _isExecutedLaunch = true;
            var ct = Application.exitCancellationToken;
            try
            {
                await configuration(ct);
                LaunchedContext context = LaunchedContext.FromOptions(options);
                _launchedSource.Value.TrySetResult(context);
            }
            catch (Exception ex)
            {
                _launchedSource.Value.TrySetException(ex);
            }
        }
    }
}