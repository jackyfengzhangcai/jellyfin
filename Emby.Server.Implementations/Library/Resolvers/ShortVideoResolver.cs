#nullable disable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Emby.Naming.Common;
using Emby.Naming.Video;
using Jellyfin.Data.Enums;
using Jellyfin.Extensions;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Resolvers;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;

namespace Emby.Server.Implementations.Library.Resolvers
{
    /// <summary>
    /// Class ShortVideoResolver.
    /// </summary>
    public partial class ShortVideoResolver : BaseVideoResolver<ShortVideo>, IMultiItemResolver
    {
        private readonly IImageProcessor _imageProcessor;

        private static readonly CollectionType[] _validCollectionTypes =
        [
            CollectionType.shortvideos
        ];

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortVideoResolver"/> class.
        /// </summary>
        /// <param name="imageProcessor">The image processor.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="namingOptions">The naming options.</param>
        /// <param name="directoryService">The directory service.</param>
        public ShortVideoResolver(IImageProcessor imageProcessor, ILogger<ShortVideoResolver> logger, NamingOptions namingOptions, IDirectoryService directoryService)
            : base(logger, namingOptions, directoryService)
        {
            _imageProcessor = imageProcessor;
        }

        /// <summary>
        /// Gets the priority.
        /// </summary>
        /// <value>The priority.</value>
        public override ResolverPriority Priority => ResolverPriority.Plugin;

        [GeneratedRegex(@"\bsample\b", RegexOptions.IgnoreCase)]
        private static partial Regex IsIgnoredRegex();

        /// <inheritdoc />
        public MultiItemResolverResult ResolveMultiple(
            Folder parent,
            List<FileSystemMetadata> files,
            CollectionType? collectionType,
            IDirectoryService directoryService)
        {
            var result = ResolveMultipleInternal(parent, files, collectionType);

            if (result is not null)
            {
                foreach (var item in result.Items)
                {
                    SetInitialItemValues((ShortVideo)item, null);
                }
            }

            return result;
        }

        /// <summary>
        /// Resolves the specified args.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns>Video.</returns>
        protected override ShortVideo Resolve(ItemResolveArgs args)
        {
            var collectionType = args.GetCollectionType();

            // 如果不是shortvideos类型，直接返回null
            if (collectionType != CollectionType.shortvideos)
            {
                return null;
            }

            // 处理目录
            if (args.IsDirectory)
            {
                if (IsInvalid(args.Parent, collectionType))
                {
                    return null;
                }

                var files = args.GetActualFileSystemChildren().ToList();
                var shortVideo = FindShortVideo(args, args.Path, args.Parent, files, DirectoryService, collectionType, false);

                // 忽略extras
                return shortVideo?.ExtraType is null ? shortVideo : null;
            }

            if (args.Parent is null)
            {
                return base.Resolve(args);
            }

            if (IsInvalid(args.Parent, collectionType))
            {
                return null;
            }

            var item = ResolveVideo<ShortVideo>(args, false);

            // 忽略extras
            if (item?.ExtraType is not null)
            {
                return null;
            }

            if (item is not null)
            {
                item.IsInMixedFolder = true;
            }

            return item;
        }

        private MultiItemResolverResult ResolveMultipleInternal(
            Folder parent,
            List<FileSystemMetadata> files,
            CollectionType? collectionType)
        {
            if (IsInvalid(parent, collectionType))
            {
                return null;
            }

            // 只处理shortvideos类型
            if (collectionType is CollectionType.shortvideos)
            {
                return ResolveVideos<ShortVideo>(parent, files, true, collectionType, false);
            }

            return null;
        }

        private MultiItemResolverResult ResolveVideos<T>(
            Folder parent,
            IEnumerable<FileSystemMetadata> fileSystemEntries,
            bool supportMultiEditions,
            CollectionType? collectionType,
            bool parseName)
            where T : Video, new()
        {
            var files = new List<FileSystemMetadata>();
            var leftOver = new List<FileSystemMetadata>();

            // 遍历每个子文件/文件夹查找视频
            foreach (var child in fileSystemEntries)
            {
                if (child.IsDirectory)
                {
                    leftOver.Add(child);
                }
                else if (!IsIgnoredRegex().IsMatch(child.Name))
                {
                    files.Add(child);
                }
            }

            var videoInfos = files
                .Select(i => VideoResolver.Resolve(i.FullName, i.IsDirectory, NamingOptions, parseName, parent.ContainingFolderPath))
                .Where(f => f is not null)
                .ToList();

            var resolverResult = VideoListResolver.Resolve(videoInfos, NamingOptions, supportMultiEditions, parseName, parent.ContainingFolderPath);

            var result = new MultiItemResolverResult
            {
                ExtraFiles = leftOver
            };

            var isInMixedFolder = resolverResult.Count > 1 || parent?.IsTopParent == true;

            foreach (var video in resolverResult)
            {
                var firstVideo = video.Files[0];
                var path = firstVideo.Path;
                if (video.ExtraType is not null)
                {
                    result.ExtraFiles.Add(files.Find(f => string.Equals(f.FullName, path, StringComparison.OrdinalIgnoreCase)));
                    continue;
                }

                var additionalParts = video.Files.Count > 1 ? video.Files.Skip(1).Select(i => i.Path).ToArray() : Array.Empty<string>();

                var videoItem = new T
                {
                    Path = path,
                    IsInMixedFolder = isInMixedFolder,
                    ProductionYear = video.Year,
                    Name = parseName ? video.Name : firstVideo.Name,
                    AdditionalParts = additionalParts,
                    LocalAlternateVersions = video.AlternateVersions.Select(i => i.Path).ToArray()
                };

                SetVideoType(videoItem, firstVideo);
                Set3DFormat(videoItem, firstVideo);

                result.Items.Add(videoItem);
            }

            result.ExtraFiles.AddRange(files.Where(i => !ContainsFile(resolverResult, i)));

            return result;
        }

        private static bool ContainsFile(IReadOnlyList<VideoInfo> result, FileSystemMetadata file)
        {
            for (var i = 0; i < result.Count; i++)
            {
                var current = result[i];
                for (var j = 0; j < current.Files.Count; j++)
                {
                    if (ContainsFile(current.Files[j], file))
                    {
                        return true;
                    }
                }

                for (var j = 0; j < current.AlternateVersions.Count; j++)
                {
                    if (ContainsFile(current.AlternateVersions[j], file))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool ContainsFile(VideoFileInfo result, FileSystemMetadata file)
        {
            return string.Equals(result.Path, file.FullName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Sets the initial item values.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="args">The args.</param>
        protected override void SetInitialItemValues(ShortVideo item, ItemResolveArgs args)
        {
            base.SetInitialItemValues(item, args);
        }

        /// <summary>
        /// Finds a short video based on file system entries.
        /// </summary>
        /// <returns>ShortVideo.</returns>
        private ShortVideo FindShortVideo(ItemResolveArgs args, string path, Folder parent, List<FileSystemMetadata> fileSystemEntries, IDirectoryService directoryService, CollectionType? collectionType, bool parseName)
        {
            const bool SupportsMultiVersion = true;

            var result = ResolveVideos<ShortVideo>(parent, fileSystemEntries, SupportsMultiVersion, collectionType, parseName) ??
                new MultiItemResolverResult();

            if (result.Items.Count == 1)
            {
                var shortVideo = (ShortVideo)result.Items[0];
                shortVideo.IsInMixedFolder = false;
                shortVideo.Name = Path.GetFileName(shortVideo.ContainingFolderPath);
                return shortVideo;
            }

            return null;
        }

        private bool IsInvalid(Folder parent, CollectionType? collectionType)
        {
            if (parent is not null)
            {
                if (parent.IsRoot)
                {
                    return true;
                }
            }

            if (collectionType is null)
            {
                return false;
            }

            return !_validCollectionTypes.Contains(collectionType.Value);
        }
    }
}
