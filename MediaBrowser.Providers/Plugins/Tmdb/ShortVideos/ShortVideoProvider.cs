#pragma warning disable CS1998

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;

namespace MediaBrowser.Providers.Plugins.Tmdb.ShortVideos
{
    /// <summary>
    /// ShortVideo provider powered by TMDb.
    /// </summary>
    public class ShortVideoProvider : IRemoteMetadataProvider<ShortVideo, ShortVideoInfo>, IHasOrder
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILibraryManager _libraryManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortVideoProvider"/> class.
        /// </summary>
        /// <param name="libraryManager">The <see cref="ILibraryManager"/>.</param>
        /// <param name="httpClientFactory">The <see cref="IHttpClientFactory"/>.</param>
        public ShortVideoProvider(
            ILibraryManager libraryManager,
            IHttpClientFactory httpClientFactory)
        {
            _libraryManager = libraryManager;
            _httpClientFactory = httpClientFactory;
        }

        /// <inheritdoc />
        public int Order => 1;

        /// <inheritdoc />
        public string Name => TmdbUtils.ProviderName;

        /// <inheritdoc />
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(ShortVideoInfo searchInfo, CancellationToken cancellationToken)
        {
            return [];
        }

        /// <inheritdoc />
        public async Task<MetadataResult<ShortVideo>> GetMetadata(ShortVideoInfo info, CancellationToken cancellationToken)
        {
            return new MetadataResult<ShortVideo>();
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return _httpClientFactory.CreateClient(NamedClient.Default).GetAsync(url, cancellationToken);
        }
    }
}
