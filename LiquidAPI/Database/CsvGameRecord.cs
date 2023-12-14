namespace LiquidAPI;

public record CsvGameRecord
{
    public string Rank { get; set; }
    public string Name { get; set; }
    public string Platform { get; set; }
    public string Year { get; set; }
    public string Genre { get; set; }
    public string Publisher { get; set; }
    public string NA_Sales { get; set; }
    public string EU_Sales { get; set; }
    public string JP_Sales { get; set; }
    public string Other_Sales { get; set; }
    public string Global_Sales { get; set; }
}