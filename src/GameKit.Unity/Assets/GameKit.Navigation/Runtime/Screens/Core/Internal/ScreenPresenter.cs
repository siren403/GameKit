using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Common.Results;
using GameKit.Navigation.Screens.Core.Commands;
using Void = GameKit.Common.Results.Void;

namespace GameKit.Navigation.Screens.Core.Internal
{
    internal class ScreenPresenter<TScreen> where TScreen : IScreen
    {
        private readonly HashSet<string> _rendering = new();

        public void Initialize(IEnumerable<ScreenEntry<TScreen>> screens)
        {
            foreach (var (_, page, _) in screens)
            {
                page.IsVisible = false;
            }

            _rendering.Clear();
        }

        /// <summary>
        /// Show는 보이게 되는 처리 중 예외가 발생하면 비정상적으로
        /// 보일 수도 있으므로 실패 후 안보이게 처리. 
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async UniTask<FastResult<Void>> ShowScreenAsync(
            ScreenEntry<TScreen> entry,
            CancellationToken ct = default
        )
        {
            var (id, screen, router) = entry;
            try
            {
                await router.PublishAsync(new ShowCommand(), ct);
                _rendering.Add(id);
                return FastResult.Ok;
            }
            catch (Exception e)
            {
                screen.IsVisible = false;
                _ = router.PublishAsync(new ScreenErrorCommand(
                    id,
                    ScreenOperation.Show,
                    ScreenErrorCodes.ShowFailed,
                    $"Exception during showing screen: {e.Message}"
                ), ct);
                return FastResult<Void>.Fail(
                    ScreenErrorCodes.ShowFailed,
                    $"Exception during showing screen: {e.Message}"
                );
            }
        }


        /// <summary>
        /// Hide는 안보이게 되는 상태이나 예외가 발생해도 최종적으로 안보이게 처리.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="ct"></param>
        public async UniTask HideScreenAsync(ScreenEntry<TScreen> entry, CancellationToken ct = default)
        {
            var (id, screen, router) = entry;
            if (_rendering.Remove(id))
            {
                try
                {
                    await router.PublishAsync(new HideCommand(), ct);
                }
                catch (Exception e)
                {
                    screen.IsVisible = false;
                    _ = router.PublishAsync(new ScreenErrorCommand(
                        id,
                        ScreenOperation.Hide,
                        ScreenErrorCodes.HideFailed,
                        $"Exception during hiding screen: {e.Message}"
                    ), ct);
                }
            }
        }

        internal bool IsRendering(string id)
        {
            return _rendering.Contains(id);
        }
    }
}