using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Assets;
using GameKit.Navigation.Scenes.Commands;
using GameKit.Navigation.Screens.Core;
using GameKit.Navigation.Screens.Dialog;
using GameKit.Navigation.Screens.Dialog.Commands;
using GameKit.Navigation.Screens.Page;
using GameKit.Navigation.Screens.Page.Commands;
using GameKit.Navigation.Screens.Page.Extensions;
using GameKit.Navigation.Screens.Sessions.Dialog;
using R3;
using Samples.Navigation.SceneOverview.Dialogs;
using Samples.Navigation.SceneOverview.Pages;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer.Unity;
using VitalRouter;

namespace Samples.Navigation.SceneOverview.Scenes
{
    // https://docs.unity3d.com/Packages/com.unity.addressables@2.6/manual/remote-content-assetbundle-cache.html
    // 참조 되지 않는 캐시 항목 제거
    // var result = await Addressables.CleanBundleCache().Task.AsUniTask();
    // Debug.Log($"Addressables.CleanBundleCache: {result}");
    public class IntroEntryPoint : IInitializable, IStartable
    {
        private readonly Router _router;
        private readonly InitPage _initPage;
        private readonly IScreenLauncher<ErrorPage, string> _errorPage;
        private readonly DownloadPage _downloadPage;
        private readonly IQuestionSession<ConfirmDialog, bool> _confirmQuestion;

        public IntroEntryPoint(
            Router router,
            InitPage initPage,
            IScreenLauncher<ErrorPage, string> errorPage,
            DownloadPage downloadPage,
            IQuestionSession<ConfirmDialog, bool> confirmQuestion
        )
        {
            _router = router;
            _initPage = initPage;
            _errorPage = errorPage;
            _downloadPage = downloadPage;
            _confirmQuestion = confirmQuestion;
        }

        private void PushErrorPage(string message)
        {
            _errorPage.PublishAsync(new PushPageCommand(nameof(ErrorPage)), message, static (page, context) =>
            {
                context.Subscribe(page.OnClickBack, new BackPageCommand());
                context.Subscribe(page.OnClickCopy, async static (msg, ct) =>
                {
                    Debug.Log(msg);
                    GUIUtility.systemCopyBuffer = msg;
                    await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: ct);
                });
            });
        }


        public void Initialize()
        {
            _initPage.OnClickInit.SubscribeAwait(async (_, ct) =>
            {
                var isConfirmed = await _confirmQuestion.ShowAsync(static (dialog, answer) =>
                {
                    dialog.Message = "Start initialization?"; 
                    answer.Yes(dialog.OnClickConfirm);
                    answer.No(dialog.OnClickCancel, dialog.OnClickScrim);
                }, ct);
                if (!isConfirmed)
                {
                    Debug.Log("Initialization cancelled by user.");
                    return;
                }

                Debug.Log("Initializing Addressables...");
                var initResult = await AddressableOperations.InitializeAsync(ct);
                if (initResult.IsError)
                {
                    Debug.LogError($"Failed to initialize Addressables: {initResult}");
                    return;
                }

                var catalogResult = await AddressableOperations.CheckCatalog(ct);
                if (catalogResult.IsError)
                {
                    Debug.LogError($"Failed to check catalog: {catalogResult}");
                    PushErrorPage(catalogResult.ToString());
                    return;
                }

                var planTask = ToScenePlanCommand.CreateUsingDownloadManifestAsync("/title", ct: ct);
                var planResult = await planTask;
                if (planResult.IsError)
                {
                    PushErrorPage(catalogResult.ToString());
                    return;
                }

                var plan = planResult.Value;
                if (plan.Manifest.IsDownloaded)
                {
                    NextScene(plan);
                }
                else
                {
                    ToDownloadPage(plan).Forget();
                }
            }, AwaitOperation.Drop);

            return;

            async UniTaskVoid ToDownloadPage(ToScenePlanCommand command)
            {
                _downloadPage.Message = $"Download {command.Label}? {command.Manifest.Size}";
                _downloadPage.Progress = 0;
                await _router.ToPageAsync(nameof(DownloadPage));
                await _downloadPage.OnClickDownload.FirstAsync();
                _downloadPage.Message = "Downloading...";

                var planResult =
                    await ToScenePlanCommand.ToDownloadLocationsAsync(command, new DownloadLogger(_downloadPage));
                if (planResult.IsError)
                {
                    _downloadPage.Message = planResult.ToString();
                    return;
                }

                _downloadPage.Message = "Completed download.";
                await UniTask.Delay(1000); // 잠시 대기 후 페이지 닫기

                NextScene(planResult.Value);
            }

            void NextScene(ToScenePlanCommand command)
            {
                _router.PublishAsync(command);
            }
        }

        class DownloadLogger : IProgress<DownloadStatus>
        {
            private readonly DownloadPage _page;

            public DownloadLogger(DownloadPage page)
            {
                _page = page;
            }

            public void Report(DownloadStatus value)
            {
                ByteSize downloaded = value.DownloadedBytes;
                ByteSize total = value.TotalBytes;
                Debug.Log($"Download status: {downloaded}/{total} ({value.Percent:P2})");
                _page.Progress = value.Percent;
            }
        }

        public void Start()
        {
            _router.ToPage(nameof(InitPage));
        }
    }
}