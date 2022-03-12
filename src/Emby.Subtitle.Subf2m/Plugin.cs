using Emby.Subtitle.SubF2M.Share;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Drawing;
using System;
using System.IO;

namespace Emby.Subtitle.SubF2M
{
    public class Plugin : BasePlugin, IHasThumbImage
    {
        public override Guid Id => new Guid("54cc7575-3a29-47c1-9255-fc6a8f3c1302");

        public override string Name => Const.PluginName;

        public override string Description => "Subf2m.co subtitle provider";

        public ImageFormat ThumbImageFormat => ImageFormat.Png;

        public Stream GetThumbImage()
        {
            var type = GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".thumb.png");
        }
    }
}
