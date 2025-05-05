using System.Collections.Generic;
using Jellyfin.Database.Implementations.Interfaces;

namespace Jellyfin.Database.Implementations.Entities.Libraries
{
    /// <summary>
    /// An entity representing a shortVideo.
    /// </summary>
    public class ShortVideo : LibraryItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShortVideo"/> class.
        /// </summary>
        /// <param name="library">The library.</param>
        public ShortVideo(Library library) : base(library)
        {
            ShortVideoMetadata = new HashSet<ShortVideoMetadata>();
        }

        /// <summary>
        /// Gets a collection containing the metadata for this shortVideo.
        /// </summary>
        public virtual ICollection<ShortVideoMetadata> ShortVideoMetadata { get; private set; }
    }
}
