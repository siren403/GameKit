using System;
using Cysharp.Threading.Tasks;
using GameKit.Navigation.Scenes.Commands;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;
using VitalRouter;

namespace Samples.Navigation.SceneOverview
{
    [Routes(CommandOrdering.Drop)]
    public partial class TransitionPresenter : IInitializable
    {
        private readonly Image _image;
        private readonly Slider _slider;
        private readonly RectTransform _transform;

        public TransitionPresenter(Image image, Slider slider)
        {
            _image = image;
            _slider = slider;
            _transform = _image.GetComponent<RectTransform>();
        }

        public void Initialize()
        {
            var height = _transform.rect.height;
            _transform.pivot = new Vector2(0.5f, 0);
            _transform.anchoredPosition = new Vector2(0, height);
        }

        [Route]
        private async UniTask On(TransitionStartedCommand command, PublishContext context)
        {
            var height = _transform.rect.height;
            await LMotion.Create(height, 0, 0.3f)
                .WithEase(Ease.OutCirc)
                .Bind(_transform, static (value, state) => { state.anchoredPosition = new Vector2(0, value); })
                .ToUniTask(context.CancellationToken);
        }

        [Route]
        private void On(TransitionProgressCommand command)
        {
            Debug.Log(command);
            _slider.value = command.Progress;
        }

        [Route]
        private async UniTask On(TransitionEndedCommand command, PublishContext context)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(5), cancellationToken: context.CancellationToken);
            var height = _transform.rect.height;
            await LMotion.Create(0, height, 0.3f)
                .WithEase(Ease.OutCirc)
                .Bind(_transform, static (value, state) => { state.anchoredPosition = new Vector2(0, value); })
                .ToUniTask(context.CancellationToken);
        }
    }
}