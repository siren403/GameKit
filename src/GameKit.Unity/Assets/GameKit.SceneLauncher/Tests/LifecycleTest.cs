using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.SceneLauncher.VContainer;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;
using VContainer.Unity;

namespace GameKit.SceneLauncher.Tests
{
    [TestFixture]
    public class LifecycleTest
    {
        [UnityTest]
        public IEnumerator Main()
        {
            var lifecycle = new GameObject().AddComponent<LifecycleQueue>();
            var scene = SceneManager.GetActiveScene();
            var startup = ScopeInjector.CreateScope<StartupLifetimeScope>(
                scene,
                "StartupLifetimeScope",
                new EntryPointInstaller(lifecycle.Queue)
            );
            return UniTask.ToCoroutine(async () =>
            {
                await UniTask.DelayFrame(3);
                var a = (
                    lifecycle.Queue.Dequeue(),
                    lifecycle.Queue.Dequeue(),
                    lifecycle.Queue.Dequeue()
                );
                var b = (
                    LifecycleQueue.KeyAwake,
                    LifecycleQueue.KeyOnEnable,
                    EntryPointInstaller.KeyInitialize
                );
                Assert.AreEqual(a, b);
            });
        }

        [UnityTest]
        public IEnumerator MainNextSub()
        {
            return UniTask.ToCoroutine(async () => { });
        }

        [UnityTest]
        public IEnumerator SubNextMain()
        {
            return UniTask.ToCoroutine(async () => { });
        }
    }

    internal class EntryPointInstaller : IInstaller
    {
        public const string KeyInitialize = "EntryPoint.Initialize";
        public const string KeyStart = "EntryPoint.Start";
        public const string KeyStartAsync = "EntryPoint.StartAsync";
        private readonly Queue<string> _queue;

        public EntryPointInstaller(Queue<string> queue)
        {
            _queue = queue;
        }

        public void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<EntryPoints>().WithParameter(_queue);
        }

        private class EntryPoints : IInitializable, IStartable, IAsyncStartable
        {
            private readonly Queue<string> _queue;

            public EntryPoints(Queue<string> queue)
            {
                _queue = queue;
            }

            public UniTask StartAsync(CancellationToken cancellation = new())
            {
                Debug.Log(KeyStartAsync);
                _queue.Enqueue(KeyStartAsync);
                return UniTask.CompletedTask;
            }

            public void Initialize()
            {
                Debug.Log(KeyInitialize);
                _queue.Enqueue(KeyInitialize);
            }

            public void Start()
            {
                Debug.Log(KeyStart);
                _queue.Enqueue(KeyStart);
            }
        }
    }

    internal class LifecycleQueue : MonoBehaviour
    {
        public const string KeyAwake = "Awake";
        public const string KeyStart = "Start";
        public const string KeyUpdate = "Update";
        public const string KeyOnEnable = "OnEnable";
        public const string KeyOnDisable = "OnDisable";
        public const string KeyOnDestroy = "OnDestroy";

        public readonly Queue<string> Queue = new();

        private int _ticks;

        private void Awake()
        {
            Debug.Log(KeyAwake);
            Queue.Enqueue(KeyAwake);
        }

        private void Start()
        {
            Debug.Log(KeyStart);
            Queue.Enqueue(KeyStart);
        }

        private void Update()
        {
            _ticks++;
            if (_ticks <= 1)
            {
                Debug.Log(KeyUpdate);
                Queue.Enqueue(KeyUpdate);
            }
        }

        private void OnEnable()
        {
            Debug.Log(KeyOnEnable);
            Queue.Enqueue(KeyOnEnable);
        }

        private void OnDisable()
        {
            Debug.Log(KeyOnDisable);
            Queue.Enqueue(KeyOnDisable);
        }

        private void OnDestroy()
        {
            Debug.Log(KeyOnDestroy);
            Queue.Enqueue(KeyOnDestroy);
        }
    }
}