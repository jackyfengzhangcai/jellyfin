#nullable disable

#pragma warning disable CS1591

using MediaBrowser.Controller.Providers;

namespace MediaBrowser.Controller.Entities
{
    /// <summary>
    /// Class ShortVideo.
    /// </summary>
    public class ShortVideo : Video, IHasLookupInfo<ShortVideoInfo>
    {
        public ShortVideoInfo GetLookupInfo()
        {
            var info = GetItemLookupInfo<ShortVideoInfo>();
            return info;
        }

        public override bool RequiresRefresh()
        {
            return false;
        }
    }
}
