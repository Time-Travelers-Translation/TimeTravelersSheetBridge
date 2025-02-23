namespace Logic.Domain.GoogleSheetsManagement.InternalContract.DataClasses
{
    internal class SheetPropertiesResponseData
    {
        public int SheetId { get; set; }
        public string Title { get; set; }
        public int Index { get; set; }
        public string SheetType { get; set; }
        public GridPropertiesResponseData GridProperties { get; set; }
        public ColorResponseData TabColor { get; set; }
    }
}
