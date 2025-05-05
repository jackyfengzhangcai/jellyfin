using System.Collections.Generic;

namespace Jellyfin.Database.Implementations.Entities.Libraries
{
    /// <summary>
    /// An entity holding the metadata for a shortVideo.
    /// </summary>
    public class ShortVideoMetadata : ItemMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShortVideoMetadata"/> class.
        /// </summary>
        /// <param name="title">The title or name of the shortVideo.</param>
        /// <param name="language">ISO-639-3 3-character language codes.</param>
        public ShortVideoMetadata(string title, string language) : base(title, language)
        {
        }
    }
}
