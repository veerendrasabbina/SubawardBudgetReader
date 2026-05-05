using System.Globalization;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using SubawardBudgetReader.Models;

namespace SubawardBudgetReader.Services;

public sealed class SubawardParser : ISubawardParser
{
    private static readonly Regex SubawardRegex = new(
        @"^\s*Subaward:\s*(.*)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public IReadOnlyList<SubawardEntry> ParseFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("A spreadsheet path is required.", nameof(filePath));
        }

        using var workbook = new XLWorkbook(filePath);
        var entries = new List<SubawardEntry>();
        var fileName = Path.GetFileName(filePath);

        foreach (var worksheet in workbook.Worksheets)
        {
            entries.AddRange(ParseWorksheet(worksheet, fileName));
        }

        return entries;
    }

    private static IEnumerable<SubawardEntry> ParseWorksheet(IXLWorksheet worksheet, string fileName)
    {
        foreach (var row in worksheet.RowsUsed())
        {
            foreach (var cell in row.CellsUsed())
            {
                var label = cell.GetString();
                var match = SubawardRegex.Match(label);

                if (!match.Success)
                {
                    continue;
                }

                var name = match.Groups[1].Value.Trim();
                var nameColumn = cell.Address.ColumnNumber;

                if (string.IsNullOrWhiteSpace(name))
                {
                    var nameCell = FindNameCell(row, cell.Address.ColumnNumber + 1);
                    if (nameCell is null)
                    {
                        continue;
                    }

                    name = nameCell.GetString().Trim();
                    nameColumn = nameCell.Address.ColumnNumber;
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                yield return new SubawardEntry
                {
                    FileName = fileName,
                    SubrecipientName = name,
                    Amount = FindAmount(worksheet, row, nameColumn + 1)
                };
            }
        }
    }

    private static IXLCell? FindNameCell(IXLRow row, int firstPossibleColumn)
    {
        return row
            .CellsUsed(cell => cell.Address.ColumnNumber >= firstPossibleColumn)
            .FirstOrDefault(cell =>
            {
                var text = cell.GetString().Trim();
                return text.Length > 0 && !TryReadAmount(cell, out _);
            });
    }

    private static decimal FindAmount(IXLWorksheet worksheet, IXLRow row, int firstAmountColumn)
    {
        var candidates = row
            .CellsUsed(cell => cell.Address.ColumnNumber >= firstAmountColumn)
            .Select(cell => new AmountCandidate(cell.Address.ColumnNumber, TryReadAmount(cell, out var amount), amount))
            .Where(candidate => candidate.HasAmount)
            .ToList();

        if (candidates.Count == 0)
        {
            return 0m;
        }

        var scored = candidates
            .Select(candidate => candidate with
            {
                Score = ScoreAmountColumn(worksheet, row.RowNumber(), candidate.ColumnNumber)
            })
            .ToList();

        var bestScore = scored.Max(candidate => candidate.Score);

        if (bestScore == 0)
        {
            return scored.OrderBy(candidate => candidate.ColumnNumber).First().Amount;
        }

        return scored
            .Where(candidate => candidate.Score == bestScore)
            .OrderByDescending(candidate => candidate.ColumnNumber)
            .First()
            .Amount;
    }

    private static int ScoreAmountColumn(IXLWorksheet worksheet, int rowNumber, int columnNumber)
    {
        var headers = GetHeaderTextAbove(worksheet, rowNumber, columnNumber).ToList();
        var score = 0;

        if (headers.Any(header => header.Contains("total", StringComparison.OrdinalIgnoreCase)))
        {
            score += 100;
        }

        if (headers.Any(header => header.Contains("sponsor", StringComparison.OrdinalIgnoreCase)))
        {
            score += 20;
        }

        if (headers.Any(header => header.Contains("period", StringComparison.OrdinalIgnoreCase)))
        {
            score += 5;
        }

        if (headers.Any(header => header.Contains("cost share", StringComparison.OrdinalIgnoreCase)))
        {
            score -= 70;
        }

        return score;
    }

    private static IEnumerable<string> GetHeaderTextAbove(IXLWorksheet worksheet, int rowNumber, int columnNumber)
    {
        for (var currentRow = 1; currentRow < rowNumber; currentRow++)
        {
            var text = worksheet.Cell(currentRow, columnNumber).GetString().Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                yield return text;
            }
        }
    }

    private static bool TryReadAmount(IXLCell cell, out decimal amount)
    {
        amount = 0m;

        try
        {
            if (cell.TryGetValue<decimal>(out var numericAmount))
            {
                amount = numericAmount;
                return true;
            }
        }
        catch
        {
            // Some formula or formatted cells can be awkward to coerce. Falling back to text keeps one odd cell from stopping the file.
        }

        var text = cell.GetString().Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var cleaned = text
            .Replace("$", string.Empty, StringComparison.Ordinal)
            .Replace(",", string.Empty, StringComparison.Ordinal)
            .Replace("(", "-", StringComparison.Ordinal)
            .Replace(")", string.Empty, StringComparison.Ordinal);

        return decimal.TryParse(cleaned, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.CurrentCulture, out amount)
            || decimal.TryParse(cleaned, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.InvariantCulture, out amount);
    }

    private sealed record AmountCandidate(int ColumnNumber, bool HasAmount, decimal Amount)
    {
        public int Score { get; init; }
    }
}
