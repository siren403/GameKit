// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameKit.SceneLauncher.VContainer
{
    public static class SceneScopeInitializer
    {
        private static SceneInstallerResolver? _resolver;
        private static Scene? _startedScene = null;

        public static bool IsStartedFromMainScene
        {
            get
            {
                if (_startedScene.HasValue)
                {
                    return _startedScene.Value.buildIndex == 0;
                }

                return false;
            }
        }

        public static bool IsStartedScene(Scene scene)
        {
            if (_startedScene.HasValue)
            {
                return _startedScene.Value == scene;
            }

            return false;
        }


        public static SceneInstallerResolver Resolver => _resolver!;

        private static UniTaskCompletionSource? _mainSceneLoadedSource;

        static SceneScopeInitializer()
        {
            _resolver = null;
        }

        public static void Initialize(SceneInstallerResolver? resolver)
        {
            _resolver = resolver;
            _mainSceneLoadedSource = new UniTaskCompletionSource();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneLoaded()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (_resolver != null)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
            }

            var activeScene = SceneManager.GetActiveScene();
            _startedScene = activeScene;
        }


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CheckMainScene()
        {
            if (_resolver == null)
            {
                return;
            }

            if (SceneManager.sceneCount == 1 && SceneManager.GetSceneAt(0).name.StartsWith("InitTestScene"))
            {
                return;
            }

            bool loadedMainScene = false;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.buildIndex != 0) continue;

                loadedMainScene = true;
                break;
            }

            if (!loadedMainScene)
            {
                SceneManager.LoadScene(0, LoadSceneMode.Additive);
            }
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_resolver == null)
            {
                return;
            }

            bool isMainScene = scene.buildIndex == 0;
            if (isMainScene)
            {
                ScopeInjector.CreateScope<StartupLifetimeScope>(
                    scene,
                    nameof(StartupLifetimeScope),
                    _resolver.Resolve(scene)
                );
                SceneManager.SetActiveScene(scene);
                _mainSceneLoadedSource?.TrySetResult();
            }
            else
            {
                var subScene = scene;
                // WARN: MainScope 초기화 후에 SubScope를 초기화 하는 프로세스가 이미 있었지만
                // Installer를 로드 되었을 떄 기준으로 가져와서 MainScope에서 추가한 인스롤러는 적용되지 않는 문제가 있다.
                // 그래서 임시로 SubScope생성자체를 지연 시킴. 이부분은 추후에 개선 필요.
                _mainSceneLoadedSource?.Task.ContinueWith(() =>
                {
                    ScopeInjector.CreateScope<PostLaunchLifetimeScope>(
                        subScene,
                        nameof(PostLaunchLifetimeScope),
                        _resolver.Resolve(subScene)
                    );
                });
            }
        }
    }
}