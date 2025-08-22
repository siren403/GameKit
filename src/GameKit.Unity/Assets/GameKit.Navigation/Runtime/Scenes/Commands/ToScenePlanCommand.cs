using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameKit.Assets;
using GameKit.Common.Results;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using VitalRouter;

namespace GameKit.Navigation.Scenes.Commands
{
    public readonly struct ToScenePlanCommand : ICommand
    {
        public readonly string Label;
        public readonly DownloadManifest Manifest;
        public int? TransitionIndex { get; init; }

        public IResourceLocation TransitionLocation =>
            TransitionIndex.HasValue ? Manifest.Locations[TransitionIndex.Value] : null;

        public ToScenePlanCommand(string label, DownloadManifest manifest)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                throw new ArgumentException("Label cannot be null or whitespace.", nameof(label));
            }

            Label = label;
            Manifest = manifest;
            TransitionIndex = null;
        }

        public void Deconstruct(
            out string label,
            out DownloadManifest manifest,
            out IResourceLocation transitionLocation
        )
        {
            label = Label;
            manifest = Manifest;
            transitionLocation = TransitionLocation;
        }

        public static async UniTask<FastResult<ToScenePlanCommand>> CreateUsingDownloadManifestAsync(
            string label,
            bool appendTransition = true,
            CancellationToken ct = default
        )
        {
            var mainResult = await AddressableOperations.GetDownloadManifestAsync(label, ct);
            if (mainResult.IsError(out FastResult<ToScenePlanCommand> fail))
            {
                return fail;
            }

            // 트랜지션씬은 하나로만 제한해야 할것 같은데
            // 일단은 라벨로 처리해서 여러개 있어도 하나만 쓰게함
            // 근데 다운로드는 다 해버림
            var mainManifest = mainResult.Value;
            if (appendTransition)
            {
                var transitionLabel = $"{label}:transition";
                DownloadManifest transitionManifest = null;
                var transitionResult = await AddressableOperations.GetDownloadManifestAsync(transitionLabel, ct);
                if (!transitionResult.IsError)
                {
                    transitionManifest = transitionResult.Value;
                }
                else
                {
                    transitionLabel = "/:transition";
                    transitionResult = await AddressableOperations.GetDownloadManifestAsync(transitionLabel, ct);
                    if (!transitionResult.IsError)
                    {
                        transitionManifest = transitionResult.Value;
                    }
                }

                if (transitionManifest is not null)
                {
                    var merged = mainManifest + transitionManifest;
                    return FastResult<ToScenePlanCommand>.Ok(
                        new ToScenePlanCommand(label, mainManifest + transitionManifest)
                        {
                            TransitionIndex = merged.Locations.IndexOf(transitionManifest.Locations.First()),
                        }
                    );
                }
            }

            return FastResult<ToScenePlanCommand>.Ok(
                new ToScenePlanCommand(label, mainManifest)
            );
        }

        public static async UniTask<FastResult<ToScenePlanCommand>> ToDownloadLocationsAsync(
            ToScenePlanCommand command,
            IProgress<DownloadStatus> progress = null,
            CancellationToken ct = default
        )
        {
            var downloadResult =
                await AddressableOperations.DownloadAsync(command.Manifest, progress, ct);
            if (downloadResult.IsError(out FastResult<ToScenePlanCommand> fail))
            {
                return fail;
            }

            return FastResult<ToScenePlanCommand>.Ok(new ToScenePlanCommand(command.Label, downloadResult.Value)
                {
                    TransitionIndex = command.TransitionIndex
                }
            );
        }
    }
}