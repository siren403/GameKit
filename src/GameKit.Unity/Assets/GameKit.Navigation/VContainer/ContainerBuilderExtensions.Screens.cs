using System;
using GameKit.Logger.VContainer;
using GameKit.Navigation.Screens.Core;
using GameKit.Navigation.Screens.Core.Internal;
using GameKit.Navigation.Screens.Dialog;
using GameKit.Navigation.Screens.Dialog.Internal;
using GameKit.Navigation.Screens.Page;
using GameKit.Navigation.Screens.Page.Internal;
using VContainer;
using VitalRouter.VContainer;

namespace GameKit.Navigation.VContainer
{
    public static partial class ContainerBuilderExtensions
    {
        public static void RegisterPages(
            this IContainerBuilder builder,
            Action<ScreenBuilder<IPage>> configuration,
            string? name = null
        )
        {
            builder.RegisterStackNavigator<PageNavigator, IPage, PageStack>(configuration, name);
        }

        public static void RegisterDialogs(
            this IContainerBuilder builder,
            Action<ScreenBuilder<IDialog>> configuration,
            string? name = null
        )
        {
            builder.RegisterStackNavigator<DialogNavigator, IDialog, DialogStack>(configuration, name);
        }

        private static void RegisterStackNavigator<T, TScreen, TStack>(
            this IContainerBuilder builder,
            Action<ScreenBuilder<TScreen>> configuration,
            string? name = null
        )
            where TScreen : IScreen
            where TStack : ScreenStack
        {
            builder.Register<ScreenRegistry<TScreen>>(Lifetime.Scoped).WithParameter(name);
            builder.Register<ScreenPresenter<TScreen>>(Lifetime.Scoped);
            builder.Register<TStack>(Lifetime.Scoped);

            builder.RegisterVitalRouter(routing => routing.Map<T>());

            var pages = new ScreenBuilder<TScreen>(builder);
            configuration.Invoke(pages);

            builder.RegisterBuildCallback(container =>
            {
                var presenter = container.Resolve<ScreenPresenter<TScreen>>();
                var registry = container.Resolve<ScreenRegistry<TScreen>>();
                presenter.Initialize(registry.CachedScreens);
            });

            builder.RegisterLogger();
        }
    }
}