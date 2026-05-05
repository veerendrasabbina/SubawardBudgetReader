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

## Assumptions

- The spreadsheets follow the same general budget format.
- Subrecipient rows use `Subaward: {SubRecipientName}`.
- Some files may split `Subaward:` and the subrecipient name into separate cells on the same row.
- The subaward amount is on the same row as the subaward label.
- When a total column is present, the app treats that as the most reasonable official amount column. If sponsor and cost-share columns are both present, it prefers the sponsor total over cost share.
- The app searches dynamically instead of depending on fixed row numbers.
- A file with no subaward rows is still valid.
- Blank or invalid amount cells are treated as zero so one bad cell does not stop the whole import.
- Workbooks may contain multiple worksheets, and the parser checks each worksheet.

## Questions I Would Ask in Real Work

- Which amount column should be treated as the official subaward amount?
- Can a workbook contain multiple sheets that need to be processed?
- Should subrecipient names be matched case-sensitively?
- How should blank or invalid amounts be handled?
- Should final results be sorted alphabetically or by highest amount?
- Should the output also be exported to CSV or Excel?
