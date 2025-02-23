namespace Logic.Domain.GoogleSheetsManagement.InternalContract.DataClasses
{
    internal class UpdateCellsRequestData
    {
        public RowData[] Rows { get; set; }
        public string Fields { get; set; } = "userEnteredValue";
        public GridRangeData Range { get; set; }
    }
}
