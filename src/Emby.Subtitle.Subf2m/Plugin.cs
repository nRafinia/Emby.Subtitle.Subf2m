using System;
using System.IO;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Drawing;

namespace Emby.Subtitle.Subf2m
{
    public class Plugin : BasePlugin, IHasThumbImage
    {
        public override Guid Id => new Guid("54cc7575-3a29-47c1-9255-fc6a8f3c1302");

        public override string Name => StaticName;
        public static string StaticName= "Subf2m";

        public override string Description => "Download subtitles from Subf2m.co";

        public ImageFormat ThumbImageFormat => ImageFormat.Png;

        public Stream GetThumbImage()
        {
            var type = GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".thumb.png");
        }
    }
}
