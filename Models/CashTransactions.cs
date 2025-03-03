using System.Xml.Serialization;

public class CashTransactions
{
    [XmlElement(ElementName = "CashTransaction")]
    public List<CashTransaction> CashTransaction { get; set; }
}
