using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameKit.Common.Results;
using Microsoft.Extensions.Logging;
using UnityEngine;
using VitalRouter;

namespace GameKit.Navigation.Screens.Page.Internal
{
    internal class PageRegistry : IDisposable
    {
        private readonly ILogger<PageRegistry> _logger;

        /// <summary>
        /// Id: IPage
        /// </summary>
        private readonly Dictionary<string, PageEntry> _cachedPages = new();

        /// <summary>
        /// Id: Addressable Key
        /// </summary>
        private readonly Dictionary<string, IPageFactory> _pageFactories = new();

        public IEnumerable<PageEntry> CachedPages => _cachedPages.Values;

        public PageRegistry(ILogger<PageRegistry> logger)
        {
            _logger = logger;
        }

        public void AddPage(string pageId, IPage page)
        {
            if (string.IsNullOrEmpty(pageId))
            {
                _logger.LogDebug("Page.AddPage: {pageId} cannot be null or empty.", pageId);
                return;
            }

            if (_cachedPages.ContainsKey(pageId))
            {
                _logger.LogDebug("Page.AddPage: Page with ID '{pageId}' already exists in the registry.", pageId);
                return;
            }

            var router = new Router(CommandOrdering.Drop);
            page.MapTo(router);
            var entry = new PageEntry(
                pageId,
                page,
                router
            );
            _cachedPages[pageId] = entry;
        }

        public void AddPage<T>(string pageId, string key, Transform parent = null) where T : IPage
        {
            if (string.IsNullOrEmpty(pageId))
            {
                _logger.LogDebug("Page.AddPage: {pageId} cannot be null or empty.", pageId);
                return;
            }

            if (string.IsNullOrEmpty(key))
            {
                _logger.LogDebug("Page.AddPage: {key} cannot be null or empty.", key);
                return;
            }

            var factory = new PageFactory<T>(key)
            {
                Parent = parent
            };

            if (_pageFactories.TryAdd(pageId, factory))
            {
                return;
            }

            _logger.LogDebug(
                "Page.AddPage: Page with ID '{pageId}' already exists in the registry.",
                pageId
            );
        }

        public async UniTask<FastResult<PageEntry>> GetPageAsync(string pageId)
        {
            if (_cachedPages.TryGetValue(pageId, out var cached))
            {
                return FastResult<PageEntry>.Ok(cached);
            }

            if (!_pageFactories.TryGetValue(pageId, out var factory))
            {
                return FastResult<PageEntry>.Fail(
                    "Page.GetPageAsync",
                    $"Page with ID '{pageId}' not found in registry."
                );
            }

            var result = await factory.InstantiateAsync();
            if (result.IsError(out FastResult<PageEntry> fail))
            {
                return fail;
            }

            var page = result.Value;
            var router = new Router(CommandOrdering.Drop);
            page.MapTo(router);
            var entry = new PageEntry(
                pageId,
                page,
                router
            );
            _cachedPages[pageId] = entry;
            return FastResult<PageEntry>.Ok(entry);
        }

        public void Dispose()
        {
            foreach (var entry in _cachedPages.Values)
            {
                entry.Dispose();
            }

            _cachedPages.Clear();
            _pageFactories.Clear();
        }
    }
}