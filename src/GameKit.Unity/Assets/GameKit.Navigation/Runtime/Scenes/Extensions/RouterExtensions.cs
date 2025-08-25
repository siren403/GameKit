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
        public static void ToScene(this Router router, string label)
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
        }

        public static void ToScene(this Router router, ToScenePlanCommand command)
        {
            if (router == null)
            {
                throw new ArgumentNullException(nameof(router));
            }

            if (command.Label == null)
            {
                throw new ArgumentException("Label cannot be null.", nameof(command.Label));
            }

            router.PublishAsync(command);
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