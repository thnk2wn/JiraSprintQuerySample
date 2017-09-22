using ClosedXML.Excel;
using System.Linq;

namespace JIRASprintQuery.Excel
{
    public class TicketColumnLetters
    {
        public TicketColumnLetters(IXLWorksheet worksheet)
        {
            var table = worksheet.RangeUsed().AsTable();
            Name = GetColumnLetter(table, "Name");
            Points = GetColumnLetter(table, "Points");
            Id = GetColumnLetter(table, "Id");
            Url = GetColumnLetter(table, "Url");
            StatusRowText = GetColumnLetter(table, "StatusRowText");
            AssigneeRowText = GetColumnLetter(table, "AssigneeRowText");
        }

        public string Name { get; }
        public string Points { get; }
        public string Id { get; }
        public string Url { get; }
        public string StatusRowText { get; }
        public string AssigneeRowText { get; }

        private static string GetColumnLetter(IXLTable table, string columnHeader)
        {
            var cell = table.HeadersRow().CellsUsed(c => c.Value.ToString() == columnHeader).FirstOrDefault();
            return cell?.WorksheetColumn().ColumnLetter();
        }
    }
}
