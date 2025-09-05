using System;
using GameKit.Navigation.Screens.Core;
using GameKit.Navigation.Screens.Core.Internal;
using GameKit.Navigation.Screens.Page;
using GameKit.Navigation.Screens.Page.Internal;
using VContainer;
using VContainer.Unity;

namespace GameKit.Navigation.VContainer
{
    public sealed class ScreenBuilder<TLayer> where TLayer : IScreen
    {
        private readonly IContainerBuilder _builder;

        public ScreenBuilder(IContainerBuilder builder)
        {
            _builder = builder;
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

            _builder.RegisterBuildCallback(container =>
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

            _builder.Register<T>(Lifetime.Scoped).AsSelf();
            _builder.RegisterBuildCallback(container =>
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

            _builder.RegisterComponentInHierarchy<T>();
            _builder.RegisterBuildCallback(container =>
            {
                var registry = container.Resolve<ScreenRegistry<TLayer>>();
                var page = container.Resolve<T>();
                registry.AddScreen(id, page);
            });
        }

        // public void InHierarchy<T, TProps>(string id) where T : TScreen, IPageProps<TProps>
        // {
        //     InHierarchy<T>(id);
        //     _builder.Register<QuickPage<T, TProps>>(Lifetime.Singleton)
        //         .As<IQuickPage<T, TProps>>()
        //         .WithParameter(id);
        // }

        public void InHierarchyWithLauncher<T, TProps>(string id) where T : TLayer, IScreenProps<TProps>
        {
            InHierarchy<T>(id);
            _builder.Register<ScreenLauncher<T, TLayer, TProps>>(Lifetime.Singleton)
                .As<IScreenLauncher<T, TProps>>()
                .WithParameter(id);
            _builder.RegisterBuildCallback(container =>
            {
                var launcher = container.Resolve<IScreenLauncher<T, TProps>>();
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

            _builder.RegisterBuildCallback(container =>
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

            if (!_builder.Exists(typeof(T)))
            {
                _builder.RegisterComponentInHierarchy<TParent>();
            }

            _builder.RegisterBuildCallback(container =>
            {
                var registry = container.Resolve<ScreenRegistry<TLayer>>();
                var parentProvider = container.Resolve<TParent>();
                registry.AddScreen<T>(id, key, parentProvider.Parent);
            });
        }
    }
}