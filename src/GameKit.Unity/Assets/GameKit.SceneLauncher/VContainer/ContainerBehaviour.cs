// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace GameKit.SceneLauncher.VContainer
{
    public abstract class ContainerBehaviour : MonoBehaviour
    {
        protected virtual bool AutoInject => true;

        protected virtual void Awake()
        {
            PostLaunchLifetimeScope.GetLaunchedTask(gameObject.scene).ContinueWith(container =>
            {
                if (AutoInject)
                {
                    container.Inject(this);
                }
                OnBuild(container);
            });
        }

        protected virtual void OnBuild(IObjectResolver container)
        {

        }
    }
}
