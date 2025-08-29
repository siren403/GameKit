using GameKit.Navigation.Screens.Core.Commands;
using VitalRouter;

namespace GameKit.Navigation.Screens.Core.Internal
{
    internal static class ScreenExtensions
    {
        public static bool AlreadyCurrentScreen(this ScreenStack stack, string id, Router router)
        {
            var result = stack.TryPeek(out var currentScreenId) && currentScreenId == id;

            if (result)
            {
                _ = router.PublishAsync(new ScreenErrorCommand(
                    id,
                    ScreenOperation.None,
                    ScreenErrorCodes.AlreadyCurrent,
                    $"Already on screen '{id}'"
                ));
            }

            return result;
        }
    }
}