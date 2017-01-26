using System.Xml.Serialization;

namespace Shoko.Desktop.AutoUpdates
{
    /// <remarks/>
    [XmlType(AnonymousType = true)]
    [XmlRoot("shokoclient", Namespace = "", IsNullable = false)]
    public partial class JMMVersions
    {
        [XmlElement("versions")]
        public Versions versions { get; set; }

        [XmlElement("updates")]
        public Updates updates { get; set; }
    }
}
