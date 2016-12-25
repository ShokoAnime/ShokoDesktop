using System.Xml.Serialization;

namespace JMMClient.AutoUpdates
{
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute("shokoclient", Namespace = "", IsNullable = false)]
    public partial class JMMVersions
    {
        [XmlElement("versions")]
        public Versions versions { get; set; }

        [XmlElement("updates")]
        public Updates updates { get; set; }
    }
}
