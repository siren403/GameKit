// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using Cysharp.Threading.Tasks;
using GameKit.SceneLauncher.VContainer;
using UnityEngine;
using VContainer;

namespace GameKit.SceneLauncher.VContainer
{
    public class ContainerProvider
    {
        private readonly AsyncLazy<IObjectResolver> _container;

        public ContainerProvider(GameObject origin)
        {
            _container = new AsyncLazy<IObjectResolver>(() =>
                PostLaunchLifetimeScope.GetLaunchedTask(origin.scene));
        }

        public UniTask<IObjectResolver> GetContainer()
        {
            return _container.Task;
        }

        public async UniTask<T> Resolve<T>()
        {
            IObjectResolver container = await GetContainer();
            return container.Resolve<T>();
        }

        public async UniTask<(bool success, T result)> TryResolve<T>()
        {
            IObjectResolver container = await GetContainer();
            return (container.TryResolve(out T result), result);
        }
    }
}
