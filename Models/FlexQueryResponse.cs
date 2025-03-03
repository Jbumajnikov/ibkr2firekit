using System.Xml.Serialization;

[XmlRoot(ElementName = "FlexQueryResponse")]
public class FlexQueryResponse
{
    [XmlAttribute(AttributeName = "queryName")]
    public string QueryName { get; set; }

    [XmlAttribute(AttributeName = "type")]
    public string Type { get; set; }

    [XmlElement(ElementName = "FlexStatements")]
    public FlexStatements FlexStatements { get; set; }
}
