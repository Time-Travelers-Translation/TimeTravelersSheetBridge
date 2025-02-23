namespace Logic.Domain.GoogleSheetsManagement.Contract
{
    public interface ICodeFlowProvider
    {
        ICodeFlowManager GetCodeFlow();
    }
}
