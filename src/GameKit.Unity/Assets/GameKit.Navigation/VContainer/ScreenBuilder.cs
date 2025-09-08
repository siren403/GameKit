using System;
using GameKit.Navigation.Screens.Core;
using GameKit.Navigation.Screens.Core.Internal;
using GameKit.Navigation.Screens.Page;
using GameKit.Navigation.Screens.Page.Internal;
using VContainer;
using VContainer.Unity;

namespace GameKit.Navigation.VContainer
{
    public class ScreenBuilder<TLayer> where TLayer : IScreen
    {
        protected readonly IContainerBuilder Builder;

        public ScreenBuilder(IContainerBuilder builder)
        {
            Builder = builder;
        }

        public void InMemory<T>(string id, T instance) where T : TLayer
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Screen ID cannot be null or empty.", nameof(id));
            }

            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance), "Screen instance cannot be null.");
            }

            Builder.RegisterBuildCallback(container =>
            {
                var registry = container.Resolve<ScreenRegistry<TLayer>>();
                registry.AddScreen(id, instance);
            });
        }

        public void InMemory<T>(string id) where T : TLayer
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Screen ID cannot be null or empty.", nameof(id));
            }

            Builder.Register<T>(Lifetime.Scoped).AsSelf();
            Builder.RegisterBuildCallback(container =>
            {
                var registry = container.Resolve<ScreenRegistry<TLayer>>();
                var page = container.Resolve<T>();
                registry.AddScreen(id, page);
            });
        }

        public void InHierarchy<T>(string id) where T : TLayer
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Screen ID cannot be null or empty.", nameof(id));
            }

            Builder.RegisterComponentInHierarchy<T>();
            Builder.RegisterBuildCallback(container =>
            {
                var registry = container.Resolve<ScreenRegistry<TLayer>>();
                var page = container.Resolve<T>();
                registry.AddScreen(id, page);
            });
        }

        public void InAddressable<T>(string id, string key) where T : TLayer
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Page ID cannot be null or empty.", nameof(id));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Addressable key cannot be null or empty.", nameof(key));
            }

            Builder.RegisterBuildCallback(container =>
            {
                var registry = container.Resolve<ScreenRegistry<TLayer>>();
                registry.AddScreen<T>(id, key);
            });
        }

        public void InAddressable<T, TParent>(string id, string key)
            where T : TLayer
            where TParent : IParentProvider
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Addressable key cannot be null or empty.", nameof(key));
            }

            if (!Builder.Exists(typeof(T)))
            {
                Builder.RegisterComponentInHierarchy<TParent>();
            }

            Builder.RegisterBuildCallback(container =>
            {
                var registry = container.Resolve<ScreenRegistry<TLayer>>();
                var parentProvider = container.Resolve<TParent>();
                registry.AddScreen<T>(id, key, parentProvider.Parent);
            });
        }
    }
}