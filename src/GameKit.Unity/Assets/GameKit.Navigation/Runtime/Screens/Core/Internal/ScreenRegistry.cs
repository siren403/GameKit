using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameKit.Common.Results;
using GameKit.Navigation.Screens.Core.Commands;
using Microsoft.Extensions.Logging;
using UnityEngine;
using VitalRouter;

namespace GameKit.Navigation.Screens.Core.Internal
{
    internal class ScreenRegistry<TScreen> : IDisposable where TScreen : IScreen
    {
        private readonly ILogger<ScreenRegistry<TScreen>> _logger;
        private readonly string? _name;

        /// <summary>
        /// Id: IPage
        /// </summary>
        private readonly Dictionary<string, ScreenEntry<TScreen>> _cachedScreens = new();

        /// <summary>
        /// Id: Addressable Key
        /// </summary>
        private readonly Dictionary<string, IScreenFactory<TScreen>> _screenFactories = new();

        public IEnumerable<ScreenEntry<TScreen>> CachedScreens => _cachedScreens.Values;

        public ScreenRegistry(ILogger<ScreenRegistry<TScreen>> logger, string? name = null)
        {
            _logger = logger;
            _name = name;
#if UNITY_EDITOR
            Debug.Log($"PageRegistry({name}) created");
#endif
        }

        public void AddScreen(string id, TScreen screen)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogDebug(
                    "{name}.{op}: {id} cannot be null or empty.",
                    typeof(TScreen).Name, nameof(AddScreen), id
                );
                return;
            }

            if (_cachedScreens.ContainsKey(id))
            {
                _logger.LogDebug(
                    "{name}.{op}: Screen with ID '{id}' already exists in the registry.",
                    typeof(TScreen).Name, nameof(AddScreen), id
                );
                return;
            }

            var router = new Router(CommandOrdering.Drop);
            screen.MapTo(router);
            var entry = new ScreenEntry<TScreen>(
                id,
                screen,
                router
            );
            _cachedScreens[id] = entry;
        }

        public void AddScreen<T>(string id, string key, Transform? parent = null) where T : TScreen
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogDebug("Page.AddPage: {pageId} cannot be null or empty.", id);
                return;
            }

            if (string.IsNullOrEmpty(key))
            {
                _logger.LogDebug("Page.AddPage: {key} cannot be null or empty.", key);
                return;
            }

            var factory = new AddressableScreenFactory<TScreen, T>(key)
            {
                Parent = parent
            };
            if (_screenFactories.TryAdd(id, factory))
            {
                return;
            }

            _logger.LogDebug(
                "Page.AddPage: Page with ID '{pageId}' already exists in the registry.",
                id
            );
        }

        public async UniTask<FastResult<ScreenEntry<TScreen>>> GetScreenAsync(string id)
        {
            if (_cachedScreens.TryGetValue(id, out var cached))
            {
                return FastResult<ScreenEntry<TScreen>>.Ok(cached);
            }

            if (!_screenFactories.TryGetValue(id, out var factory))
            {
                return FastResult<ScreenEntry<TScreen>>.Fail(
                    "Screen.GetScreen",
                    $"Screen with ID '{id}' not found in registry."
                );
            }

            var result = await factory.InstantiateAsync();
            if (result.IsError(out FastResult<ScreenEntry<TScreen>> fail))
            {
                return fail;
            }

            var screen = result.Value;
            var router = new Router(CommandOrdering.Drop);
            screen.MapTo(router);
            var entry = new ScreenEntry<TScreen>(
                id,
                screen,
                router
            );
            _cachedScreens[id] = entry;
            return FastResult<ScreenEntry<TScreen>>.Ok(entry);
        }

        public async UniTask<FastResult<ScreenEntry<TScreen>>> GetScreenWithErrorAsync(string id, Router router)
        {
            var result = await GetScreenAsync(id);
            if (result.IsError)
            {
                _ = router.PublishAsync(new ScreenErrorCommand(
                    id,
                    ScreenOperation.Get,
                    ScreenErrorCodes.NotFound,
                    $"Screen '{id}' not found.\nReason: {result.FirstError}"
                ));
            }

            return result;
        }

        public void Dispose()
        {
            foreach (var entry in _cachedScreens.Values)
            {
                entry.Dispose();
            }

            _cachedScreens.Clear();
            _screenFactories.Clear();
        }
    }
}