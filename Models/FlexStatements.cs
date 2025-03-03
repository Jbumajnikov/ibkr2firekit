using System.Xml.Serialization;

public class FlexStatements
{
    [XmlAttribute(AttributeName = "count")]
    public int Count { get; set; }

    [XmlElement(ElementName = "FlexStatement")]
    public List<FlexStatement> FlexStatement { get; set; }
}
