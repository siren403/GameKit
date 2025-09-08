using System;
using GameKit.Navigation.Screens.Page;
using GameKit.Navigation.Screens.Sessions.Page;
using GameKit.Navigation.Screens.Sessions.Page.Internal;
using VContainer;

namespace GameKit.Navigation.VContainer
{
    public class PageBuilder : ScreenBuilder<IPage>
    {
        public PageBuilder(IContainerBuilder builder) : base(builder)
        {
        }

        public void SessionInHierarchy<TPage>(string id) where TPage : IPage
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Page ID cannot be null or empty.", nameof(id));
            }

            InHierarchy<TPage>(id);
            Builder.Register<PageSession<TPage>>(Lifetime.Scoped)
                .As<IPageSession<TPage>>()
                .WithParameter(id);

            Builder.RegisterBuildCallback(static container =>
            {
                _ = container.Resolve<IPageSession<TPage>>(); //
            });
        }
    }
}