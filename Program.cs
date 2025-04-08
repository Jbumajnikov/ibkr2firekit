using System.Xml.Serialization;
using System.Globalization;
using ClosedXML.Excel;

public class Ibkr2Firekit
{
    public static void Main(string[] args)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(FlexQueryResponse));

        string filePath = "Templates/ibkr.xml";

        var result = new List<ResultItem>();

        using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
        {
            try
            {
                FlexQueryResponse flexQueryResponse = (FlexQueryResponse)serializer.Deserialize(fileStream);

                Console.WriteLine($"Query Name: {flexQueryResponse.QueryName}");
                Console.WriteLine($"Flex Statements Count: {flexQueryResponse.FlexStatements.Count}");

                var allTrades = flexQueryResponse.FlexStatements.FlexStatement
                    .Where(fs => fs.Trades != null && fs.Trades.Trade != null)
                    .SelectMany(fs => fs.Trades.Trade)
                    .ToList();

                var allCashTransactions = flexQueryResponse.FlexStatements.FlexStatement
                    .Where(fs => fs.CashTransactions != null && fs.CashTransactions.CashTransaction != null)
                    .SelectMany(fs => fs.CashTransactions.CashTransaction)
                    .ToList();

                foreach (var lot in allTrades)
                {
                    if(lot.AssetCategory == "CASH") continue;

                    Console.WriteLine($"\tSymbol: {lot.ListingExchange}:{lot.Symbol}, Type: {lot.BuySell}, Quantity: {lot.Quantity}, Currency: {lot.Currency}, Price: {lot.TradePrice}, DateTime: {lot.ReportDate}");

                    result.Add(new ResultItem
                    {
                        Ticker = $"{lot.ListingExchange}:{lot.Symbol}",
                        Count = lot.Quantity,
                        Price = lot.Currency == "USD" ? lot.TradePrice : lot.TradePrice * lot.FxRateToBase,
                        Currency = "USD",
                        Date = System.DateTime.ParseExact(lot.ReportDate, "yyyyMMdd", CultureInfo.InvariantCulture),
                        Operation = "PurchaseSale",
                        Comment = lot.Quantity < 0 ? $"Sale {lot.Symbol}" : $"Purchase {lot.Symbol}"
                    });
                }

                Dictionary<string, decimal> totalNeddedCashDict = allTrades.Where(ct => ct.AssetCategory != "CASH").GroupBy(ct => ct.Symbol).ToDictionary(ctg => ctg.Key, ctg => ctg.Sum(ct => ct.Quantity * ct.TradePrice * -1) * -1);
                decimal totalNeddedCash = allTrades.Where(ct => ct.AssetCategory != "CASH").Sum(ct => ct.Quantity * ct.TradePrice * -1) * -1;
                totalNeddedCash = Math.Ceiling(totalNeddedCash / 100) * 100;

                result.Add(new ResultItem
                {
                    Price = totalNeddedCash,
                    Currency = "USD",
                    Date = DateTime.ParseExact(allTrades.OrderBy(at => at.DateTime).First().DateTime, "yyyyMMdd;HHmmss", CultureInfo.InvariantCulture).AddDays(-1),
                    Operation = "DepositWithdrawal",
                    Comment = "AUTO Top up brokerage account"
                });

                foreach (var cashTransaction in allCashTransactions)
                {
                    Console.WriteLine($"\tCash Transaction: Symbol: {cashTransaction.Symbol}, Currency: {cashTransaction.Currency}, Amount: {cashTransaction.Amount}, Type: {cashTransaction.Type}, DateTime: {cashTransaction.ReportDate}");

                    var newResultItem = new ResultItem
                    {
                        Price = cashTransaction.Currency == "USD" ? cashTransaction.Amount : cashTransaction.Amount * cashTransaction.FxRateToBase,
                        Currency = "USD",
                        Date = DateTime.ParseExact(cashTransaction.ReportDate, "yyyyMMdd", CultureInfo.InvariantCulture)
                    };

                    bool skip = false;
                    switch (cashTransaction.Type)
                    {
                        case "Deposits/Withdrawals":
                            newResultItem.Operation = "DepositWithdrawal";
                            newResultItem.Comment = "Top up brokerage account";
                            skip = true;
                            break;
                        case "Withholding Tax":
                            newResultItem.Operation = "ProfitLoss";
                            newResultItem.Comment = "Withholding tax";
                            break;
                        case "Dividends":
                        case "Payment In Lieu Of Dividends":
                            newResultItem.Operation = "ProfitLoss";
                            newResultItem.Comment = $"Dividends for {cashTransaction.Symbol}";
                            break;
                        case "Other Fees":
                            newResultItem.Operation = "ProfitLoss";
                            newResultItem.Comment = $"Other fees for {cashTransaction.Symbol}";
                            break;
                        default:
                            throw new Exception($"Unknown Cash Transaction Type: {cashTransaction.Type}");
                    }

                    if (!skip)
                    {
                        result.Add(newResultItem);
                    }
                }

                Console.WriteLine($"Total needed cash: {totalNeddedCash}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during deserialization: {ex.Message}");
            }
        }

        Write(result);
    }

    private static void Write(List<ResultItem> items)
    {
        string outputFilePath = "Templates/firekit.xlsx";

        try
        {
            // Load the Excel template file
            using (var workbook = new XLWorkbook(outputFilePath))
            {
                var worksheet = workbook.Worksheet(3); // Assuming the data sheet is the third one

                int row = 2; // Start writing data from row 2 (after the header)

                // Iterate through the items
                foreach (var item in items.OrderBy(i => i.Date))
                {
                    // Write data to Excel
                    worksheet.Cell(row, 1).Value = item.AssetName;
                    worksheet.Cell(row, 2).Value = item.Ticker;
                    worksheet.Cell(row, 3).Value = item.Count;
                    worksheet.Cell(row, 4).Value = item.Price;
                    worksheet.Cell(row, 5).Value = item.Currency;
                    worksheet.Cell(row, 6).Value = item.Date;
                    worksheet.Cell(row, 7).Value = item.Operation;
                    worksheet.Cell(row, 8).Value = item.Comment;

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                workbook.SaveAs(outputFilePath);
            }

            Console.WriteLine("Excel file created successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
