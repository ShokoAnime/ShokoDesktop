namespace Shoko.Desktop.AutoUpdates
{
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Versions
    {

        /// <remarks/>
        public string serverversion { get; set; }

        /// <remarks/>
        public string desktopversion { get; set; }

        public override string ToString()
        {
            return $"Server: {serverversion} --- Desktop: {desktopversion}";
        }

        public long ServerVersionAbs => ShokoAutoUpdatesHelper.ConvertToAbsoluteVersion(serverversion);

        public string ServerVersionFriendly => serverversion;

        public long DesktopVersionAbs => ShokoAutoUpdatesHelper.ConvertToAbsoluteVersion(desktopversion);

        public string DesktopVersionFriendly => desktopversion;
    }
}
