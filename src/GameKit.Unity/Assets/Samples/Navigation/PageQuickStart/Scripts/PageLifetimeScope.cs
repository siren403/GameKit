using System;
using GameKit.Navigation.Screens.Page.Commands;
using GameKit.Navigation.Screens.Page.Extensions;
using GameKit.Navigation.Screens.Sessions.Page;
using GameKit.Navigation.VContainer;
using Samples.Navigation.PageQuickStart.Pages;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;
using VitalRouter;

namespace Samples.Navigation.PageQuickStart
{
    public class PageLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterPages(pages =>
            {
                pages.SessionInHierarchy<LoginPage>(PageIds.Login);
                pages.InHierarchy<HomePage>(PageIds.Home);
                pages.InAddressable<SettingsPage, PageRoot>(
                    PageIds.Settings,
                    "Assets/Samples/Navigation/PageQuickStart/Prefabs/SettingsPage.prefab"
                );
            });

            builder.RegisterEntryPoint<EntryPoint>();
            builder.RegisterComponentInHierarchy<UIDocument>();
        }

        class EntryPoint : IInitializable
        {
            private readonly Router _router;
            private readonly UIDocument _document;
            private readonly IPageSession<LoginPage> _login;

            public EntryPoint(
                Router router,
                UIDocument document,
                IPageSession<LoginPage> login
            )
            {
                _router = router;
                _document = document;
                _login = login;
            }

            public void Initialize()
            {
                var root = _document.rootVisualElement;

                var loginTo = root.Q<Button>(classes: new[] { "login", "to" });
                var loginPush = root.Q<Button>(classes: new[] { "login", "push" });
                var loginReplace = root.Q<Button>(classes: new[] { "login", "replace" });
                loginTo.clicked += () => _login.PushAsync(static (page, binder) =>
                {
                    
                });
                loginPush.clicked += () => _router.PushPageAsync(PageIds.Login);
                loginReplace.clicked += () => _router.ReplacePageAsync(PageIds.Login);

                var homeTo = root.Q<Button>(classes: new[] { "home", "to" });
                var homePush = root.Q<Button>(classes: new[] { "home", "push" });
                var homeReplace = root.Q<Button>(classes: new[] { "home", "replace" });
                homeTo.clicked += () => _router.ToPageAsync(PageIds.Home);
                homePush.clicked += () => _router.PushPageAsync(PageIds.Home);
                homeReplace.clicked += () => _router.ReplacePageAsync(PageIds.Home);

                var settingsTo = root.Q<Button>(classes: new[] { "settings", "to" });
                var settingsPush = root.Q<Button>(classes: new[] { "settings", "push" });
                var settingsReplace = root.Q<Button>(classes: new[] { "settings", "replace" });
                settingsTo.clicked += () => _router.ToPageAsync(PageIds.Settings);
                settingsPush.clicked += () => _router.PushPageAsync(PageIds.Settings);
                settingsReplace.clicked += () => _router.ReplacePageAsync(PageIds.Settings);

                var back = root.Q<Button>(classes: new[] { "back" });
                back.clicked += () => _router.BackPageAsync();

                _router.Subscribe<PageErrorCommand>((command, context) => { Debug.Log(command); });
            }
        }
    }
}