class ResultItem
{
    public string AssetName { get; private set; } = "IBKR";
    public string Ticker { get; set; }
    public decimal Count { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; }
    public System.DateTime Date { get; set; }
    public string Operation { get; set; }
    public string Comment { get; set; }
}
