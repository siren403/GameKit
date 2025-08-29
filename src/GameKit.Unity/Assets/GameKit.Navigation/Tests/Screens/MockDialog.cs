using Cysharp.Threading.Tasks;
using GameKit.Navigation.Screens.Core.Commands;
using GameKit.Navigation.Screens.Dialog;
using UnityEngine;
using VitalRouter;

namespace GameKit.Navigation.Tests.Screens
{
    [Routes]
    public partial class MockDialog : IDialog
    {
        public const string Confirm = "ConfirmDialog";
        public const string Alert = "AlertDialog";
        public const string Loading = "LoadingDialog";

        public string Id { get; set; }
        public bool IsVisible { get; set; }

        [Route]
        private async UniTask On(ShowCommand command, PublishContext context)
        {
            IsVisible = true;
            Debug.Log($"Showing dialog: {Id}");
            await UniTask.DelayFrame(3, cancellationToken: context.CancellationToken);
        }

        [Route]
        private async UniTask On(HideCommand command)
        {
            Debug.Log($"Hiding dialog: {Id}");
            await UniTask.DelayFrame(3);
            IsVisible = false;
        }
    }
}