using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameKit.Navigation.Scenes.Commands;
using VitalRouter;

namespace GameKit.Navigation.Scenes.Extensions
{
    public static class RouterExtensions
    {
        public static UniTaskVoid ToScene(this Router router, string label)
        {
            if (router == null)
            {
                throw new ArgumentNullException(nameof(router));
            }

            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentException("Label cannot be null or empty.", nameof(label));
            }

            router.PublishAsync(new ToSceneCommand { Label = label });
            return new UniTaskVoid();
        }

        public static ValueTask ToSceneAsync(this Router router, string label, CancellationToken ct = default)
        {
            if (router == null)
            {
                throw new ArgumentNullException(nameof(router));
            }

            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentException("Label cannot be null or empty.", nameof(label));
            }

            return router.PublishAsync(new ToSceneCommand { Label = label }, ct);
        }
    }
}