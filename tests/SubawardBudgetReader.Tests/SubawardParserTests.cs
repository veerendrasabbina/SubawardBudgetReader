using SubawardBudgetReader.Services;

namespace SubawardBudgetReader.Tests;

public sealed class SubawardParserTests
{
    [Fact]
    public void Example1_contains_the_expected_subrecipients()
    {
        var parser = new SubawardParser();
        var filePath = Path.Combine(AppContext.BaseDirectory, "TestFiles", "SubawardBudgetExample1.xlsx");

        var entries = parser.ParseFile(filePath);

        var names = entries.Select(entry => entry.SubrecipientName).ToList();
        Assert.Equal(["Indiana", "Mayo", "Purdue", "Florida"], names);
    }
}
