using System.Xml.Serialization;

public class Trades
{
    [XmlElement(ElementName = "Trade")]
    public List<Trade> Trade { get; set; }
}
