// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameKit.Assets;
using GameKit.Navigation.Scenes;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;
using VitalRouter.VContainer;

namespace GameKit.Navigation.VContainer
{
    public static partial class ContainerBuilderExtensions
    {
        public static void RegisterSceneNavigator(this IContainerBuilder builder,
            Action<SceneNavigatorBuilder> configure)
        {
            var navigator = new SceneNavigatorBuilder(builder);
            configure(navigator);

            var options = new NavigatorOptions()
            {
            };
            builder.RegisterInstance(options);
            builder.RegisterVitalRouter(routing => routing.Map<SceneNavigator>());
        }
    }

    public class SceneNavigatorBuilder
    {
        private readonly IContainerBuilder _builder;

        public SceneNavigatorBuilder(IContainerBuilder builder)
        {
            _builder = builder;
        }
    }


// public static class NavigatorExtensions
// {
// #if !UNITY_WEBGL
//         private static readonly AsyncLazy _readyCache = new(async () =>
//         {
//             await UniTask.WaitUntil(() => Caching.ready);
//         });
// #endif
//         [Obsolete]
//         public static async UniTask CheckForUpdates(this SceneNavigator navigator)
//         {
//             var updates = await AddressableExtensions.CheckForCatalogUpdates();
//             if (!updates.IsSuccess)
//             {
//                 return;
//             }
//
//             if (!updates.Result.Any())
//             {
//                 return;
//             }
//
//             // TODO: AddressableExtensions.UpdateCatalogs
//             {
// #if !UNITY_WEBGL
//                 await _readyCache;
// #endif
//                 await Addressables.UpdateCatalogs(true, updates.Result);
//             }
//
//             await navigator.ClearAsync();
//             await navigator.StartupAsync();
//         }
// }
}