using System.Xml.Serialization;

public class FlexStatement
{
    [XmlAttribute(AttributeName = "accountId")]
    public string AccountId { get; set; }

    [XmlAttribute(AttributeName = "fromDate")]
    public string FromDate { get; set; }

    [XmlAttribute(AttributeName = "toDate")]
    public string ToDate { get; set; }

    [XmlAttribute(AttributeName = "period")]
    public string Period { get; set; }

    [XmlAttribute(AttributeName = "whenGenerated")]
    public string WhenGenerated { get; set; }

    [XmlElement(ElementName = "Trades")]
    public Trades Trades { get; set; }

    [XmlElement(ElementName = "CashTransactions")]
    public CashTransactions CashTransactions { get; set; }
}
