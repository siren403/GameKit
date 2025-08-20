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

namespace GameKit.Navigation.VContainer
{
    public static partial class ContainerBuilderExtensions
    {
        public static void RegisterSceneNavigator(this IContainerBuilder builder,
            Action<SceneNavigatorBuilder> configure)
        {
            var nav = new SceneNavigatorBuilder(builder);
            configure(nav);

            var options = new NavigatorOptions()
            {
                AutoStartup = nav.AutoStartup,
                EntryPath = nav.EntryLabel,
            };
            builder.RegisterInstance(options);
            builder.Register<SceneNavigator>(Lifetime.Singleton).AsSelf();
            if (options.AutoStartup)
            {
                builder.RegisterEntryPoint<SceneNavigatorInitializer>();
            }
        }
    }

    public class SceneNavigatorBuilder
    {
        private readonly IContainerBuilder _builder;

        public bool AutoStartup = true;
        public string EntryLabel { get; set; } = string.Empty;

        public SceneNavigatorBuilder(IContainerBuilder builder)
        {
            _builder = builder;
        }
    }


    public static class NavigatorExtensions
    {
#if !UNITY_WEBGL
        private static readonly AsyncLazy _readyCache = new(async () =>
        {
            await UniTask.WaitUntil(() => Caching.ready);
        });
#endif

        /// <summary>
        /// Auto startup
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="label"></param>
        public static void AutoStartupFromMainScene(this SceneNavigatorBuilder builder, string label)
        {
            builder.AutoStartup = SceneManager.GetSceneAt(0).buildIndex == 0;
            builder.EntryLabel = label;
        }

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
    }
}