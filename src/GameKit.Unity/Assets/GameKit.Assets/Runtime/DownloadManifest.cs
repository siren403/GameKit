using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace GameKit.Assets
{
    public record DownloadManifest
    {
        public ByteSize Size { get; internal init; }
        public readonly IList<IResourceLocation> Locations;

        public bool IsDownloaded => Locations.Any() && Size == 0;

        private DownloadManifest()
        {
        }

        public DownloadManifest(ByteSize size, IList<IResourceLocation> locations)
        {
            Size = size;
            if (locations is { Count: 0 })
            {
                throw new ArgumentException("Locations cannot be empty.", nameof(locations));
            }

            Locations = locations;
        }

        public void Deconstruct(out ByteSize size, out IList<IResourceLocation> locations)
        {
            size = Size;
            locations = Locations;
        }

        public static DownloadManifest operator +(DownloadManifest left, DownloadManifest right)
        {
            return new(
                left.Size + right.Size,
                left.Locations.Concat(right.Locations).ToList()
            );
        }

        public override string ToString()
        {
            return
                $"{nameof(DownloadManifest)}({nameof(Size)}: {Size}, \n{nameof(Locations)}: {string.Join(", \n", Locations)}\n {nameof(IsDownloaded)}: {IsDownloaded})";
        }
    }
}