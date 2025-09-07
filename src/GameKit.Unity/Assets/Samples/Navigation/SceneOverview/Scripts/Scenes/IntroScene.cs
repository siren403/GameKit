using GameKit.Navigation.VContainer;
using Samples.Navigation.SceneOverview.Dialogs;
using Samples.Navigation.SceneOverview.Pages;
using VContainer;
using VContainer.Unity;

namespace Samples.Navigation.SceneOverview.Scenes
{
    public class IntroScene : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<IntroEntryPoint>();
            builder.RegisterPages(pages =>
            {
                pages.InHierarchy<InitPage>(nameof(InitPage));
                pages.InHierarchy<DownloadPage>(nameof(DownloadPage));
                pages.InHierarchyWithLauncher<ErrorPage, string>(nameof(ErrorPage));
            }, nameof(IntroScene));

            builder.RegisterDialogs(dialogs =>
            {
                dialogs.QuestionInHierarchy<ConfirmDialog, bool>(nameof(ConfirmDialog));
            }, nameof(IntroScene));
        }
    }
}