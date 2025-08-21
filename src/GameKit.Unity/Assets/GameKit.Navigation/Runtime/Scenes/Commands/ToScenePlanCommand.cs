using System;
using System.Collections.Generic;
using GameKit.Assets;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;
using VitalRouter;

namespace GameKit.Navigation.Scenes.Commands
{
    public readonly struct ToScenePlanCommand : ICommand
    {
        public readonly string Label;
        public readonly IList<IResourceLocation> Locations;
        public ByteSize DownloadSize { get; init; }
        public bool IsDownloaded { get; init; }
        public int? TransitionIndex { get; init; }

        public IResourceLocation TransitionLocation =>
            TransitionIndex.HasValue ? Locations[TransitionIndex.Value] : null;

        public ToScenePlanCommand(
            string label,
            IList<IResourceLocation> locations
        )
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                throw new ArgumentException("Label cannot be null or whitespace.", nameof(label));
            }

            if (locations is {Count: 0})
            {
                throw new ArgumentException("Locations cannot be empty.", nameof(locations));
            }

            Label = label;
            Locations = locations;
            DownloadSize = 0;
            IsDownloaded = false;
            TransitionIndex = null;
        }

        public void Deconstruct(
            out string label,
            out IList<IResourceLocation> locations,
            out bool isDownloaded,
            out IResourceLocation transitionLocation
        )
        {
            label = Label;
            locations = Locations;
            isDownloaded = IsDownloaded;
            transitionLocation = TransitionLocation;
        }
    }


}