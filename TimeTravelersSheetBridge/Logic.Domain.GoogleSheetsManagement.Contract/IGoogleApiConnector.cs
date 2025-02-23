namespace Logic.Domain.GoogleSheetsManagement.Contract
{
    public interface IGoogleApiConnector
    {
        ISheetManager CreateSheetManager(string sheetId);
    }
}
