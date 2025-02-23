namespace Logic.Domain.GoogleSheetsManagement.InternalContract.DataClasses
{
    internal class GridRangeData
    {
        public int SheetId { get; set; }
        public int? StartColumnIndex { get; set; }
        public int? EndColumnIndex { get; set; }
        public int? StartRowIndex { get; set; }
        public int? EndRowIndex { get; set; }
    }
}
