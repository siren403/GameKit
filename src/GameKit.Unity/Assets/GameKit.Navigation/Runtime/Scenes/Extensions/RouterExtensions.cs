using System;
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
            router.PublishAsync(command);
        }
    }
}