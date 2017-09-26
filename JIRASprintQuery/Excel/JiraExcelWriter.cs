using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Excel;
using Serilog;
using JIRASprintQuery.JIRA;

namespace JIRASprintQuery.Excel
{
    public class JiraExcelWriter
    {
        private readonly ILogger _logger;

        public JiraExcelWriter(ILogger logger)
        {
            _logger = logger;
        }

        public FileInfo Filename { get; private set; }

        public void Write(SprintDetails details, string sprintName, bool launch = true)
        {
            var tickets = details.Tickets;
            Filename = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                $"{sprintName}.xlsx"));
            _logger.Information("Writing {ticketCount} items to {file}.",
                tickets.Count, Filename.Name);

            using (var workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var ticketsWorksheet = workbook.AddWorksheet("All Tickets");
                SetupTicketsWorksheet(details, ticketsWorksheet);
                SetupPivotWorksheet(workbook, "Status");
                SetupPivotWorksheet(workbook, "Assignee");
                AddTicketSummaryRows(workbook, details);
                workbook.SaveAs(Filename.FullName);
            }

            _logger.Information($"Created {Filename.FullName}");

            if (launch)
                Process.Start(Filename.FullName);
        }

        private static void SetupTicketsWorksheet(SprintDetails details, IXLWorksheet worksheet)
        {
            SetupHeaderRow(worksheet, XLColor.LightGoldenrodYellow);

            using (var writer = new CsvWriter(new ExcelSerializer(worksheet)))
            {
                writer.WriteRecords(details.Tickets);
            }

            var columnLetters = new TicketColumnLetters(worksheet);

            var rowCount = worksheet.Rows().Count();
            var pointsSumCell = worksheet.Cell($"{columnLetters.Points}{rowCount + 2}");
            pointsSumCell.FormulaA1 = $"SUM({columnLetters.Points}2:{columnLetters.Points}{rowCount})";
            pointsSumCell.Style.Font.Bold = true;

            worksheet.Columns().AdjustToContents();
            worksheet.Columns(columnLetters.Name).Width = 70;
            worksheet.Column(columnLetters.Url).Hide();
            worksheet.Column(columnLetters.StatusRowText).Hide();
            worksheet.Column(columnLetters.AssigneeRowText).Hide();

            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                var cell = row.Cell(columnLetters.Id);
                var ticketId = cell.Value.ToString();

                if (!string.IsNullOrEmpty(ticketId)) 
                {
                    var ticket = details.Tickets.Single(t => t.Id == ticketId);
                    cell.Hyperlink = new XLHyperlink(ticket.Url, ticket.Url);
                }
            }
        }

        private static void AddTicketSummaryRows(XLWorkbook workbook, SprintDetails details)
        {
            var worksheet = workbook.Worksheet("All Tickets");
            var sum = details.Summary;
            var rowCount = worksheet.Rows().Count();

            SetBoldCell(worksheet, "A", rowCount + 1, "Points");
            SetBoldCell(worksheet, "B", rowCount + 1, $"Total: {sum.TotalPoints}, " +
                $"Done: {sum.TotalDonePoints}, Remaining {sum.TotalPoints - sum.TotalDonePoints}");

            SetBoldCell(worksheet, "A", rowCount + 2, "Tickets");
            SetBoldCell(worksheet, "B", rowCount + 2, $"This sprint: {sum.ItemsThisSprint}, " +
                $"Prior Sprint(s): {sum.ItemsPriorSprint}, " +
                $"Total: {details.Tickets.Count}, " +
                $"Done: {sum.ItemsDone}, " +
                $"Remaining: {sum.ItemsRemaining}");

            SetBoldCell(worksheet, "A", rowCount + 3, "Bugs");
            SetBoldCell(worksheet, "B", rowCount + 3, $"Created: {sum.BugsCreated}, " +
                $"Addressed: {sum.BugsAddressed}");

            SetBoldCell(worksheet, "A", rowCount + 4, "Status");
            var statusSummary = string.Join(", ", 
                sum.StatusCounts.OrderBy(s => s.Key).Select(s => $"{s.Key}: {s.Value}"));
            SetBoldCell(worksheet, "B", rowCount + 4, statusSummary);
        }

        private static void SetupPivotWorksheet(XLWorkbook workbook, string column)
        {
            // Sheets are 1-based. Setup pivot data range.
            var ticketsSheet = workbook.Worksheet(1);
            var ticketsDataTable = ticketsSheet.RangeUsed().AsTable();
            var ticketsHeader = ticketsDataTable.Range(1, 1, 1, 3);
            var dataRange = ticketsSheet.Range(ticketsHeader.FirstCell(), ticketsDataTable.DataRange.LastCell());

            // Add pivot worksheet and pivot table with data range.
            var pivotWorksheet = workbook.AddWorksheet($"{column} Counts");
            var pivot = pivotWorksheet.PivotTables.AddNew($"{column}PivotTable", pivotWorksheet.Cell(1, 1), dataRange);
            pivot.AutofitColumns = true;

            // Setup pivot field
            var pivotField = pivot.RowLabels.Add(column);
            pivotField.ShowBlankItems = false;
            pivotField.Collapsed = true;

            // Configure pivot values
            var pivotValues = pivot.Values.Add(column, $"Count of {column}");
            pivotValues.SummaryFormula = XLPivotSummary.Count;

            // Add nested row label with row details for use when pivot field value is expanded.
            // Bit of a hack to workaround not being able to get multiple row labels on same level (all nested).
            // Assumes {column}RowText column exists - corresponding object property combines multiple key fields in one.
            pivot.RowLabels.Add($"{column}RowText");

            //pivotWorksheet.Columns().AdjustToContents(); // not working
            pivotWorksheet.Column(1).Width = 75;
            pivotWorksheet.Column(2).Width = 25;
        }

        private static void SetBoldCell(IXLWorksheet worksheet, string colLetter, int row, string value)
        {
            var cell = worksheet.Cell($"{colLetter}{row}");
            cell.Style.Font.Bold = true;
            cell.Value = value;
        }

        private static void SetupHeaderRow(IXLWorksheet worksheet, XLColor color)
        {
            var headerRow = worksheet.FirstRow();
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = color;
            worksheet.SheetView.FreezeRows(1);
        }
    }    
}
