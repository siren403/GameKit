using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Common.Results;
using GameKit.Navigation.Screens.Page.Commands;
using GameKit.Navigation.Screens.Page.Errors;
using UnityEngine;
using Void = GameKit.Common.Results.Void;

namespace GameKit.Navigation.Screens.Page.Internal
{
    internal class PagePresenter
    {
        private readonly HashSet<string> _rendering = new();

        public void Initialize(IEnumerable<PageEntry> pages)
        {
            foreach (var (_, page, _) in pages)
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
        public async UniTask<FastResult<Void>> ShowPageAsync(
            PageEntry entry,
            CancellationToken ct = default
        )
        {
            var (id, page, router) = entry;
            try
            {
                await router.PublishAsync(new ShowCommand(), ct);
                _rendering.Add(id);
                return FastResult.Ok;
            }
            catch
            {
                _ = router.PublishAsync(new PageErrorCommand(
                    pageId: id,
                    operation: PageOperation.None,
                    errorCode: PageErrorCodes.ShowFailed,
                    message: $"Failed to show page '{id}'"
                ), ct);
                page.IsVisible = false;
                return FastResult<Void>.Fail(PageErrorCodes.ShowFailed, $"Failed to show page '{id}'");
            }
        }

        /// <summary>
        /// Hide는 안보이게 되는 상태이나 예외가 발생해도 최종적으로 안보이게 처리.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="ct"></param>
        public async UniTask HidePage(PageEntry entry, CancellationToken ct = default)
        {
            var (id, page, router) = entry;
            if (_rendering.Remove(id))
            {
                try
                {
                    await router.PublishAsync(new HideCommand(), ct);
                }
                catch (Exception e)
                {
                    page.IsVisible = false;
                    _ = router.PublishAsync(new PageErrorCommand(
                        pageId: id,
                        operation: PageOperation.None,
                        errorCode: PageErrorCodes.HideFailed,
                        message: $"Failed to hide page '{id}': {e.Message}"
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