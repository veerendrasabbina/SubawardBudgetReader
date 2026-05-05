# Subaward Budget Reader

Subaward Budget Reader is a small .NET console application that reads Excel budget spreadsheets and finds subrecipient rows under the budget's other direct costs section.

The app looks for rows labeled like `Subaward: Indiana` or rows where `Subaward:` is in one cell and the name is in the next cell. It prints the subrecipients found in each file, then prints a final alphabetical summary with total subaward amounts grouped by subrecipient.

## Technologies Used

- .NET 10
- ClosedXML for reading Excel files
- xUnit for tests

## How to Run

From the repository root:

```bash
dotnet restore
dotnet build
dotnet run --project src/SubawardBudgetReader.Console
```

By default, the app reads spreadsheets from:

```text
src/SubawardBudgetReader.Console/InputFiles
```

You can also pass a different input folder:

```bash
dotnet run --project src/SubawardBudgetReader.Console -- /path/to/input-folder
```

## How to Run Tests

```bash
dotnet test
```

The unit test reads `SubawardBudgetExample1.xlsx` from the test project and confirms the parser finds exactly these subrecipients:

- Indiana
- Mayo
- Purdue
- Florida

## Folder Structure

```text
SubawardBudgetReader/
|
├── SubawardBudgetReader.sln
├── src/
│   └── SubawardBudgetReader.Console/
│       ├── Program.cs
│       ├── Models/
│       │   └── SubawardEntry.cs
│       ├── Services/
│       │   ├── ISubawardParser.cs
│       │   └── SubawardParser.cs
│       └── InputFiles/
│           ├── SubawardBudgetExample1.xlsx
│           ├── SubawardBudgetExample2.xlsx
│           └── SubawardBudgetExample3.xlsx
├── tests/
│   └── SubawardBudgetReader.Tests/
│       ├── SubawardParserTests.cs
│       └── TestFiles/
│           └── SubawardBudgetExample1.xlsx
└── README.md
```

## Sample Output

```text
============================================================
Subaward Budget Reader
============================================================

File: SubawardBudgetExample1.xlsx

Subrecipients found:
  - Indiana
  - Mayo
  - Purdue
  - Florida

File: SubawardBudgetExample2.xlsx

Subrecipients found:
  - Ecotek
  - Purdue
  - Mayo

File: SubawardBudgetExample3.xlsx

Subrecipients found:
  - U WA
  - U CO
  - Mayo

============================================================
Final Subaward Summary
============================================================
Subrecipient            Total Subaward Amount
------------------------------------------------------------
Ecotek                  $25,000.00
Florida                 $25,000.00
Indiana                 $25,000.00
Mayo                    $65,419.00
Purdue                  $45,000.00
U CO                    $25,000.00
U WA                    $25,000.00
```

## Assumptions I Made

I treated this as a practical import utility for a budget office, so I tried to make the parser flexible without making the project bigger than it needed to be.

- The spreadsheets follow the same general budget format, even if the exact row numbers change from file to file.
- A subrecipient row is identified by the text `Subaward:`. In some files the name appears in the same cell, like `Subaward: Mayo`; in others, `Subaward:` is in one cell and the name is in the next cell.
- The subaward amount belongs on the same row as the `Subaward:` label.
- When a row has both period amounts and a total amount, the total column is the best value to report.
- Blank or non-numeric amount cells should not stop the import. The app treats them as zero and keeps reading the rest of the workbook.

## Questions I Would Ask in Real Work

If this were going into a real team workflow, these are the questions I would want to confirm before calling the behavior final:

- Which column is the official amount for reporting: period total, sponsor total, cost share total, or another budget column?
- Should cost share ever be included in the final subaward total, or should it always be reported separately?
- Can a workbook contain multiple budget worksheets that should all be included, or should some sheets be ignored?
- Are subrecipient names expected to be matched case-insensitively, or should the app preserve and report slight naming differences?
- Should blank or invalid amounts be treated as zero, flagged in the console output, or written to an error report for review?
