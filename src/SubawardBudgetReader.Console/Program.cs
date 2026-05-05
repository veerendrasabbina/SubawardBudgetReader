using System.Globalization;
using SubawardBudgetReader.Models;
using SubawardBudgetReader.Services;

var inputFolder = FindInputFolder(args);
ISubawardParser parser = new SubawardParser();

PrintHeader("Subaward Budget Reader");

if (!inputFolder.Exists)
{
    Console.WriteLine($"Input folder was not found: {inputFolder.FullName}");
    return;
}

var files = inputFolder
    .EnumerateFiles("*.xlsx")
    .OrderBy(file => file.Name, StringComparer.OrdinalIgnoreCase)
    .ToList();

if (files.Count == 0)
{
    Console.WriteLine($"No .xlsx files were found in: {inputFolder.FullName}");
    return;
}

var allEntries = new List<SubawardEntry>();

foreach (var file in files)
{
    Console.WriteLine();
    Console.WriteLine($"File: {file.Name}");
    Console.WriteLine();

    try
    {
        var entries = parser.ParseFile(file.FullName).ToList();
        allEntries.AddRange(entries);

        Console.WriteLine("Subrecipients found:");
        if (entries.Count == 0)
        {
            Console.WriteLine("  No subrecipients found.");
            continue;
        }

        foreach (var name in entries.Select(entry => entry.SubrecipientName).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine($"  - {name}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Subrecipients found:");
        Console.WriteLine($"  Could not read this file: {ex.Message}");
    }
}

Console.WriteLine();
PrintHeader("Final Subaward Summary");
PrintSummary(allEntries);

static DirectoryInfo FindInputFolder(string[] args)
{
    if (args.Length > 0)
    {
        return new DirectoryInfo(Path.GetFullPath(args[0]));
    }

    var candidates = new[]
    {
        Path.Combine(AppContext.BaseDirectory, "InputFiles"),
        Path.Combine(Environment.CurrentDirectory, "InputFiles"),
        Path.Combine(Environment.CurrentDirectory, "src", "SubawardBudgetReader.Console", "InputFiles")
    };

    var folder = candidates.FirstOrDefault(Directory.Exists) ?? candidates[0];
    return new DirectoryInfo(folder);
}

static void PrintHeader(string title)
{
    Console.WriteLine(new string('=', 60));
    Console.WriteLine(title);
    Console.WriteLine(new string('=', 60));
}

static void PrintSummary(IEnumerable<SubawardEntry> entries)
{
    var summary = entries
        .GroupBy(entry => entry.SubrecipientName, StringComparer.OrdinalIgnoreCase)
        .Select(group => new
        {
            Name = group.First().SubrecipientName,
            Total = group.Sum(entry => entry.Amount)
        })
        .OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
        .ToList();

    if (summary.Count == 0)
    {
        Console.WriteLine("No subaward entries were found.");
        return;
    }

    Console.WriteLine($"{ "Subrecipient",-24}Total Subaward Amount");
    Console.WriteLine(new string('-', 60));

    foreach (var entry in summary)
    {
        Console.WriteLine($"{entry.Name,-24}{entry.Total.ToString("C", CultureInfo.CurrentCulture)}");
    }
}
