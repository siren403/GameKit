using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Assets;
using GameKit.Common.Results;
using UnityEngine.ResourceManagement.ResourceLocations;
using Void = GameKit.Common.Results.Void;

namespace GameKit.Navigation.Scenes.Commands
{
    public struct ToScenePlanBuilder
    {
        private readonly string _label;
        private Func<CancellationToken, UniTask<FastResult<Void>>> _onCheckCatalog;
        private Func<string, CancellationToken, UniTask<FastResult<(ByteSize, IList<IResourceLocation>)>>>
            _onGetDownloadSize;
        private Func<string, CancellationToken, UniTask<FastResult<IResourceLocation>>> _onAppendTransition;

        public ToScenePlanBuilder(string label)
        {
            _label = label;
            _onCheckCatalog = null;
            _onGetDownloadSize = null;
            _onAppendTransition = null;
        }

        public CheckCatalogPhase CheckCatalog()
        {
            return new CheckCatalogPhase(ref this);
        }

        public struct CheckCatalogPhase
        {
            private ToScenePlanBuilder _builder;

            public CheckCatalogPhase(ref ToScenePlanBuilder builder)
            {
                _builder = builder;
                _builder._onCheckCatalog = static async (ct) =>
                {
                    var catalogResult = await AddressableOperations.CheckCatalog(ct);
                    if (catalogResult.Result.IsError(out FastResult<Void> fail))
                    {
                        return fail;
                    }

                    if (catalogResult.Value.Any())
                    {
                        return FastResult<Void>.Fail("Catalog.UpToDate");
                    }

                    return FastResult.Ok;
                };
            }

            public GetDownloadSizePhase GetDownloadSize<TState>(
                TState state,
                Func<TState, CancellationToken, UniTask<FastResult<Void>>> func
            )
            {
                return new GetDownloadSizePhase(ref _builder);
            }
        }

        public struct GetDownloadSizePhase
        {
            private ToScenePlanBuilder _builder;

            public GetDownloadSizePhase(ref ToScenePlanBuilder builder)
            {
                _builder = builder;
                _builder._onGetDownloadSize = static async (label, ct) =>
                {
                    var sizeResult = await AddressableOperations.GetDownloadSizeAsync(label, ct);
                    if (sizeResult.IsError(out FastResult<(ByteSize, IList<IResourceLocation>)> fail))
                    {
                        return fail;
                    }

                    var (size, locations) = sizeResult.Value;
                    if (!locations.Any())
                    {
                        return FastResult<(ByteSize, IList<IResourceLocation>)>.Fail("Download.EmptyLocations");
                    }

                    return FastResult<(ByteSize, IList<IResourceLocation>)>.Ok((size, locations));
                };
            }

            public AppendTransitionPhase AppendTransition()
            {
                return new AppendTransitionPhase(ref _builder);
            }
        }

        public struct AppendTransitionPhase
        {
            private ToScenePlanBuilder _builder;

            public AppendTransitionPhase(ref ToScenePlanBuilder builder)
            {
                _builder = builder;
                _builder._onAppendTransition = static async (label, ct) =>
                {
                    var transitionLabel = $"{label}:transition";
                    IResourceLocation transitionLocation = null;
                    var transitionSizeResult =
                        await AddressableOperations.GetDownloadSizeAsync(transitionLabel, ct);
                    if (!transitionSizeResult.IsError)
                    {
                        transitionLocation = transitionSizeResult.Value.Item2.FirstOrDefault();
                    }

                    if (transitionLocation is null)
                    {
                        transitionLabel = "/:transition";
                        transitionSizeResult =
                            await AddressableOperations.GetDownloadSizeAsync(transitionLabel, ct);
                        if (!transitionSizeResult.IsError)
                        {
                            transitionLocation = transitionSizeResult.Value.Item2.FirstOrDefault();
                        }
                    }

                    if (transitionLocation is not null)
                    {
                        return FastResult<IResourceLocation>.Ok(transitionLocation);
                    }

                    return FastResult<IResourceLocation>.Fail("Transition.NotFound");
                };
            }

            public DownloadLocationsPhase DownloadLocations<TState>(
                TState state,
                Func<TState, CancellationToken, UniTask<FastResult<Void>>> func
            )
            {
                return new DownloadLocationsPhase(ref _builder);
            }
        }

        public struct DownloadLocationsPhase
        {
            private readonly ToScenePlanBuilder _builder;

            public DownloadLocationsPhase(ref ToScenePlanBuilder builder)
            {
                _builder = builder;
            }

            public async UniTask<FastResult<ToScenePlanCommand>> BuildAsync(CancellationToken ct = default)
            {
                var catalogResult = await (_builder._onCheckCatalog?.Invoke(ct)
                                           ?? UniTask.FromResult(FastResult.Failure));
                if (catalogResult.IsError(out FastResult<ToScenePlanCommand> fail))
                {
                    return fail;
                }

                return FastResult<ToScenePlanCommand>.Ok(new ToScenePlanCommand());
            }
        }
    }
}