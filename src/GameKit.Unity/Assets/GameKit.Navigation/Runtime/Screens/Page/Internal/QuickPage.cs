using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameKit.Navigation.Screens.Page.Extensions;
using R3;
using UnityEngine;
using VitalRouter;

namespace GameKit.Navigation.Screens.Page.Internal
{
    internal class QuickPage<TPage, TProps> : IQuickPage<TPage, TProps>
        where TPage : IPage, IPageProps<TProps>
    {
        private readonly Router _router;
        private readonly string _pageId;
        private readonly PageRegistry _registry;
        private readonly PagePresenter _presenter;
        private readonly BindingContext _context;

        public QuickPage(
            Router router,
            string pageId,
            PageRegistry registry,
            PagePresenter presenter
        )
        {
            _router = router;
            _pageId = pageId;
            _registry = registry;
            _presenter = presenter;
            _context = new BindingContext(_router);
        }

        public async UniTask PushAsync(TProps props, CancellationToken ct = default)
        {
            var pageResult = await _registry.GetPageAsync(_pageId);
            if (pageResult.IsError)
            {
                return;
            }

            var (_, page, _) = pageResult.Value;
            if (page is TPage typedPage)
            {
                typedPage.Props = props;
            }
#if UNITY_EDITOR
            else
            {
                Debug.LogWarning($"Page is not of type {typeof(TPage).Name}");
            }
#endif
            await _router.PushPageAsync(_pageId, ct: ct);
        }

        public async UniTask PushAsync<TState>(
            TProps props,
            TState state,
            Action<TState, TPage, IBindingContext> binding,
            CancellationToken ct = default
        )
        {
            var pageResult = await _registry.GetPageAsync(_pageId);
            if (pageResult.IsError)
            {
                return;
            }

            var (_, page, _) = pageResult.Value;
            if (page is not TPage typedPage)
            {
#if UNITY_EDITOR
                Debug.LogError($"Page binding failed. page is not of type {typeof(TPage).Name}");
#endif
                return;
            }

            typedPage.Props = props;
            _context.Initialize();
            using (_context)
            {
                binding.Invoke(state, typedPage, _context);
                await _router.PushPageAsync(_pageId, ct: ct);
                await UniTask.WaitWhile((_presenter, _pageId), static state =>
                {
                    var (presenter, pageId) = state;
                    return presenter.IsRendering(pageId);
                }, cancellationToken: ct);
            }
        }
    }

    internal class BindingContext : IDisposable, IBindingContext
    {
        private readonly Router _router;
        private DisposableBag _disposable;
        private readonly HashSet<object> _binds = new();

        private bool _isDisposed = true;

        private BindingContext()
        {
        }

        public void Initialize()
        {
            if (!_isDisposed)
            {
                throw new InvalidOperationException("This context is already initialized.");
            }

            _isDisposed = false;
            _disposable = new DisposableBag();
        }

        public BindingContext(Router router)
        {
            _router = router;
        }

        public void OnBack<T>(Observable<T> source)
        {
            if (_binds.Contains(source))
            {
                throw new InvalidOperationException("This source is already bound.");
            }

            source.SubscribeAwait(
                _router,
                static (_, router, ct) => router.BackPageAsync(ct: ct),
                AwaitOperation.Drop
            ).AddTo(ref _disposable);
            _binds.Add(source);
        }

        public void OnClick<T, TState>(
            Observable<T> source,
            TState state,
            Func<T, TState, CancellationToken, ValueTask> action
        )
        {
            if (_binds.Contains(source))
            {
                throw new InvalidOperationException("This source is already bound.");
            }

            source.SubscribeAwait(
                state,
                action,
                AwaitOperation.Drop
            ).AddTo(ref _disposable);
            _binds.Add(source);
        }

        public void OnClick<T>(Observable<T> source, Func<T, CancellationToken, ValueTask> action)
        {
            if (_binds.Contains(source))
            {
                throw new InvalidOperationException("This source is already bound.");
            }

            source.SubscribeAwait(
                action,
                AwaitOperation.Drop
            ).AddTo(ref _disposable);
            _binds.Add(source);
        }

        public void Dispose()
        {
            _isDisposed = true;
            _binds.Clear();
            _disposable.Dispose();
        }
    }
}