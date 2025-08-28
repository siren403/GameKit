using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace GameKit.SceneLauncher.VContainer
{
    public static class ScopeInjector
    {
        public static T CreateScope<T>(Scene scene, string name, IInstaller extraInstaller)
            where T : LaunchedLifetimeScope
        {
            Assert.IsNotNull(name);
            var gameObject = new GameObject(name);
            gameObject.SetActive(false);
            var newScope = gameObject.AddComponent<T>();
#if UNITY_EDITOR
            Debug.Log($"Using extra installer. {extraInstaller.GetType().FullName}");
#endif
            newScope.ExtraInstaller = extraInstaller;
            SceneManager.MoveGameObjectToScene(gameObject, scene);
            gameObject.SetActive(true);
            return newScope;
        }
    }
}