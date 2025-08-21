using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using UnityEngine.SceneManagement;

namespace GameKit.SceneLauncher.VContainer
{
    using Lazy = InitializableLazy<UniTaskCompletionSource<IObjectResolver>>;

    public class PostLaunchLifetimeScope : LaunchedLifetimeScope
    {
        private static readonly Dictionary<Scene, Lazy> _lazies = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            _lazies.Clear();
        }

        public static UniTask<IObjectResolver> GetLaunchedTask(Scene scene)
        {
            if (_lazies.TryGetValue(scene, out Lazy lazy))
            {
                return lazy.Value.Task;
            }

            lazy = new Lazy(() => new UniTaskCompletionSource<IObjectResolver>());
            _lazies.Add(scene, lazy);
            return lazy.Value.Task;
        }

        protected override void Awake()
        {
            autoRun = false;
            parentReference = new ParentReference();
            base.Awake();
            GetLaunchedTask(gameObject.scene).Forget();

            async UniTaskVoid Launch(CancellationToken cancellationToken)
            {
                LaunchedContext context = await StartupLauncher.LaunchedTask;
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                Startup(context);
                if (_lazies.TryGetValue(gameObject.scene, out Lazy lazy))
                {
                    lazy.Value.TrySetResult(Container);
                }
            }

            Launch(destroyCancellationToken).Forget();
        }

        private void Startup(LaunchedContext context)
        {
            if (ExtraInstaller != null)
            {
                using (Enqueue(ExtraInstaller))
                using (EnqueueParent(context.GetScope()))
                {
                    Build();
                }
            }
            else
            {
                using (EnqueueParent(context.GetScope()))
                {
                    Build();
                }
            }

            ExtraInstaller = null;
        }
    }
}