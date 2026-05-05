namespace SubawardBudgetReader.Models;

public sealed class SubawardEntry
{
    public string FileName { get; init; } = string.Empty;
    public string SubrecipientName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
}
