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
            // TODO: 현재 씬의 동작이 멈춰야 함.
            Debug.Log($"In: {command.Label}");
            await _fade.InAsync(ct: context.CancellationToken);
        }

        [Route]
        private async UniTask On(TransitionEndedCommand command, PublishContext context)
        {
            Debug.Log($"Out: {command.Label}");
            await _fade.OutAsync(ct: context.CancellationToken);
            // TODO: 이떄부터 로드된 씬이 동작해야 함.
        }
    }
}