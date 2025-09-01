using Cysharp.Threading.Tasks;
using GameKit.Navigation.Screens.Core.Internal;
using GameKit.Navigation.Screens.Page.Commands;
using GameKit.Navigation.Screens.Page.Errors;
using Microsoft.Extensions.Logging;
using VitalRouter;

namespace GameKit.Navigation.Screens.Page.Internal
{
    [Routes(CommandOrdering.Drop)]
    internal sealed partial class PageNavigator
    {
        private readonly ScreenPresenter<IPage> _presenter;
        private readonly ScreenRegistry<IPage> _registry;
        private readonly Router _router;
        private readonly ILogger<PageNavigator> _logger;

        private readonly PageStack _stack;

        public PageNavigator(
            ScreenPresenter<IPage> presenter,
            ScreenRegistry<IPage> registry,
            Router router,
            ILogger<PageNavigator> logger,
            PageStack stack
        )
        {
            _presenter = presenter;
            _registry = registry;
            _router = router;
            _logger = logger;
            _stack = stack;
        }

        public bool IsCurrentPage(string pageId)
        {
            return _presenter.IsRendering(pageId) && _stack.AlreadyCurrentScreen(pageId, _router);
        }


        [Route]
        private async UniTask On(ToPageCommand command, PublishContext context)
        {
            var newPageId = command.PageId;
            // 1. 중복 체크 (Navigator가 Stack 상태 확인)
            if (_stack.AlreadyCurrentScreen(newPageId, _router))
            {
                return;
            }

            var newPageResult = await _registry.GetScreenWithErrorAsync(newPageId, _router);
            if (newPageResult.IsError)
            {
                return;
            }

            var newPage = newPageResult.Value;

            // 3. 모든 기존 페이지 숨기기 + Stack 조작
            while (_stack.TryPop(out var id))
            {
                var popPageResult = await _registry.GetScreenAsync(id);
                if (popPageResult.IsError)
                {
                    continue;
                }

                var popPage = popPageResult.Value;
                _ = _presenter.HideScreenAsync(popPage);
            }

            // 4. 새 페이지 표시 + Stack에 추가
            var showResult = await _presenter.ShowScreenAsync(newPage, context.CancellationToken);
            if (showResult.IsError)
            {
                return;
            }

            _stack.Push(newPageId);
        }


        [Route]
        private async UniTask On(BackPageCommand command, PublishContext context)
        {
            if (_stack.TryPop(out var id))
            {
                var backPageResult = await _registry.GetScreenWithErrorAsync(id, _router);
                if (backPageResult.IsError)
                {
                    return;
                }

                var backPage = backPageResult.Value;
                _ = _presenter.HideScreenAsync(backPage);
            }
            else
            {
                _logger.LogDebug("No page to go back to.");
                return;
            }

            if (_stack.TryPeek(out var nextId))
            {
                var nextPageResult = await _registry.GetScreenWithErrorAsync(nextId, _router);
                if (nextPageResult.IsError)
                {
                    return;
                }

                var nextPage = nextPageResult.Value;
                var showResult = await _presenter.ShowScreenAsync(nextPage, context.CancellationToken);
                if (showResult.IsError)
                {
                    return;
                }
            }
            else
            {
                _logger.LogDebug("No next page to show after going back.");
            }
        }


        [Route]
        private async UniTask On(PushPageCommand command, PublishContext context)
        {
            var newPageId = command.PageId;

            if (_stack.AlreadyCurrentScreen(newPageId, _router))
            {
                return;
            }

            var newPageResult = await _registry.GetScreenWithErrorAsync(newPageId, _router);
            if (newPageResult.IsError)
            {
                return;
            }

            var newPage = newPageResult.Value;

            if (_stack.TryPeek(out var currentId))
            {
                if (currentId == newPageId)
                {
                    _logger.LogDebug("Cannot push the same page again.");
                    return;
                }

                var currentPageResult = await _registry.GetScreenWithErrorAsync(currentId, _router);
                if (currentPageResult.IsError)
                {
                    return;
                }

                var currentPage = currentPageResult.Value;
                _ = _presenter.HideScreenAsync(currentPage);
            }

            var showResult = await _presenter.ShowScreenAsync(newPage, context.CancellationToken);
            if (showResult.IsError)
            {
                return;
            }

            _stack.Push(newPageId);
        }

        [Route]
        private async UniTask On(ReplacePageCommand command, PublishContext context)
        {
            var newPageId = command.PageId;
            if (_stack.AlreadyCurrentScreen(newPageId, _router))
            {
                return;
            }

            var newPageResult = await _registry.GetScreenWithErrorAsync(newPageId, _router);
            if (newPageResult.IsError)
            {
                return;
            }

            var newPage = newPageResult.Value;

            if (_stack.TryPop(out var oldId))
            {
                var oldPageResult = await _registry.GetScreenWithErrorAsync(oldId, _router);
                if (oldPageResult.IsError)
                {
                    return;
                }

                var oldPage = oldPageResult.Value;
                _ = _presenter.HideScreenAsync(oldPage);
            }
            else
            {
                _logger.LogDebug("No page to replace.");
            }

            var showResult = await _presenter.ShowScreenAsync(newPage, context.CancellationToken);
            if (showResult.IsError)
            {
                return;
            }

            _stack.Push(newPageId);
        }
    }
}