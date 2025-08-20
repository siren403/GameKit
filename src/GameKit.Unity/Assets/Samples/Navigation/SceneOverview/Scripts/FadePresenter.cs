using Cysharp.Threading.Tasks;
using GameKit.Navigation.Scenes.Commands;
using UnityEngine;
using VitalRouter;

namespace Samples.Navigation.SceneOverview
{
    [Routes(CommandOrdering.Drop)]
    public partial class FadePresenter
    {
        private readonly FadeCanvas _fade;

        public FadePresenter(FadeCanvas fade)
        {
            _fade = fade;
        }

        [Route]
        private async UniTask On(TransitionStartedCommand command, PublishContext context)
        {
            Debug.Log($"In: {command.Path}");
            await _fade.InAsync(ct: context.CancellationToken);
        }

        [Route]
        private async UniTask On(TransitionEndedCommand command, PublishContext context)
        {
            Debug.Log($"Out: {command.Path}");
            await _fade.OutAsync(ct: context.CancellationToken);
        }
    }
}