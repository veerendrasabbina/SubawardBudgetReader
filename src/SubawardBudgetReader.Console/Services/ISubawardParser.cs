using SubawardBudgetReader.Models;

namespace SubawardBudgetReader.Services;

public interface ISubawardParser
{
    IReadOnlyList<SubawardEntry> ParseFile(string filePath);
}
