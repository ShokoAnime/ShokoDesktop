namespace Shoko.Desktop.AutoUpdates
{
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Update
    {
        /// <remarks/>
        public string version { get; set; }

        /// <remarks/>
        public string change { get; set; }

        public override string ToString()
        {
            return $"{version} - {change}";
        }

        public long VersionAbs => JMMAutoUpdatesHelper.ConvertToAbsoluteVersion(version);
    }
}
